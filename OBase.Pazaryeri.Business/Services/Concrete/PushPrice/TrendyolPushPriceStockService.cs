using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.DataAccess.Services.Abstract.PriceStock;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Getir.PriceStock;
using OBase.Pazaryeri.Domain.Dtos.Trendyol;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Services.Concrete.PushPrice
{
    public class TrendyolPushPriceStockService : ITrendyolPushPriceStockService
    {
        #region Variables

        private readonly ApiDefinitions _apiDefinition;
        private readonly IPriceStockDalService _priceStockDalService;
        private readonly IOptions<AppSettings> _options;
        private readonly IBaseService _baseService;
        private readonly ITrendyolClient _trendyolClient;
        private readonly string trendyolLogFile = nameof(CommonEnums.PazarYerleri.Trendyol);
        private readonly IServiceScopeFactory _serviceScopeFactory;

        #endregion

        #region Ctor

        public TrendyolPushPriceStockService(IOptions<AppSettings> options, IPriceStockDalService priceStockDalService, IBaseService baseService, ITrendyolClient trendyolClient, IServiceScopeFactory serviceScopeFactory)
        {
            _options = options;
            _apiDefinition = options.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Trendyol);
            _priceStockDalService = priceStockDalService;
            _baseService = baseService;
            _trendyolClient = trendyolClient;
            _serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Methods

        public async Task PushPriceStockAsync(Dictionary<string, string> properties, JobType executionType)
        {
            int NumberofThreads = int.Parse(properties[TyConstants.Parameters.NumberofThreads]);
            int NumberofProducts = int.Parse(properties[TyConstants.Parameters.NumberofProducts]);
            int NumberofTries = int.Parse(properties[TyConstants.Parameters.NumberofTries]);
            int NumberofSQLRows = int.Parse(properties[TyConstants.Parameters.NumberofSQLRows]);
            string merchantNo = _apiDefinition.Merchantno;
            executionType = JobType.All;

            List<long> refIds = await _priceStockDalService.GetAvailableRefIdsByMerchantNoAsync(executionType, merchantNo);
            Logger.Information("TrendyolService > PushPriceStock > Number of Jobs Available for Execution Type {ExecutionType}: {count}", fileName: trendyolLogFile, executionType, refIds?.Count ?? 0);

            foreach (long refId in refIds)
            {
                var merchants = await _priceStockDalService.GetStoreNoListAsync(refId, _apiDefinition.Merchantno);
                List<Task> merchTasks = new();
                var dataLst = new ConcurrentBag<CustomResult>();

                Logger.Information("TrendyolService > PushPriceStock > Execution Type: {ExecutionType}, Number of Merchants: {count}", fileName: trendyolLogFile, executionType, merchants?.Count ?? 0);



                if (merchants.Count == 0)
                    continue;

                foreach (var storeNoItem in merchants)
                {
                    merchTasks.Add(Task.Run(async () =>
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var priceStockService = scope.ServiceProvider.GetRequiredService<IPriceStockDalService>();

                        var data = new CustomResult() { MerchantId = storeNoItem, CreationDateTime = DateTime.Now };
                        data.RefId = refId;
                        data.NumberofProducts = NumberofProducts;
                        data.ThreadId = Environment.CurrentManagedThreadId;
                        data.InternalThreadCount = 0;
                        data.FailedThreadCount = 0;
                        data.SuccessfulThreadCount = 0;
                        data.ExecutionType = executionType;
                        data.MerchantNo = _apiDefinition.Merchantno;
                        data.MerchantId = storeNoItem;
                        data.CreationDateTime = DateTime.Now;
                        data = await PostProductByMerchantWithVendor(priceStockService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("TrendyolService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {data}", fileName: trendyolLogFile, executionType, data);
                    }));
                }
                await Task.WhenAll(merchTasks);

                var totalCount = dataLst?.Count() ?? 0;
                var FailedCount = dataLst?.Count(f => f.HasErrors) ?? 0;
                var SuccessCount = dataLst?.Count(f => !f.HasErrors) ?? 0;
                var HasErrors = (totalCount != SuccessCount) ? Character.E : Character.H;
                var HasErrorStr = HasErrors == Character.E ? "partially" : "successfully";
                var srvLogErrorPhrase = (totalCount != SuccessCount) ? ", LOG TABLOLARINI KONTROL EDINIZ" : "";
                var srvLogError = $"TOPLAM BIRIM SAY.: {totalCount}, BASARILI BIRIM SAY.: {SuccessCount}, BASARISIZ BIRIM SAY.: {FailedCount}{srvLogErrorPhrase}";
                var SrvLogStatus = (totalCount != SuccessCount) ? "HATA" : "TAMAMLANDI";

                var pazarYeriJobResult = await _priceStockDalService.GetPazarYeriJobResultByRefIdAsync(refId);
                if (pazarYeriJobResult != null)
                {
                    pazarYeriJobResult.ThreadSize = NumberofProducts;
                    pazarYeriJobResult.NumberOfThreads = dataLst?.Count ?? 0;
                    pazarYeriJobResult.HasErrors = HasErrors;
                    pazarYeriJobResult.HasSent = Character.E;

                    var updateResponse = await _priceStockDalService.UpdatePazarYeriJobResultAsync(pazarYeriJobResult);
                    if (!updateResponse)
                        Logger.Warning("TrendyolService > PushPriceStock > Execution Type: {ExecutionType}, Cannot update Repo for Ref_Id: {refId}", fileName: trendyolLogFile, executionType, refId);
                }

                await _priceStockDalService.UpdateServiceLog(refId, SrvLogStatus, srvLogError);

                if (totalCount != SuccessCount)
                    Logger.Warning("TrendyolService > PushPriceStock > Execution Type: {ExecutionType}, Task Result Data: {count}", fileName: trendyolLogFile, executionType, (dataLst?.Count ?? 0));

            }
        }

        public async Task<List<CustomResult>> PushPriceStockOnlyStockAsync(Dictionary<string, string> properties, long refId)
        {
            int NumberofThreads = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofThreads]);
            int NumberofProducts = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofProducts]);
            int NumberofTries = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofTries]);
            int NumberofSQLRows = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofSQLRows]);
            string merchantNo = _apiDefinition.Merchantno;
            JobType executionType = JobType.OnlyStock;

            ConcurrentBag<CustomResult> dataLst = new();
            Logger.Information("TrendyolService > PushPriceStock is running for ExecutionType: {ExecutionType}", fileName: trendyolLogFile, executionType);
            try
            {
                var merchants = await _priceStockDalService.GetStoreNoListAsync(refId, _apiDefinition.Merchantno);
                if (!merchants.Any())
                {
                    Logger.Information($"TrendyolService > PushPriceStock > Execution Type: {executionType} | no merchants found for refId:{refId}", fileName: trendyolLogFile);
                    return dataLst.ToList();
                }
                List<Task> merchTasks = new();
                Logger.Information("TrendyolService > PushPriceStock > Execution Type: {ExecutionType}, Number of Merchants: {Count}", fileName: trendyolLogFile, executionType, merchants.Count);

                foreach (var storeNoItem in merchants)
                {
                    merchTasks.Add(Task.Run(async () =>
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var priceStockService = scope.ServiceProvider.GetRequiredService<IPriceStockDalService>();

                        var data = new CustomResult() { MerchantId = storeNoItem, CreationDateTime = DateTime.Now };
                        data.RefId = refId;
                        data.NumberofProducts = NumberofProducts;
                        data.ThreadId = Environment.CurrentManagedThreadId;
                        data.InternalThreadCount = 0;
                        data.FailedThreadCount = 0;
                        data.SuccessfulThreadCount = 0;
                        data.ExecutionType = executionType;
                        data.MerchantNo = merchantNo;
                        data.MerchantId = storeNoItem;
                        data.CreationDateTime = DateTime.Now;
                        data = await PostProductByMerchantWithVendor(priceStockService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("TrendyolService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {@data}", fileName: trendyolLogFile, executionType, data);
                    }));
                }
                await Task.WhenAll(merchTasks);

            }
            catch (Exception ex)
            {
                Logger.Error("TrendyolService > PushPriceStock, Execution Type: {ExecutionType}, Exception occurred with Exception: {ex.Message}", fileName: trendyolLogFile, executionType, ex);
            }
            Logger.Information("TrendyolService > PushPriceStock is completed for ExecutionType: {ExecutionType}", fileName: trendyolLogFile, executionType);
            return dataLst.ToList();
        }

        public async Task VerifyPriceStockAsync(Dictionary<string, string> properties)
        {
            var executionType = JobType.VerifyPriceAndStock;

            var verifiableslst = await _priceStockDalService.GetAvailableVerifiablesAsync(_apiDefinition.Merchantno);

            List<IGrouping<string, MerchantVerify>> jobs = verifiableslst.GroupBy(f => f.GUID).ToList();

            Logger.Information("TrendyolService > VerifyPriceStock > Number of Guids Available for Execution Type {executionType}: {count}", fileName: trendyolLogFile, executionType, jobs.Count);
            List<Task> merchTasks = new();
            var dataLst = new ConcurrentBag<CustomVerifyResult>();

            foreach (var jobitm in jobs)
            {
                merchTasks.Add(Task.Run(async () =>
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var priceStockService = scope.ServiceProvider.GetRequiredService<IPriceStockDalService>();

                    var data = new CustomVerifyResult() { Guid = jobitm.Key, CreationDateTime = DateTime.Now };
                    data.Guid = jobitm.Key;
                    data.MerchantId = jobitm.Select(s => s.PAZAR_YERI_BIRIM_NO).Distinct().SingleOrDefault();
                    data.MerchantVerifies = jobitm.ToList();
                    data.DetailsCount = 0;
                    data.FailedCount = 0;
                    data.ProcessingCount = 0;
                    data.SuccessfulCount = 0;
                    data.ExecutionType = executionType;
                    data = await TY_VerifyPostsByGuidAsync(priceStockService, data, executionType);
                    dataLst.Add(data);
                    Logger.Warning("TrendyolService > TY_VerifyPriceStock > Execution Type: {executionType}, Individual Task Result Data: {Guid}", fileName: trendyolLogFile, executionType, data.Guid);
                }));
            }

            await Task.WhenAll(merchTasks);

            var totalCount = dataLst?.Count ?? 0;
            var SuccessCount = dataLst?.Count(f => !f.HasErrors) ?? 0;

            if (totalCount != SuccessCount)
                Logger.Warning("TrendyolService > TY_VerifyPriceStock > Execution Type:{executionType}, Task Result Data: {Count}", fileName: trendyolLogFile, executionType, dataLst.Count);
        }



        #endregion

        #region Utilities

        private async Task<CustomResult> PostProductByMerchantWithVendor(IPriceStockDalService priceStockDalService, CustomResult data)
        {
            data.StartDateTime = DateTime.Now;
            List<TrendyolPushPriceStockRequestMainDto> requests = new();

            #region Outer Exception
            try
            {
                var jobdetails = await priceStockDalService.GetJobResultDetailsByIdMerchAsync(data.RefId, data.MerchantId, data.MerchantNo,satisFiyatControl:true);
                data.DetailsCount = jobdetails.Count;

                jobdetails = _baseService.CalculateThreads(data.DetailsCount, data.NumberofProducts, jobdetails.ToList(), data.RefId, data.MerchantId, data.ThreadId).ToList();
                data.InternalThreadCount = jobdetails.Count;

                if (!jobdetails.Any())
                {
                    Logger.Warning($"TrendyolService > TY_PostProductByMerchantWithVendor > Execution Type: {data.ExecutionType}, No Details found for RefId: {data.RefId}, MerchantId: {data.MerchantId}", fileName: trendyolLogFile);
                    return data;
                }


                #region Conventional For Loop
                for (int i = 1; i <= jobdetails.Max(f => f.ThreadNo); i++)
                {
                    TrendyolPushPriceStockRequestMainDto req = new()
                    {
                        Guid = string.Empty,
                        APIResponse = string.Empty,
                        RequestFailed = false,
                        ResultException = string.Empty,
                        HttpStatusCode = HttpStatusCode.Unused,
                        DetailsCount = 0,
                        Thread_No = i
                    };

                    var tmpdetails = jobdetails.Where(f => f.ThreadNo == i);
                    req.DetailsCount = tmpdetails.Count();

                    req.RequestItem = new()
                    {
                        Items = new()
                    };

                    foreach (var det in tmpdetails)
                    {
                        TrendyolPushPriceStockRequestItemDto requestItem = new()
                        {
                            #region WhichHasBeenSent
                            SalePrice = det.SatisFiyat ?? 0,
                            ListPrice = det.SatisFiyat ?? 0,
                            Barcode = det.Barkod,
                            Quantity = det.StokMiktar ?? 0,
                            #endregion

                            #region ForReference
                            DetailId = det.DetailId,
                            RefId = det.RefId
                            #endregion
                        };
                        req.RequestItem.Items.Add(requestItem);
                    }
                    ;

                    var requestStr = JsonConvert.SerializeObject(req);

                    try
                    {
                        var result = await _trendyolClient.SendPriceStockAsync(_apiDefinition.SupplierId, req.RequestItem);


                        Logger.Information("TrendyolService > PostProductByMerchant > SendPriceStockAsync Response: {@response}", fileName: trendyolLogFile, result);


                        req.RequestFailed = !(result.ResponseMessage.IsSuccessStatusCode);
                        req.HttpStatusCode = result.ResponseMessage.StatusCode;
                        req.APIResponse = result.StringContent;
                        var resultObject = JsonConvert.DeserializeObject<TrendyolPushPriceStockResponseDto>(result.StringContent);
                        req.Guid = result.ResponseMessage.IsSuccessStatusCode ? resultObject.BatchRequestId : string.Empty;

                        PazarYeriLog logEntry = new()
                        {
                            RefId = data.RefId,
                            ExecutionType = Enum.GetName(typeof(JobType), data.ExecutionType),
                            PazarYeriBirimNo = data.MerchantId,
                            ThreadNo = req.Thread_No,
                            LogType = result.ResponseMessage.IsSuccessStatusCode ? nameof(LogType.AllCompleted) : nameof(LogType.AllFailed),
                            Request = requestStr,
                            Response = $"HTTP StatusCode: {result.ResponseMessage.StatusCode} >  {req.APIResponse}",
                            HasErrors = result.ResponseMessage.IsSuccessStatusCode ? Character.H : Character.E,
                            DetailId = 0,
                            PazarYeriNo = data.MerchantNo,
                            Guid = !req.RequestFailed ? resultObject.BatchRequestId : string.Empty,
                        };
                        Logger.Information("TrendyolService > PostProductByMerchant > PushPriceStockAsync() > logEntry {@logEntry}", trendyolLogFile, logEntry);
                    }
                    catch (Exception ex)
                    {
                        req.RequestFailed = true;
                        req.ResultException = $"TrendyolService > PostProductByMerchant > Execution Type: {data.ExecutionType}, Inner Exception for Ref_Id: {data.RefId}, MerchantId: {data.MerchantId}, ThreadId: {data.ThreadId}, Thread_No: {req.Thread_No} - HTTP: {ex.Message}, with StackTrace: {ex.StackTrace}";

                        PazarYeriLog logEntry = new()
                        {
                            RefId = data.RefId,
                            ExecutionType = Enum.GetName(typeof(JobType), data.ExecutionType),
                            PazarYeriBirimNo = data.MerchantId,
                            ThreadNo = req.Thread_No,
                            LogType = nameof(LogType.AllFailed),
                            Request = requestStr,
                            Response = ex.Message,
                            HasErrors = Character.E,
                            DetailId = 0,
                            PazarYeriNo = data.MerchantNo,
                            Guid = string.Empty
                        };
                        await priceStockDalService.InsertPazarYeriLogAsync(logEntry);
                    }
                    requests.Add(req);
                }
                #endregion

                //For Performance Logging
                data.RequestCompletionDateTime = DateTime.Now;
                data.FailedThreadCount = requests.Count(f => f.RequestFailed);
                data.SuccessfulThreadCount = requests.Count(f => !f.RequestFailed);

                #region Pazaryeri Job Result Details Update
                Stopwatch stopwatch = Stopwatch.StartNew();
                foreach (var requestItem in requests)
                {
                    List<PazarYeriJobResultDetails> updatedDetailList = new();
                    foreach (var individualItem in requestItem.RequestItem.Items)
                    {
                        long refId = individualItem.RefId;
                        long detailId = individualItem.DetailId;
                        var detail = jobdetails.FirstOrDefault(f => f.RefId == refId && f.DetailId == detailId);

                        if (detail == null) continue;

                        detail.Guid = requestItem.Guid;
                        detail.ThreadNo = requestItem.Thread_No;
                        detail.HasErrors = requestItem.RequestFailed ? Character.E : Character.H;
                        detail.HasSent = Character.E;
                        updatedDetailList.Add(detail);
                    }
                    try
                    {
                        if (updatedDetailList.Any())
                            await priceStockDalService.UpdatePazarYeriJobResultDetailRangeAsync(updatedDetailList);

                        Logger.Information("TrendyolService > PostProductByMerchant > Execution Type: {ExecutionType}, Requests updates successfully in Repo Db/for RefId: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}", fileName: trendyolLogFile, data?.ExecutionType, data?.RefId, data?.MerchantId, requestItem?.Thread_No);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("TrendyolService > PostProductByMerchant > Execution Type: {ExecutionType}, Cannot update requests successfully in Db/Repo for RefId: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId},Exception : {ex}", fileName: trendyolLogFile, data?.ExecutionType, data?.RefId, data?.MerchantId, requestItem?.Thread_No, ex);
                    }
                }
                stopwatch.Stop();
                Logger.Information("TrendyolService UpdatePazarYeriJobResultDetailRangeAsync finished in {elapsedTime}ms.", trendyolLogFile, stopwatch.ElapsedMilliseconds);
                #endregion

                data.CompletionDateTime = DateTime.Now;
                if (requests.Count(f => !f.RequestFailed) != requests.Count)
                {
                    data.HasErrors = true;
                    Logger.Warning("TrendyolService > PostProductByMerchant > Execution Type: {ExecutionType}, Completed partially for MerchantId: {MerchantId} for Execution Data: {@data}", fileName: trendyolLogFile, data?.ExecutionType, data?.MerchantId, data);
                }
                else
                {
                    data.HasErrors = false;
                    Logger.Information("TrendyolService > PostProductByMerchant > Execution Type: {ExecutionType}, Completed successfully for MerchantId: {MerchantId} for Execution Data: {@data}", fileName: trendyolLogFile, data?.ExecutionType, data?.MerchantId, data);
                }
            }
            catch (Exception ex)
            {
                data.CompletionDateTime = DateTime.Now;
                data.HasErrors = true;
                data.ExceptionsString = ex.Message;
                Logger.Error("TrendyolService > PostProductByMerchant > Execution Type: data.ExecutionType, Failed with Exception for MerchantId: {MerchantId} for Execution Data:{@data}, Exception: {exception}", trendyolLogFile, data.ExecutionType, data.MerchantId, data, ex);
            }
            #endregion

            return data;
        }

        private async Task<CustomVerifyResult> TY_VerifyPostsByGuidAsync(IPriceStockDalService priceStockDalService, CustomVerifyResult data, JobType ExecutionType)
        {
            Logger.Information("TYService > TY_VerifyPostsByGuid > Verifiable Data: MerchantVerifies Count => {Count}", fileName: trendyolLogFile, data.MerchantVerifies?.Count ?? 0);

            data!.StartDateTime = DateTime.Now;
            string guid = data.Guid;
            string merchantId = _apiDefinition.SupplierId;

            try
            {
                var result = await _trendyolClient.GetBatchRequestResultAsync(merchantId, guid);

                data.RequestCompletionDateTime = DateTime.Now;

                List<PazarYeriLog> pazarYeriLogs = new();
                List<PazarYeriJobResultDetails> detailPazarYeriJobList = new List<PazarYeriJobResultDetails>();
                PazarYeriLog logEntry;
                if (result.ResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var results = JsonConvert.DeserializeObject<TrendyolVerifyPriceStockResponseDto>(result.StringContent);
                    var requestItems = results;
                    var dataItems = data.MerchantVerifies;

                    if (requestItems.Items != null && requestItems.Items.Any())
                    {
                        foreach (var requestItm in requestItems.Items)
                        {
                            var dataItem = dataItems.FirstOrDefault(f => f.BARKOD == requestItm.RequestItem.Barcode);

                            if (dataItem != null)
                            {
                                requestItm.RequestItem.RefId = dataItem.REF_ID;
                                requestItm.RequestItem.DetailId = dataItem.DETAIL_ID;
                                requestItm.RequestItem.Thread_No = dataItem.THREAD_NO;
                            }
                        }
                    }

                    if (requestItems != null)
                    {
                        var tmp = dataItems.FirstOrDefault(f => f.GUID == requestItems.BatchRequestId);

                        if (tmp != null)
                        {
                            requestItems.RefId = tmp.REF_ID;
                            requestItems.DetailId = tmp.DETAIL_ID;
                            requestItems.Thread_No = tmp.THREAD_NO;
                        }

                    }

                    try
                    {

                        var pazarYeriJobResultDetails = await priceStockDalService.GetPazarYeriJobResultDetailsAsync(requestItems.RefId, requestItems.Thread_No, data.Guid, data.MerchantId);

                        if (requestItems.Items != null && requestItems.Items.Any())
                        {
                            foreach (var item in pazarYeriJobResultDetails)
                            {
                                #region Update PazaryeriJobResultDetails
                                if (pazarYeriJobResultDetails.Any())
                                {
                                    if (pazarYeriJobResultDetails?.Any(f => f.DetailId == item.DetailId) ?? false)
                                    {
                                        item.HasVerified = Character.H;
                                        item.HasErrors = Character.E;

                                    }
                                    else
                                    {
                                        item.HasVerified = Character.E;
                                        item.HasErrors = Character.H;
                                    }
                                    detailPazarYeriJobList.Add(item);
                                }
                                #endregion
                                #region Insert Pazaryeri Log
                                if (item.HasErrors == Character.E)
                                {
                                    logEntry = new()
                                    {
                                        RefId = item.RefId,
                                        ExecutionType = data.ExecutionType.ToString(),
                                        PazarYeriBirimNo = data.MerchantId,
                                        ThreadNo = item.ThreadNo,
                                        LogType = requestItems.Status,
                                        Request = $"MerchantId: {merchantId}, Guid: {guid}",
                                        Response = JsonConvert.SerializeObject(requestItems),
                                        HasErrors = Character.E,
                                        DetailId = item.DetailId,
                                        PazarYeriNo = PazarYeri.Trendyol,
                                        Guid = data.Guid
                                    };
                                    await priceStockDalService.InsertPazarYeriLogAsync(logEntry);
                                }
                                #endregion
                            }
                            if (detailPazarYeriJobList.Any())
                                await priceStockDalService.UpdatePazarYeriJobResultDetailRangeVerifiedAsync(detailPazarYeriJobList).ConfigureAwait(false);

                            data.HasErrors = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        data.HasErrors = true;
                        Logger.Error("TYService > UpdateRefIdDetailIdByGuid, MerchantId: {merchantId}, Guid: {guid}, ExecutionType: {ExecutionType}, Exception: {exception}, Requests: {@requestItems}", fileName: trendyolLogFile, merchantId, guid, ExecutionType, ex, requestItems ?? null);
                    }
                    data.CompletionDateTime = DateTime.Now;

                }
                else
                {

                    var requestItems = new GetRequestsByIdRespDto();

                    foreach (var dataitm in data.MerchantVerifies)
                    {
                        var requestItem = new BaseRequestIdResponseItem
                        {
                            DetailId = dataitm.DETAIL_ID,
                            ErrorMessages = new List<string> { result.StringContent },
                            RefId = dataitm.REF_ID,
                            Status = result.ResponseMessage.StatusCode.ToString(),
                            ThreadNo = dataitm.THREAD_NO
                        };
                        requestItems.RequestItems.Add(requestItem);
                    }
                    try
                    {
                        var pazarYeriJobResultDetails = await priceStockDalService.GetPazarYeriJobResultDetailsAsync(requestItems.RefId, requestItems.Thread_No, data.Guid, data.MerchantId);
                        var HasError = (requestItems.Status == null || requestItems.Status.ToUpper() == "SUCCESS") ? Character.H : Character.E;
                        var HasVerified = requestItems.Status == null ? Character.C : requestItems.Status[..1];

                        if (requestItems.RequestItems != null)
                        {
                            foreach (var itm in requestItems.RequestItems)
                            {
                                HasError = itm.ErrorMessages == null && requestItems.Status == "Done" ? Character.H : Character.E;

                                var detail = await priceStockDalService.GetPazarYeriJobResultDetailAsync(requestItems.RefId, requestItems.DetailId, requestItems.Thread_No, data.Guid, data.MerchantId);

                                if (detail == null)
                                    continue;

                                detail.HasVerified = HasVerified;
                                detail.HasErrors = HasError;

                                detailPazarYeriJobList.Add(detail);

                                logEntry = new PazarYeriLog
                                {
                                    RefId = requestItems.RefId,
                                    ExecutionType = data.ExecutionType.ToString(),
                                    PazarYeriBirimNo = data.MerchantId,
                                    ThreadNo = requestItems.Thread_No,
                                    LogType = requestItems.Status,
                                    Request = $"MerchantId: {merchantId}, Guid: {guid}",
                                    Response = JsonConvert.SerializeObject(itm.ErrorMessages),
                                    HasErrors = Character.E,
                                    DetailId = requestItems.DetailId,
                                    PazarYeriNo = PazarYeri.Trendyol,
                                    Guid = data.Guid
                                };

                                await priceStockDalService.InsertPazarYeriLogAsync(logEntry);
                            }
                            if (detailPazarYeriJobList.Any())
                                await priceStockDalService.UpdatePazarYeriJobResultDetailRangeVerifiedAsync(detailPazarYeriJobList).ConfigureAwait(false);
                        }
                        data.HasErrors = false;
                    }
                    catch (Exception ex)
                    {
                        data.HasErrors = true;
                        Logger.Error("TrendyolService > TY_VerifyPostsByGuid MerchantId: {merchantId}, Guid: {guid}, ExecutionType: {ExecutionType}, Exception: {exception}, Requests: {@requestItems}", fileName: trendyolLogFile, merchantId, guid, ExecutionType, ex, requestItems);
                    }
                    data.CompletionDateTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                data.HasErrors = true;
                data.CompletionDateTime = DateTime.Now;
                data.ExceptionString = $"TrendyolService > TY_VerifyPostsByGuid > Execution Type: {ExecutionType}, Exception: {ex.Message}";
                Logger.Error("TrendyolService > TY_VerifyPostsByGuid > Execution Type: {ExecutionType}, Exception: {exception}", fileName: trendyolLogFile, ExecutionType, ex);
            }
            return data;
        }

        #endregion

    }
}
