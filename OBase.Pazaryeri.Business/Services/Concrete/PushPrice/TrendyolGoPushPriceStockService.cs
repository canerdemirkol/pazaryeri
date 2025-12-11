using System.Net;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.DataAccess.Services.Abstract.PriceStock;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace OBase.Pazaryeri.Business.Services.Concrete.PushPrice
{
    public class TrendyolGoPushPriceStockService : ITrendyolGoPushPriceStockService
    {
        #region Variables

        private readonly IPriceStockDalService _priceStockDalService;
        private readonly IBaseService _baseService;
        private readonly IOptions<AppSettings> _options;
        private readonly ITrendyolGoClient _trendyolGoClient;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.TrendyolGo);
        private readonly IServiceScopeFactory _serviceScopeFactory;

        #endregion

        #region Ctor

        public TrendyolGoPushPriceStockService(IPriceStockDalService priceStockDalService, IOptions<AppSettings> options, ITrendyolGoClient trendyolGoClient, IBaseService baseService, IServiceScopeFactory serviceScopeFactory)
        {
            _priceStockDalService = priceStockDalService;
            _baseService = baseService;
            _options = options;
            _trendyolGoClient = trendyolGoClient;
            _apiDefinition = _options.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.TrendyolGo);
            _serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Methods

        public async Task PushPriceStock(Dictionary<string, string> properties, JobType executionType)
        {
            int NumberofProducts = int.Parse(properties[TyGoConstants.Parameters.NumberofProducts]);
            string merchantNo = _apiDefinition.Merchantno;
            executionType = JobType.All;

            List<long> refIds = await _priceStockDalService.GetAvailableRefIdsByMerchantNoAsync(executionType, merchantNo);

            Logger.Information("TGService > PushPriceStock > Number of Jobs Available for Execution Type {ExecutionType}: {count}", fileName: _logFolderName, executionType, refIds?.Count ?? 0);

            foreach (long refId in refIds)
            {
                var merchants = await _priceStockDalService.GetStoreNoListAsync(refId, _apiDefinition.Merchantno);
                List<Task> merchTasks = new List<Task>();
                var dataLst = new ConcurrentBag<CustomResult>();

                Logger.Information("TGService > PushPriceStock > Execution Type: {ExecutionType}, Number of Merchants: {count}", fileName: _logFolderName, executionType, merchants?.Count() ?? 0);



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
                        data = await TG_PostProductByMerchantAsync(priceStockService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("TGService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {@data}", fileName: _logFolderName, executionType, data);
                    }));
                }
                await Task.WhenAll(merchTasks);

                var totalCount = dataLst.Count();
                var FailedCount = dataLst.Count(f => f.HasErrors);
                var SuccessCount = dataLst.Count(f => !f.HasErrors);
                var HasErrors = (totalCount != SuccessCount) ? Character.E : Character.H;
                var HasErrorStr = HasErrors == Character.E ? "partially" : "successfully";
                var srvLogErrorPhrase = (totalCount != SuccessCount) ? ", LOG TABLOLARINI KONTROL EDINIZ" : "";
                var srvLogError = $"TOPLAM BIRIM SAY.: {totalCount}, BASARILI BIRIM SAY.: {SuccessCount}, BASARISIZ BIRIM SAY.: {FailedCount}{srvLogErrorPhrase}";
                var SrvLogStatus = (totalCount != SuccessCount) ? "HATA" : "TAMAMLANDI";

                var pazarYeriJobResult = await _priceStockDalService.GetPazarYeriJobResultByRefIdAsync(refId);
                if (pazarYeriJobResult != null)
                {
                    pazarYeriJobResult.ThreadSize = NumberofProducts;
                    pazarYeriJobResult.NumberOfThreads = dataLst.Count;
                    pazarYeriJobResult.HasErrors = HasErrors;
                    pazarYeriJobResult.HasSent = Character.E;

                    var updateResponse = await _priceStockDalService.UpdatePazarYeriJobResultAsync(pazarYeriJobResult);

                    if (!updateResponse)
                        Logger.Warning("TGService > PushPriceStock > Execution Type: {ExecutionType}, Cannot update Repo for Ref_Id: {refId}", fileName: _logFolderName, executionType, refId);

                }

                await _priceStockDalService.UpdateServiceLog(refId, SrvLogStatus, srvLogError);

                if (totalCount != SuccessCount)
                    Logger.Warning("TGService > PushPriceStock > Execution Type: {ExecutionType}, Task Result Data: {count}", fileName: _logFolderName, executionType, dataLst?.Count ?? 0);
            }
        }

        public async Task<List<CustomResult>> PushPriceStockOnlyStockAsync(Dictionary<string, string> properties, long refId)
        {
            int NumberofProducts = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofProducts]);
            string merchantNo = _apiDefinition.Merchantno;
            JobType executionType = JobType.OnlyStock;

            ConcurrentBag<CustomResult> dataLst = new();
            Logger.Information("TGService > PushPriceStock is running for ExecutionType: {ExecutionType}", fileName: _logFolderName, executionType);
            try
            {
                var merchants = await _priceStockDalService.GetStoreNoListAsync(refId, _apiDefinition.Merchantno);

                if (!merchants.Any())
                {
                    Logger.Information($"TGService > PushPriceStock > Execution Type: {executionType} | no merchants found for refId:{refId}", fileName: _logFolderName);
                    return dataLst.ToList();
                }

                List<Task> merchTasks = new();
                Logger.Information("TGService > PushPriceStock > Execution Type: {ExecutionType}, Number of Merchants: {count}", fileName: _logFolderName, executionType, merchants.Count);

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
                        data = await TG_PostProductByMerchantAsync(priceStockService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("TGService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {@data}", fileName: _logFolderName, executionType, data);

                    }));
                }
                await Task.WhenAll(merchTasks);

            }
            catch (Exception ex)
            {
                Logger.Error("TGService > PushPriceStock, Execution Type: {ExecutionType}, Exception occurred with Exception: {exception}", fileName: _logFolderName, executionType, ex);
            }
            Logger.Information("TGService > PushPriceStock is completed for ExecutionType: {ExecutionType}", fileName: _logFolderName, executionType);
            return dataLst.ToList();
        }

        public async Task VerifyPriceStockAsync(Dictionary<string, string> properties)
        {
            var executionType = CommonEnums.JobType.VerifyPriceAndStock;


            var verifiableslst = await _priceStockDalService.GetAvailableVerifiablesAsync(_apiDefinition.Merchantno);

            List<IGrouping<string, MerchantVerify>> jobs = verifiableslst.GroupBy(f => f.GUID).ToList();
            Logger.Information("TGService > VerifyPriceStockAsync > Number of Guids Available for Execution Type {executionType}: {count}", fileName: _logFolderName, executionType, jobs?.Count ?? 0);

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
                    data = await TG_VerifyPostsByGuidAsync(priceStockService, data, executionType);
                    dataLst.Add(data);
                    Logger.Information("TGService > VerifyPriceStockAsync > Execution Type: {executionType}, Individual Task Result Data: {Guid}", fileName: _logFolderName, executionType, data.Guid);
                }));
            }

            await Task.WhenAll(merchTasks);

            var totalCount = dataLst?.Count() ?? 0;
            var SuccessCount = dataLst?.Count(f => !f.HasErrors) ?? 0;

            if (totalCount != SuccessCount)
            {
                Logger.Warning("TGService > VerifyPriceStockAsync > Execution Type:{executionType}, Task Result Data: {Count}", fileName: _logFolderName, executionType, dataLst?.Count ?? 0);
            }
        }

        #endregion

        #region Private Methods

        private async Task<CustomResult> TG_PostProductByMerchantAsync(IPriceStockDalService priceStockDalService, CustomResult data)
        {
            data.StartDateTime = DateTime.Now;
            List<TGUpdateStockAndPriceReqDto> requests = new List<TGUpdateStockAndPriceReqDto>();

            try
            {
                var jobdetails = await priceStockDalService.GetJobResultDetailsByIdMerchAsync(data.RefId, data.MerchantId, data.MerchantNo);

                data.DetailsCount = jobdetails.Count;

                jobdetails = _baseService.CalculateThreads(data.DetailsCount, data.NumberofProducts, jobdetails.ToList(), data.RefId, data.MerchantId, data.ThreadId).ToList();
                data.InternalThreadCount = jobdetails.Count;

                if (!jobdetails.Any())
                {
                    Logger.Warning($"TGService > TG_PostProductByMerchant > Execution Type: {data.ExecutionType}, No Details found for RefId: {data.RefId}, MerchantId: {data.MerchantId}", fileName: _logFolderName);
                    return data;
                }

                #region Conventional For Loop
                for (int i = 1; i <= jobdetails.Max(f => f.ThreadNo); i++)
                {
                    TGUpdateStockAndPriceReqDto req = new TGUpdateStockAndPriceReqDto
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
                    req.TyGoRequestItems = new TGUpdateStockAndPriceReqDto.RequestItems();
                    req.TyGoRequestItems.Items = new List<TGUpdateStockAndPriceDto>();

                    foreach (var det in tmpdetails)
                    {
                        TGUpdateStockAndPriceDto RequestItem = new TGUpdateStockAndPriceDto
                        {
                            #region WhichHasBeenSent
                            sellingPrice = det.IndirimliSatisFiyat.HasValue && det.IndirimliSatisFiyat.Value > 0 ? det.IndirimliSatisFiyat.Value : det.SatisFiyat.HasValue ? det.SatisFiyat.Value : 0,
                            originalPrice = det.SatisFiyat.HasValue ? det.SatisFiyat.Value : 0,
                            barcode = det.PazarYeriMalNo,
                            quantity = det.StokMiktar.HasValue ? det.StokMiktar.Value : 0,
                            storeId = det.PazarYeriBirimNo,
                            #endregion

                            #region ForReference
                            DetailId = det.DetailId,
                            RefId = det.RefId
                            #endregion
                        };
                        req.TyGoRequestItems.Items.Add(RequestItem);
                    }

                    var requestStr = JsonConvert.SerializeObject(req.TyGoRequestItems);

                    try
                    {
                        var result = await _trendyolGoClient.StoreUpdatePriceAndInventory(_apiDefinition.SupplierId, req.TyGoRequestItems);

                        req.RequestFailed = !(result.ResponseMessage.IsSuccessStatusCode);
                        req.HttpStatusCode = result.ResponseMessage.StatusCode;
                        req.APIResponse = result.StringContent;
                        var resultObject = JsonConvert.DeserializeObject<TGUpdateStockAndPriceRespDto>(result.StringContent);
                        req.Guid = result.ResponseMessage.IsSuccessStatusCode ? resultObject.BatchRequestId : string.Empty;

                        PazarYeriLog logEntry = new()
                        {
                            RefId = data.RefId,
                            ExecutionType = data.ExecutionType.ToString(),
                            PazarYeriBirimNo = data.MerchantId,
                            ThreadNo = req.Thread_No,
                            LogType = result.ResponseMessage.IsSuccessStatusCode ? nameof(LogType.AllCompleted) : nameof(LogType.AllFailed),
                            Request = requestStr,
                            Response = $"HTTP StatusCode: {result.ResponseMessage.StatusCode} >  {req.APIResponse}",
                            HasErrors = result.ResponseMessage.IsSuccessStatusCode ? Character.H : Character.E,
                            DetailId = 0,
                            PazarYeriNo = data.MerchantNo,
                            Guid = !req.RequestFailed ? resultObject.BatchRequestId : string.Empty
                        };
                        Logger.Information("TGService > TG_PostProductByMerchant > PushPriceStockAsync() logEntry {@logEntry}", _logFolderName, logEntry);
                    }
                    catch (Exception ex)
                    {
                        req.RequestFailed = true;
                        Logger.Error("TGService > TG_PostProductByMerchant > Execution Type: {ExecutionType}, Inner Exception for Ref_Id: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}, Thread_No: {ThreadNo} - Exception: {exception}", fileName: _logFolderName, (data?.ExecutionType.ToString() ?? ""), data?.RefId.ToString() ?? "", data.MerchantId ?? "", (data?.ThreadId.ToString() ?? ""), (req?.Thread_No.ToString() ?? ""), ex);

                        PazarYeriLog logEntry = new PazarYeriLog
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

                data.RequestCompletionDateTime = DateTime.Now;
                data.FailedThreadCount = requests.Count(f => f.RequestFailed);
                data.SuccessfulThreadCount = requests.Count(f => !f.RequestFailed);


                #region Pazaryeri Job Result Details Update
                Stopwatch stopwatch = Stopwatch.StartNew();

                foreach (var requestItem in requests)
                {
                    List<PazarYeriJobResultDetails> updatedDetailList = new List<PazarYeriJobResultDetails>();

                    foreach (var individualItem in requestItem.TyGoRequestItems.Items)
                    {
                        long refId = individualItem.RefId;
                        long detailId = individualItem.DetailId;
                        var detail = jobdetails.Find(f => f.RefId == refId && f.DetailId == detailId);
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

                        Logger.Information("TGService > TG_PostProductByMerchant > Execution Type: {ExecutionType}, Requests updates successfully in Repo Db/for RefId: {RefId}, MerchantId: {MerchantId}", fileName: _logFolderName, data?.ExecutionType, data?.RefId, data?.MerchantId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("TGService > TG_PostProductByMerchant > Execution Type: {ExecutionType}, Cannot update requests successfully in Db/Repo for RefId: {RefId}, MerchantId: {MerchantId}, Exception : {ex}", fileName: _logFolderName, data?.ExecutionType, data?.RefId, data?.MerchantId, ex);
                    }
                }


                stopwatch.Stop();
                Logger.Information("TGService UpdatePazarYeriJobResultDetailRangeAsync finished in {elapsedTime}ms.", _logFolderName, stopwatch.ElapsedMilliseconds);
                #endregion

                data.CompletionDateTime = DateTime.Now;

                if (requests.Count(f => !f.RequestFailed) != requests.Count)
                {
                    data.HasErrors = true;
                    Logger.Warning("TGService > TG_PostProductByMerchant > Execution Type: {ExecutionType}, Completed partially for MerchantId: {MerchantId} for Execution Data: {@data}", fileName: _logFolderName, data?.ExecutionType, data?.MerchantId, data);
                }
                else
                {
                    data.HasErrors = false;
                    Logger.Information("TGService > TG_PostProductByMerchant > Execution Type: {ExecutionType}, Completed successfully for MerchantId: {MerchantId} for Execution Data: {@data}", fileName: _logFolderName, data?.ExecutionType, data?.MerchantId, data);
                }

            }

            catch (Exception ex)
            {
                data.CompletionDateTime = DateTime.Now;
                data.HasErrors = true;
                data.ExceptionsString = ex.Message;

                Logger.Error("TGService > TG_PostProductByMerchant > Execution Type: {ExecutionType}, Failed with Exception for MerchantId: {MerchantId} for Execution Data: {@data}, Exception: {exception}", fileName: _logFolderName, data?.ExecutionType, data?.MerchantId, data, ex);
            }
            return data;
        }

        private async Task<CustomVerifyResult> TG_VerifyPostsByGuidAsync(IPriceStockDalService priceStockDalService, CustomVerifyResult data, JobType ExecutionType)
        {
            Logger.Information("TGService > TG_VerifyPostsGuid > Verifiable Data: MerchantVerifies Count {Count}", fileName: _logFolderName, data?.MerchantVerifies?.Count ?? 0);

            data!.StartDateTime = DateTime.Now;
            string guid = data.Guid;
            string merchantId = _apiDefinition.SupplierId;

            List<PazarYeriJobResultDetails> detailPazarYeriJobList = new List<PazarYeriJobResultDetails>();
            try
            {
                var result = await _trendyolGoClient.GetBatchRequestResult(merchantId, guid);

                data.RequestCompletionDateTime = DateTime.Now;
                var results = JsonConvert.DeserializeObject<TGGetBatchRequestResultDto.Root>(result.StringContent);

                PazarYeriLog logEntry;
                if (result.ResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var requestItems = results;
                    var dataItems = data.MerchantVerifies;

                    if (requestItems.Items != null && requestItems.Items.Count > 0)
                    {
                        foreach (var requestItmProduct in requestItems.Items.Select(s => s.RequestItem).Select(s => s.Product))
                        {
                            var tmp = dataItems.Find(f => f.PAZAR_YERI_MAL_NO == requestItmProduct.ProductBarcode);

                            if (tmp != null)
                            {
                                requestItmProduct.RefId = tmp.REF_ID;
                                requestItmProduct.DetailId = tmp.DETAIL_ID;
                                requestItmProduct.Thread_No = tmp.THREAD_NO;
                            }
                        }
                    }
                    if (requestItems != null)
                    {
                        var tmp = dataItems.Find(f => f.GUID == requestItems.BatchRequestId);

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
                        if (requestItems.Items != null && requestItems.Items.Count != 0)
                        {
                            foreach (var itm in requestItems.Items)
                            {
                                #region Update PazaryeriJobResultDetails
                                var HasVerified = !string.IsNullOrEmpty(itm.Status) ? itm.Status[..1] : string.Empty;
                                var HasError = (itm.Status == null || itm.Status.ToUpper() == "SUCCESS") ? Character.H : Character.E;
                                if (pazarYeriJobResultDetails.Any())
                                {
                                    var detailItem = pazarYeriJobResultDetails.FirstOrDefault(f => f.DetailId == itm.RequestItem.Product.DetailId);
                                    if (detailItem != null)
                                    {
                                        detailItem.HasVerified = HasVerified;
                                        detailItem.HasErrors = HasError;
                                        detailPazarYeriJobList.Add(detailItem);
                                    }
                                }
                                #endregion
                                #region Insert Pazaryeri Log

                                if (HasError == Character.E)
                                {
                                    logEntry = new()
                                    {
                                        RefId = itm.RequestItem.Product.RefId,
                                        ExecutionType = data.ExecutionType.ToString(),
                                        PazarYeriBirimNo = data.MerchantId,
                                        ThreadNo = itm.RequestItem.Product.Thread_No,
                                        LogType = requestItems.Status,
                                        Request = $"MerchantId: {merchantId}, Guid: {guid}",
                                        Response = JsonConvert.SerializeObject(itm.FailureReasons),
                                        HasErrors = Character.E,
                                        DetailId = itm.RequestItem.Product.DetailId,
                                        PazarYeriNo = PazarYeri.TrendyolGo,
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
                        Logger.Error("HERepository > UpdateRefIdDetailIdByGuid, MerchantId: {merchantId}, Guid: {guid}, ExecutionType: {ExecutionType}, Exception: {exception}, Requests: {@requestItems}", fileName: _logFolderName, merchantId, guid, ExecutionType, ex, requestItems);
                    }
                    data.CompletionDateTime = DateTime.Now;
                }
                else
                {
                    var requestItems = new TGGetRequestsByIdRespDto();

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
                        requestItems.requestItems.Add(requestItem);
                    }

                    try
                    {
                        var pazarYeriJobResultDetails = await priceStockDalService.GetPazarYeriJobResultDetailsAsync(requestItems.RefId, requestItems.ThreadNo, data.Guid, data.MerchantId);
                        var HasError = (requestItems.status == null || requestItems.status.ToUpper() == "SUCCESS") ? Character.H : Character.E;
                        var HasVerified = requestItems.status == null ? Character.C : requestItems.status.Substring(0, 1);
                        //if (pazarYeriJobResultDetails != null)
                        //{
                        //    foreach (var item in pazarYeriJobResultDetails)
                        //    {
                        //        item.HasVerified = Character.D;
                        //    }

                        //    await priceStockDalService.UpdateRangeAsync(pazarYeriJobResultDetails);
                        //}

                        if (requestItems.requestItems != null)
                        {
                            foreach (var itm in requestItems.requestItems.Select(s => s.ErrorMessages))
                            {
                                HasError = itm == null && requestItems.status is not null && requestItems.status == "Done" ? Character.H : Character.E;

                                var detail = pazarYeriJobResultDetails.FirstOrDefault(x => x.DetailId == requestItems.DetailId);
                                if (detail is not null)
                                {
                                    detail.HasVerified = HasVerified;
                                    detail.HasErrors = HasError;

                                    detailPazarYeriJobList.Add(detail);
                                }

                                logEntry = new PazarYeriLog
                                {
                                    RefId = requestItems.RefId,
                                    ExecutionType = data.ExecutionType.ToString(),
                                    PazarYeriBirimNo = data.MerchantId,
                                    ThreadNo = requestItems.ThreadNo,
                                    LogType = requestItems.status,
                                    Request = $"MerchantId: {merchantId}, Guid: {guid}",
                                    Response = JsonConvert.SerializeObject(itm),
                                    HasErrors = Character.E,
                                    DetailId = requestItems.DetailId,
                                    PazarYeriNo = PazarYeri.TrendyolGo,
                                    Guid = data.Guid
                                };

                                await priceStockDalService.InsertPazarYeriLogAsync(logEntry);
                            }
                        }

                        if (detailPazarYeriJobList.Any())
                            await priceStockDalService.UpdatePazarYeriJobResultDetailRangeVerifiedAsync(detailPazarYeriJobList).ConfigureAwait(false);

                        data.HasErrors = false;
                    }
                    catch (Exception ex)
                    {
                        data.HasErrors = true;
                        Logger.Error("TGService > UpdateRefIdDetailIdByGuid, MerchantId: {merchantId}, Guid: {guid}, ExecutionType: {ExecutionType}, Exception: {exception}, Requests: {@requestItems}", fileName: _logFolderName, merchantId, guid, ExecutionType, ex, requestItems);
                    }
                    data.CompletionDateTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                data.HasErrors = true;
                data.CompletionDateTime = DateTime.Now;
                data.ExceptionString = $"TGService > TG_VerifyPostsByGuid > Execution Type: {ExecutionType}, Exception: {ex.Message}";
                Logger.Error("TGService > TG_VerifyPostsByGuid > Execution Type: {ExecutionType}, Exception: {exception}", fileName: _logFolderName, ExecutionType, ex);
            }
            return data;
        }

        #endregion
    }
}