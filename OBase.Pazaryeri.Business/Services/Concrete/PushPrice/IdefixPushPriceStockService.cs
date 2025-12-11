using Hangfire.Storage.SQLite.Entities;
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
using OBase.Pazaryeri.Domain.Dtos.Idefix;
using OBase.Pazaryeri.Domain.Dtos.Idefix.PriceStock;
using OBase.Pazaryeri.Domain.Dtos.İdefix.PriceStock;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Net;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Dtos.TrendyolGo.TGUpdateStockAndPriceReqDto;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Services.Concrete.PushPrice
{
    public class IdefixPushPriceStockService : IIdefixPushPriceStockService
    {
        #region Variables

        private readonly IPriceStockDalService _priceStockDalService;
        private readonly IBaseService _baseService;
        private readonly IPimIdefixClient _pimIdefixClient;
        private readonly ApiDefinitions _apiDefinition;
        string _logFolderName = nameof(CommonEnums.PazarYerleri.Idefix);
        private readonly IServiceScopeFactory _serviceScopeFactory;

        #endregion

        #region Constructor

        public IdefixPushPriceStockService(IPriceStockDalService priceStockDalService, IOptions<AppSettings> options, IBaseService baseService, IPimIdefixClient pimIdefixClient, IServiceScopeFactory serviceScopeFactory)
        {
            _priceStockDalService = priceStockDalService;
            _baseService = baseService;
            _pimIdefixClient = pimIdefixClient;
            _apiDefinition = options.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Idefix);
            _serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Methods

        public async Task PushPriceStockAsync(Dictionary<string, string> properties, JobType executionType)
        {
            int numberofProducts = int.Parse(properties[IdefixConstants.Parameters.NumberofProducts]);
            int deliveryDuration = int.Parse(properties[IdefixConstants.Parameters.DeliveryDuration]);
            string deliveryType =properties[IdefixConstants.Parameters.DeliveryType];
            string merchantNo = _apiDefinition.Merchantno;
            string merchantId = _apiDefinition.SupplierId;
            executionType = JobType.All;

            List<long> refIds = await _priceStockDalService.GetAvailableRefIdsByMerchantNoAsync(executionType, merchantNo);
            Logger.Information("IdefixService > PushPriceStock > Number of Jobs Available for Execution Type: {ExecutionType}, Job Count: {jobCount} , MerchantNo:{merchantNo}", fileName: _logFolderName, executionType, refIds?.Count ?? 0, merchantNo);

            foreach (long refId in refIds)
            {
                var merchants = await _priceStockDalService.GetStoreNoListAsync(refId, _apiDefinition.Merchantno);
                List<Task> merchTasks = new List<Task>();
                var dataLst = new ConcurrentBag<CustomResult>();

                Logger.Information("IdefixService > PushPriceStock > Execution Type: {executionType}, Number of Merchants: {merchantCount}", _logFolderName, executionType, merchants.Count());

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
                        data.NumberofProducts = numberofProducts;
                        data.DeliveryType=deliveryType;
                        data.DeliveryDuration = deliveryDuration;   
                        data.ThreadId = Environment.CurrentManagedThreadId;
                        data.InternalThreadCount = 0;
                        data.FailedThreadCount = 0;
                        data.SuccessfulThreadCount = 0;
                        data.ExecutionType = executionType;
                        data.MerchantNo = _apiDefinition.Merchantno;
                        data.MerchantId = storeNoItem;
                        data.CreationDateTime = DateTime.Now;
                        data = await Idefix_PostProductByMerchantWithVendor(priceStockService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("IdefixService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {data}", fileName: _logFolderName, executionType, data);
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
                    pazarYeriJobResult.ThreadSize = numberofProducts;
                    pazarYeriJobResult.NumberOfThreads = dataLst?.Count ?? 0;
                    pazarYeriJobResult.HasErrors = HasErrors;
                    pazarYeriJobResult.HasSent = Character.E;

                    var updateResponse = await _priceStockDalService.UpdatePazarYeriJobResultAsync(pazarYeriJobResult);

                    if (!updateResponse)
                    {
                        Logger.Warning("IdefixService > PushPriceStock > Execution Type: {ExecutionType}, Cannot update Repo for Ref_Id: {refId}", fileName: _logFolderName, executionType, refId);
                    }
                }

                await _priceStockDalService.UpdateServiceLog(refId, SrvLogStatus, srvLogError);

                if (totalCount != SuccessCount)
                {
                    Logger.Warning("IdefixService > PushPriceStock > Execution Type: {ExecutionType}, Task Result Data: {count}", fileName: _logFolderName, executionType, dataLst?.Count ?? 0);
                }
            }
        }

        public async Task<List<CustomResult>> PushPriceStockOnlyStockAsync(Dictionary<string, string> properties, long refId)
        {
            int NumberofThreads = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofThreads]);
            int NumberofProducts = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofProducts]);
            int NumberofTries = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofTries]);
            int NumberofSQLRows = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofSQLRows]);
            int deliveryDuration = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.DeliveryDuration]);
            string deliveryType = properties[SharedPriceStockOnlyConstants.Parameters.DeliveryType];
            string merchantNo = _apiDefinition.Merchantno;
            JobType executionType = JobType.OnlyStock;

            ConcurrentBag<CustomResult> dataLst = new();
            Logger.Information("IdefixService > PushPriceStock is running for ExecutionType: {ExecutionType}", fileName: _logFolderName, executionType);
            try
            {
                var merchants = await _priceStockDalService.GetStoreNoListAsync(refId, _apiDefinition.Merchantno);
                if (!merchants.Any())
                {
                    Logger.Information($"IdefixService > PushPriceStock > Execution Type: {executionType} | no merchants found for refId:{refId}", fileName: _logFolderName);
                    return dataLst.ToList();
                }
                List<Task> merchTasks = new();
                Logger.Information("IdefixService > PushPriceStock > Execution Type: {ExecutionType}, Number of Merchants: {merchantCount}", fileName: _logFolderName, executionType, merchants.Count);

                foreach (var storeNoItem in merchants)
                {
                    merchTasks.Add(Task.Run(async () =>
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var priceStockService = scope.ServiceProvider.GetRequiredService<IPriceStockDalService>();

                        var data = new CustomResult() { MerchantId = storeNoItem, CreationDateTime = DateTime.Now };

                        data.RefId = (int)refId;
                        data.NumberofProducts = NumberofProducts;
                        data.ThreadId = Environment.CurrentManagedThreadId;
                        data.InternalThreadCount = 0;
                        data.FailedThreadCount = 0;
                        data.SuccessfulThreadCount = 0;
                        data.ExecutionType = executionType;
                        data.MerchantNo = merchantNo;
                        data.MerchantId = storeNoItem;
                        data.CreationDateTime = DateTime.Now;
                        data.DeliveryType = deliveryType;
                        data.DeliveryDuration = deliveryDuration;
                        data = await Idefix_PostProductByMerchantWithVendor(priceStockService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("IdefixService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {@data}", fileName: _logFolderName, executionType, data);

                    }));
                }

                await Task.WhenAll(merchTasks);

            }
            catch (Exception ex)
            {
                Logger.Error("IdefixService > PushPriceStock, Execution Type: {ExecutionType}, Exception occurred with Exception: {ex.Message}", fileName: _logFolderName, executionType, ex);
            }
            Logger.Information("IdefixService > PushPriceStock is completed for ExecutionType: {ExecutionType}", fileName: _logFolderName, executionType);
            return dataLst.ToList();
        }

        public async Task VerifyPriceStockAsync(Dictionary<string, string> properties)
        {
            var executionType = JobType.VerifyPriceAndStock;
            var verifiableslst = await _priceStockDalService.GetAvailableVerifiablesAsync(_apiDefinition.Merchantno);
            List<IGrouping<string, MerchantVerify>> jobs = verifiableslst.GroupBy(f => f.GUID).ToList();
            Logger.Information("IdefixService > Idefix_VerifyPriceStock > Number of Guids Available for Execution Type {executionType}: {count}", fileName: _logFolderName, executionType, jobs?.Count ?? 0);

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
                    await Idefix_VerifyPostsByGuidAsync(priceStockService, data, executionType);
                    dataLst.Add(data);
                    Logger.Information("IdefixService > Idefix_VerifyPriceStock > Execution Type: {executionType}, Individual Task Result Data: {Guid}", fileName: _logFolderName, executionType, data.Guid);
                }));
            }

            await Task.WhenAll(merchTasks);


            var totalCount = dataLst?.Count() ?? 0;
            var SuccessCount = dataLst?.Count(f => !f.HasErrors) ?? 0;

            if (totalCount != SuccessCount)
            {
                Logger.Warning("IdefixService > Idefix_VerifyPriceStock > Execution Type:{executionType}, Task Result Data: {Count}", fileName: _logFolderName, executionType, dataLst?.Count ?? 0);
            }
        }

        #endregion

        #region Utilities

        private async Task<CustomResult> Idefix_PostProductByMerchantWithVendor(IPriceStockDalService priceStockDalService, CustomResult data)
        {
            data.StartDateTime = DateTime.Now;
            string merchantId = _apiDefinition.SupplierId;
            List<IdefixProductPriceAndStockUpdateWithVendor> requests = new List<IdefixProductPriceAndStockUpdateWithVendor>();

            try
            {
                var jobdetails = await priceStockDalService.GetJobResultDetailsByIdMerchAsync(data.RefId, data.MerchantId, data.MerchantNo, satisFiyatControl:true);
                data.DetailsCount = jobdetails.Count;

                jobdetails = _baseService.CalculateThreads(data.DetailsCount, data.NumberofProducts, jobdetails.ToList(), data.RefId, data.MerchantId, data.ThreadId).ToList();
                data.InternalThreadCount = jobdetails.Count;

                if (!jobdetails.Any())
                {
                    Logger.Warning($"IdefixService > Idefix_PostProductByMerchantWithVendor > Execution Type: {data.ExecutionType}, No Details found for RefId: {data.RefId}, MerchantId: {data.MerchantId}", fileName: _logFolderName);
                    return data;
                }

                #region Conventional For Loop
                for (int i = 1; i <= jobdetails.Max(f => f.ThreadNo); i++)
                {
                    IdefixProductPriceAndStockUpdateWithVendor req = new IdefixProductPriceAndStockUpdateWithVendor
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
                    req.requestItem.Items = new List<InventoryItem>();
                 

                    foreach (var det in tmpdetails)
                    {

                        InventoryItem RequestItem = new InventoryItem
                        {
                            #region WhichHasBeenSent
                            Price = det.IndirimliSatisFiyat.HasValue && det.IndirimliSatisFiyat.Value > 0 ? det.IndirimliSatisFiyat.Value : det.SatisFiyat ?? 0,
                            Barcode = det.Barkod,
                            InventoryQuantity = det.StokMiktar.HasValue ? det.StokMiktar.Value : 0,
                            DeliveryDuration = data.DeliveryDuration,
                            DeliveryType=data.DeliveryType,
                            #endregion

                            #region ForReference
                            DetailId = det.DetailId,
                            RefId = det.RefId
                            #endregion
                        };
                        req.requestItem.Items.Add(RequestItem);
                    }

                    var requestStr = JsonConvert.SerializeObject(req.requestItem.Items);

                    try
                    {
                        Logger.Warning("IdefixService > PushPriceStock > inventory-upload Request: {@request}", fileName: _logFolderName, requestStr);
                        var result = await _pimIdefixClient.UpdatePriceAndQuantityWithVendor(merchantId, req.requestItem);

                        Logger.Warning("IdefixService > PushPriceStock > UpdatePriceAndQuantityWithVendor Response: {@response}", fileName: _logFolderName, result.StringContent);

                        var res = result.GetContent();

                        req.RequestFailed = !(result.ResponseMessage.IsSuccessStatusCode && res.Status.ToUpper() != "COMPLETED");
                        req.HttpStatusCode = result.ResponseMessage.StatusCode;
                        req.APIResponse = result.StringContent;
                        req.Guid = result.ResponseMessage.IsSuccessStatusCode ? res.BatchRequestId : string.Empty;

                        PazarYeriLog logEntry = new()
                        {
                            RefId = data.RefId,
                            ExecutionType = data.ExecutionType.ToString(),
                            PazarYeriBirimNo = data.MerchantId,
                            ThreadNo = req.Thread_No,
                            LogType = result.ResponseMessage.IsSuccessStatusCode ? nameof(LogType.AllCompleted) : nameof(LogType.AllFailed),
                            Request = requestStr,
                            Response = $"HTTP StatusCode: {result.ResponseMessage.StatusCode} >  {result.StringContent}",
                            HasErrors = req.RequestFailed ? Character.E : Character.H,
                            DetailId = 0,
                            PazarYeriNo = data.MerchantNo,
                            Guid = !req.RequestFailed ? res.BatchRequestId : string.Empty
                        };
                        Logger.Information("IdefixService > Idefix_PostProductByMerchantWithVendor > PushPriceStockAsync() logEntry {@logEntry}", _logFolderName, logEntry);
                        //await priceStockDalService.InsertPazarYeriLogAsync(logEntry);
                    }
                    catch (Exception ex)
                    {
                        req.RequestFailed = true;
                        Logger.Error("IdefixService > Idefix_PostProductByMerchantWithVendor > Execution Type: {ExecutionType}, Inner Exception for Ref_Id: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}, Thread_No: {ThreadNo} - Exception: {exception}", fileName: _logFolderName, (data?.ExecutionType.ToString() ?? ""), data?.RefId.ToString() ?? "", data.MerchantId ?? "", (data?.ThreadId.ToString() ?? ""), (req?.Thread_No.ToString() ?? ""), ex);

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

                    foreach (var individualItem in requestItem.requestItem.Items)
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

                        Logger.Information("IdefixService > Idefix_PostProductByMerchantWithVendor > Execution Type: {ExecutionType}, Requests updates successfully in Repo Db/for RefId: {RefId}, MerchantId: {MerchantId}", fileName: _logFolderName, data?.ExecutionType, data?.RefId, data?.MerchantId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("IdefixService > Idefix_PostProductByMerchantWithVendor > Execution Type: {ExecutionType}, Cannot update requests successfully in Db/Repo for RefId: {RefId}, MerchantId: {MerchantId}, Exception : {ex}", fileName: _logFolderName, data?.ExecutionType, data?.RefId, data?.MerchantId, ex);
                    }
                }

                stopwatch.Stop();
                Logger.Information("IdefixService UpdatePazarYeriJobResultDetailRangeAsync finished in {elapsedTime}ms.", _logFolderName, stopwatch.ElapsedMilliseconds);
                #endregion

                data.CompletionDateTime = DateTime.Now;

                if (requests.Count(f => !f.RequestFailed) != requests.Count)
                {
                    data.HasErrors = true;
                    Logger.Warning("IdefixService > Idefix_PostProductByMerchantWithVendor > Execution Type: {ExecutionType}, Completed partially for MerchantId: {MerchantId} for Execution Data: {@data}", fileName: _logFolderName, data?.ExecutionType, data?.MerchantId, data);
                }
                else
                {
                    data.HasErrors = false;
                    Logger.Information("IdefixService > Idefix_PostProductByMerchantWithVendor > Execution Type: {ExecutionType}, Completed successfully for MerchantId: {MerchantId} for Execution Data: {@data}", fileName: _logFolderName, data?.ExecutionType, data?.MerchantId, data);
                }
            }
            catch (Exception ex)
            {
                data.CompletionDateTime = DateTime.Now;
                data.HasErrors = true;
                data.ExceptionsString = ex.Message;

                Logger.Error("IdefixService > Idefix_PostProductByMerchantWithVendor > Execution Type: {ExecutionType}, Failed with Exception for MerchantId: {MerchantId} for Execution Data: {@data}, Exception: {exception}", fileName: _logFolderName, data?.ExecutionType, data?.MerchantId, data, ex);
            }
            return data;
        }

        private async Task<CustomVerifyResult> Idefix_VerifyPostsByGuidAsync(IPriceStockDalService priceStockDalService, CustomVerifyResult data, JobType ExecutionType)
        {
            Logger.Information("IdefixService > Idefix_VerifyPriceStock > Verifiable Data: {MerchantVerifiesCount}", fileName: _logFolderName, data.MerchantVerifies?.Count ?? 0);
            string merchantId = _apiDefinition.SupplierId;
            data.StartDateTime = DateTime.Now;
            string guid = data.Guid;

            try
            {
                var result = await _pimIdefixClient.GetBatchRequestsControlById(merchantId,guid);

                data.RequestCompletionDateTime = DateTime.Now;
                List<PazarYeriLog> pazarYeriLogs = new List<PazarYeriLog>();
                List<PazarYeriJobResultDetails> detailPazarYeriJobList = new List<PazarYeriJobResultDetails>();
                PazarYeriLog logEntry;

                var batchResultModel = JsonConvert.DeserializeObject<IdefixGenericResponse<ProductInventoryItemWithBatchRequesIdResponse>>(result.StringContent);   

                var dataItems = data.MerchantVerifies;

                if (result.ResponseMessage.IsSuccessStatusCode)
                {   
                    foreach (var requestItm in batchResultModel.Items)
                    {
                        var tmp = dataItems.FirstOrDefault(f => f.BARKOD == requestItm.Barcode);

                        if (tmp != null)
                        {
                            requestItm.RefId = tmp.REF_ID;
                            requestItm.DetailId = tmp.DETAIL_ID;
                            requestItm.Thread_No = tmp.THREAD_NO;                           
                        }
                    }
                    var tmpBatchRequestItem = dataItems.FirstOrDefault(f => f.GUID == batchResultModel.BatchRequestId);
                    if (tmpBatchRequestItem != null)
                    {
                        batchResultModel.RefId = tmpBatchRequestItem.REF_ID;
                        batchResultModel.DetailId = tmpBatchRequestItem.DETAIL_ID;
                        batchResultModel.Thread_No = tmpBatchRequestItem.THREAD_NO;
                    }

                    //created; Stok ve fiyat talebi oluşturuldu
                    //completed; İşlem tamamlandı
                    var completedVerifyProduct = batchResultModel?.Items?.Where(x => x.Status == "completed" || x.Status == "created").ToList();


                    //completed; İşlem tamamlandı
                    var declineVerifyProduct = batchResultModel.Items.Where(x => x.Status == "decline").ToList();

                    try
                    {
                        var pazarYeriJobResultDetails = await priceStockDalService.GetPazarYeriJobResultDetailsAsync(batchResultModel.RefId, batchResultModel.Thread_No, data.Guid, data.MerchantId);
                        foreach (var itm in pazarYeriJobResultDetails)
                        {
                            #region Update PazaryeriJobResultDetails
                            if (completedVerifyProduct?.Any(x => x.Barcode == itm.Barkod) ?? false)
                            {                             

                                itm.HasVerified = Character.E;
                                itm.HasErrors = Character.H;
                            }
                            else
                            {
                                itm.HasVerified = Character.H;
                                itm.HasErrors = Character.E;
                            }

                            detailPazarYeriJobList.Add(itm);
                            #endregion
                            #region Insert Pazaryeri Log
                            if (itm.HasErrors == Character.E)
                            {
                                logEntry = new()
                                {
                                    RefId = itm.RefId,
                                    ExecutionType = data.ExecutionType.ToString(),
                                    PazarYeriBirimNo = data.MerchantId,
                                    ThreadNo = itm.ThreadNo,
                                    LogType = batchResultModel.Status,
                                    Request = $"Guid: {guid}",
                                    Response = JsonConvert.SerializeObject(batchResultModel),
                                    HasErrors = Character.E,
                                    DetailId = itm.DetailId,
                                    PazarYeriNo = PazarYeri.Idefix,
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

                    catch (Exception ex)
                    {
                        data.HasErrors = true;
                        Logger.Error("IdefixService > UpdateRefIdDetailIdByGuid, MerchantId: {merchantId}, Guid: {guid}, ExecutionType: {ExecutionType}, Exception: {exception}, Requests: {@requestItems}", fileName: _logFolderName, data.MerchantId, guid, ExecutionType, ex, completedVerifyProduct ?? null);
                    }
                    data.CompletionDateTime = DateTime.Now;
                }
                else
                {
                    var requestItems = new IdefixGenericResponse<ProductInventoryItemWithBatchRequesIdResponse>();

                    foreach (var dataitm in dataItems)
                    {
                        var requestItem = new ProductInventoryItemWithBatchRequesIdResponse
                        {
                            DetailId = dataitm.DETAIL_ID,
                            ErrorMessages = new List<string> { result.StringContent },
                            RefId = dataitm.REF_ID,
                            Status = result.ResponseMessage.StatusCode.ToString(),
                            Thread_No = dataitm.THREAD_NO
                        };
                        requestItems.Items.Add(requestItem);
                    }

                    try
                    {
                        var pazarYeriJobResultDetails = await priceStockDalService.GetPazarYeriJobResultDetailsAsync(requestItems.RefId, requestItems.Thread_No, data.Guid, data.MerchantId);
                        var HasError = (requestItems.Status == null || requestItems.Status.ToUpper() == "SUCCESS") ? Character.H : Character.E;
                        var HasVerified = requestItems.Status == null ? Character.C : requestItems.Status.Substring(0, 1);

                        if (requestItems.Items != null)
                        {
                            foreach (var itm in requestItems.Items)
                            {
                                HasError = itm.ErrorMessages == null && requestItems.Status == "Done" ? Character.H : Character.E;

                                var detail = pazarYeriJobResultDetails.FirstOrDefault(x => x.DetailId == itm.DetailId);
                                if (detail == null)
                                    continue;

                                detail.HasVerified = HasVerified;
                                detail.HasErrors = HasError;

                                detailPazarYeriJobList.Add(detail);

                                logEntry = new PazarYeriLog
                                {
                                    RefId = itm.RefId,
                                    ExecutionType = data.ExecutionType.ToString(),
                                    PazarYeriBirimNo = data.MerchantId,
                                    ThreadNo = itm.Thread_No,
                                    LogType = itm.Status,
                                    Request = $"Guid: {guid}",
                                    Response = JsonConvert.SerializeObject(itm.ErrorMessages),
                                    HasErrors = Character.E,
                                    DetailId = itm.DetailId,
                                    PazarYeriNo = PazarYeri.Idefix,
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
                        Logger.Error("IdefixService > UpdateRefIdDetailIdByGuid, MerchantId: {merchantId}, Guid: {guid}, ExecutionType: {ExecutionType}, Exception: {exception}, Requests: {@requestItems}", fileName: _logFolderName, data.MerchantId, guid, ExecutionType, ex, requestItems ?? null);
                    }
                    data.CompletionDateTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                data.HasErrors = true;
                data.CompletionDateTime = DateTime.Now;
                data.ExceptionString = $"IdefixService > Idefix_VerifyPriceStock > Execution Type: {ExecutionType}, Exception: {ex.Message}";
                Logger.Error("IdefixService > Idefix_VerifyPriceStock > Execution Type: {ExecutionType}, Exception: {exception}", fileName: _logFolderName, ExecutionType, ex);
            }
            return data;
        }

        #endregion

    }
}