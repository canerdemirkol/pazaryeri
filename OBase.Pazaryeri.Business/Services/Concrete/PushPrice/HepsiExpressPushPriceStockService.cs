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
using OBase.Pazaryeri.Domain.Dtos.Getir;
using OBase.Pazaryeri.Domain.Dtos.Getir.PriceStock;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Constants.Constants.Db;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Services.Concrete.PushPrice
{
    public class HepsiExpressPushPriceStockService : IHepsiExpressPushPriceStockService
    {
        #region Variables

        private readonly IPriceStockDalService _priceStockDalService;
        private readonly IBaseService _baseService;
        private readonly IOptions<AppSettings> _options;
        private readonly IHepsiExpressClient _hepsiExpressClient;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string hepsiExpressLogFile = nameof(CommonEnums.PazarYerleri.HepsiExpress);
        private readonly IServiceScopeFactory _serviceScopeFactory;

        #endregion

        #region Ctor
        public HepsiExpressPushPriceStockService(IPriceStockDalService priceStockDalService,
        IBaseService baseService,
        IOptions<AppSettings> options,
        IHepsiExpressClient hepsiExpressClient,
        IServiceScopeFactory serviceScopeFactory)
        {
            _priceStockDalService = priceStockDalService;
            _baseService = baseService;
            _options = options;
            _hepsiExpressClient = hepsiExpressClient;
            _apiDefinition = _options.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.HepsiExpress);
            _serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Methods

        public async Task PushPriceStockAsync(Dictionary<string, string> properties, JobType executionType)
        {
            int NumberofProducts = int.Parse(properties[GetirConstants.Parameters.NumberofProducts]);
            string merchantNo = _apiDefinition?.Merchantno;
            executionType = JobType.All;

            List<long> refIds = await _priceStockDalService.GetAvailableRefIdsByMerchantNoAsync(executionType, merchantNo);

            Logger.Information("HepsiExpressService > PushPriceStock > Number of Jobs Available for Execution Type: {ExecutionType}, Job Count: {jobCount} , MerchantNo:{merchantNo}", fileName: hepsiExpressLogFile, executionType, refIds?.Count ?? 0, merchantNo);

            foreach (long refId in refIds)
            {
                var merchants = await _priceStockDalService.GetStoreNoListAsync(refId, _apiDefinition.Merchantno);
                List<Task> merchTasks = new List<Task>();
                var dataLst = new ConcurrentBag<CustomResult>();

                Logger.Information("HepsiExpressService > PushPriceStock > Execution Type: {executionType}, Number of Merchants: {merchantCount}", hepsiExpressLogFile, executionType, merchants.Count());



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
                        data = await HE_PostProductByMerchantWithVendor(priceStockService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("HepsiExpressService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {@data}", fileName: hepsiExpressLogFile, executionType, data);
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
                        Logger.Warning("HepsiExpressService > PushPriceStock > Execution Type: {ExecutionType}, Cannot update Repo for Ref_Id: {refId}", fileName: hepsiExpressLogFile, executionType, refId);
                }

                await _priceStockDalService.UpdateServiceLog(refId, SrvLogStatus, srvLogError);

                if (totalCount != SuccessCount)
                    Logger.Warning("HepsiExpressService > PushPriceStock > Execution Type: {ExecutionType}, Task Result Data: {count}", fileName: hepsiExpressLogFile, executionType, dataLst?.Count ?? 0);

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
            Logger.Information("HepsiExpressService > PushPriceStock is running for ExecutionType: {ExecutionType}", fileName: hepsiExpressLogFile, executionType);
            try
            {
                var merchants = await _priceStockDalService.GetStoreNoListAsync(refId, _apiDefinition.Merchantno);

                if (!merchants.Any())
                {
                    Logger.Information($"HepsiExpressService > PushPriceStock > Execution Type: {executionType} | no merchants found for refId:{refId}", fileName: hepsiExpressLogFile);
                    return dataLst.ToList();
                }

                List<Task> merchTasks = new();
                Logger.Information("HepsiExpressService > PushPriceStock > Execution Type: {ExecutionType}, Number of Merchants: {Count}", fileName: hepsiExpressLogFile, executionType, merchants.Count);

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
                        data = await HE_PostProductByMerchantWithVendor(priceStockService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("HepsiExpressService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {@data}", fileName: hepsiExpressLogFile, executionType, data);
                    }));
                }
                await Task.WhenAll(merchTasks);

            }
            catch (Exception ex)
            {
                Logger.Error("HepsiExpressService > PushPriceStock, Execution Type: {ExecutionType}, Exception occurred with Exception: {exception}", fileName: hepsiExpressLogFile, executionType, ex);
            }
            Logger.Information("HepsiExpressService > PushPriceStock is completed for ExecutionType: {executionType}", fileName: hepsiExpressLogFile, executionType);

            return dataLst.ToList();
        }

        public async Task VerifyPriceStockAsync(Dictionary<string, string> properties)
        {
            var executionType = JobType.VerifyPriceAndStock;
            var verifiableslst = await _priceStockDalService.GetAvailableVerifiablesAsync(_apiDefinition.Merchantno);

            List<IGrouping<string, MerchantVerify>> jobs = verifiableslst.GroupBy(f => f.GUID).ToList();
            Logger.Information("HepsiExpressService > HE_VerifyPriceStock > Number of Guids Available for Execution Type {executionType}: {count}", fileName: hepsiExpressLogFile, executionType, jobs?.Count ?? 0);

            List<Task> merchTasks = new List<Task>();
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
                    data = await HE_VerifyPostsByGuidAsync(priceStockService, data, executionType);
                    dataLst.Add(data);
                    Logger.Information("HEService > HE_VerifyPriceStock > Execution Type: {executionType}, Individual Task Result Data: {Guid}", fileName: hepsiExpressLogFile, executionType, data.Guid);
                }));
            }

            await Task.WhenAll(merchTasks);

            var totalCount = dataLst?.Count() ?? 0;
            var SuccessCount = dataLst?.Count(f => !f.HasErrors) ?? 0;

            if (totalCount != SuccessCount)
                Logger.Warning("HEService > HE_VerifyPriceStock > Execution Type:{executionType}, Task Result Data: {Count}", fileName: hepsiExpressLogFile, executionType, dataLst?.Count ?? 0);

        }

        #endregion

        #region Utilities

        private async Task<CustomResult> HE_PostProductByMerchantWithVendor(IPriceStockDalService priceStockDalService, CustomResult data)
        {
            data.StartDateTime = DateTime.Now;
            List<HEUpdateProductsReqDto> requests = new List<HEUpdateProductsReqDto>();

            try
            {
                var jobdetails = await priceStockDalService.GetJobResultDetailsByIdMerchAsync(data.RefId, data.MerchantId, data.MerchantNo, satisFiyatControl: true);
                data.DetailsCount = jobdetails.Count;

                jobdetails = _baseService.CalculateThreads(data.DetailsCount, data.NumberofProducts, jobdetails.ToList(), data.RefId, data.MerchantId, data.ThreadId).ToList();
                data.InternalThreadCount = jobdetails.Count;

                if (!jobdetails.Any())
                {
                    Logger.Warning($"HepsiExpressService > HE_PostProductByMerchantWithVendor > Execution Type: {data.ExecutionType}, No Details found for RefId: {data.RefId}, MerchantId: {data.MerchantId}", fileName: hepsiExpressLogFile);
                    return data;
                }
                #region Conventional For Loop
                for (int i = 1; i <= jobdetails.Max(f => f.ThreadNo); i++)
                {
                    HEUpdateProductsReqDto req = new HEUpdateProductsReqDto
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
                    req.requestItem = new Listings();
                    req.requestItem.Listing = new List<HEUpdateProductDto>();
                    List<HEListingDiscountRequestDto> listDiscount = new List<HEListingDiscountRequestDto>();

                    foreach (var det in tmpdetails)
                    {
                        Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");

                        HEUpdateProductDto RequestItem = new HEUpdateProductDto
                        {
                            #region WhichHasBeenSent
                            Price = decimal.Parse(det.SatisFiyat.HasValue ? det.SatisFiyat.Value.ToString() : "0,00m").ToString(),
                            HepsiburadaSku = det.PazarYeriMalNo,
                            MerchantSku = det.MalNo,
                            AvailableStock = det.StokMiktar,
                            DispatchTime = 0,
                            MaximumPurchasableQuantity = 0,
                            #endregion

                            #region ForReference
                            DetailId = det.DetailId,
                            RefId = det.RefId
                            #endregion
                        };
                        req.requestItem.Listing.Add(RequestItem);

                        if (det.IndirimliSatisFiyat > 0)
                        {
                            req.requestListingDiscount = new HEListingDiscountRequestDto()
                            {
                                #region WhichHasBeenSent
                                DiscountName = "indirim adi",
                                DiscountAmount = det.IndirimliSatisFiyat.Value,
                                ShortDescription = "indirim aciklama",
                                StartDate = det.IndirimBaslangicTarih.HasValue ? det.IndirimBaslangicTarih.Value.ToString("dd.MM.yyyy HH:mm") : string.Empty,
                                EndDate = det.IndirimBitisTarih.HasValue ? det.IndirimBitisTarih.Value.ToString("dd.MM.yyyy HH:mm") : string.Empty,
                                IncludedSkus = new List<string> { det.PazarYeriMalNo },
                                IsPaused = false,
                                #endregion

                                #region ForReference
                                DetailId = det.DetailId,
                                RefId = det.RefId
                                #endregion
                            };
                            listDiscount.Add(req.requestListingDiscount);
                        }
                    }

                    var requestStr = JsonConvert.SerializeObject(req.requestItem);

                    try
                    {
                        var result =await _hepsiExpressClient.UpdateListingProductStock(data.MerchantId, req.requestItem);

                        Logger.Information("HepsiExpressService > PushPriceStock > UpdateListingProductStock Response: {@response}", fileName: hepsiExpressLogFile, result);


                        req.RequestFailed = !(result.ResponseMessage.IsSuccessStatusCode);
                        req.HttpStatusCode = result.ResponseMessage.StatusCode;
                        req.APIResponse = result.StringContent;
                        var resultObject = JsonConvert.DeserializeObject<HEUpdateProductsRespDto>(result.StringContent);
                        req.Guid = result.ResponseMessage.IsSuccessStatusCode ? resultObject.id : string.Empty;

                        PazarYeriLog logEntry = new PazarYeriLog
                        {
                            RefId = data.RefId,
                            ExecutionType = data.ExecutionType.ToString(),
                            PazarYeriBirimNo = data.MerchantId,
                            ThreadNo = req.Thread_No,
                            LogType = result.ResponseMessage.IsSuccessStatusCode ? nameof(LogType.AllCompleted) : nameof(LogType.AllFailed),
                            Request = requestStr,
                            Response = $"HTTP StatusCode: {result.ResponseMessage.StatusCode} >  {result.StringContent}",
                            HasErrors = result.ResponseMessage.IsSuccessStatusCode ? Character.H : Character.E,
                            DetailId = 0,
                            PazarYeriNo = data.MerchantNo,
                            Guid = !req.RequestFailed ? resultObject.id : string.Empty
                        };
                        Logger.Information("HepsiExpressService > HE_PostProductByMerchant > PushPriceStockAsync() > logEntry {@logEntry}", hepsiExpressLogFile, logEntry);
                                               
                        #region Discount
                        foreach (var discount in listDiscount)
                        {
                            var discountRequestStr = JsonConvert.SerializeObject(discount);

                            var discountResult = _hepsiExpressClient.InsertDiscount(data.MerchantId, discount).Result;

                            var discountResultObject = JsonConvert.DeserializeObject<HEListingDiscountResponseDto>(discountResult.StringContent);

                            if (discountResult.ResponseMessage.StatusCode == HttpStatusCode.Accepted)
                            {
                                PazarYeriLog logEntries = new PazarYeriLog
                                {
                                    RefId = data.RefId,
                                    ExecutionType = data.ExecutionType.ToString(),
                                    PazarYeriBirimNo = data.MerchantId,
                                    ThreadNo = req.Thread_No,
                                    LogType = nameof(LogType.AllCompleted),
                                    Request = requestStr,
                                    Response = $"HTTP StatusCode: {result.ResponseMessage.StatusCode} >  {req.APIResponse}",
                                    HasErrors = Character.E,
                                    DetailId = 0,
                                    PazarYeriNo = data.MerchantNo,
                                    Guid = string.Empty
                                };
                                Logger.Information("HepsiExpressService > HE_PostProductByMerchant > PushPriceStockAsync() > discountResult > logEntry {@logEntry}", hepsiExpressLogFile, logEntry);
                            }
                            else
                            {
                                PazarYeriLog logEntries = new PazarYeriLog
                                {
                                    RefId = data.RefId,
                                    ExecutionType = data.ExecutionType.ToString(),
                                    PazarYeriBirimNo = data.MerchantId,
                                    ThreadNo = req.Thread_No,
                                    LogType = nameof(LogType.AllFailed),
                                    Request = requestStr,
                                    Response = $"HTTP StatusCode: {result.ResponseMessage.StatusCode} >  {req.APIResponse}",
                                    HasErrors = Character.E,
                                    DetailId = 0,
                                    PazarYeriNo = data.MerchantNo,
                                    Guid = string.Empty
                                };
                                Logger.Information("HepsiExpressService > HE_PostProductByMerchant > PushPriceStockAsync() > discountResult > logEntry {@logEntry}", hepsiExpressLogFile, logEntry);
                                await priceStockDalService.InsertPazarYeriLogAsync(logEntry);
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        req.RequestFailed = true;
                        Logger.Error("HepsiExpressService > HE_PostProductByMerchant > Execution Type: {ExecutionType}, Inner Exception for Ref_Id: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}, Thread_No: {ThreadNo} - Exception: {exception}", fileName: hepsiExpressLogFile, (data?.ExecutionType.ToString() ?? ""), data?.RefId.ToString() ?? "", data.MerchantId ?? "", (data?.ThreadId.ToString() ?? ""), (req?.Thread_No.ToString() ?? ""), ex);

                        PazarYeriLog logEntry = new PazarYeriLog
                        {
                            RefId = data.RefId,
                            ExecutionType = data.ExecutionType.ToString(),
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
                    foreach (var individualItem in requestItem.requestItem.Listing)
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

                        Logger.Information("HepsiExpressService > HE_PostProductByMerchant > Execution Type: {ExecutionType}, Requests updates successfully in Repo Db/for RefId: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}", fileName: hepsiExpressLogFile, data?.ExecutionType, data?.RefId, data?.MerchantId, requestItem?.Thread_No);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("HepsiExpressService > HE_PostProductByMerchant > Execution Type: {ExecutionType}, Cannot update requests successfully in Db/Repo for RefId: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId},Exception : {ex}", fileName: hepsiExpressLogFile, data?.ExecutionType, data?.RefId, data?.MerchantId, requestItem?.Thread_No, ex);
                    }
                    stopwatch.Stop();
                    Logger.Information("HepsiExpressService UpdatePazarYeriJobResultDetailRangeAsync finished in {elapsedTime}ms.", hepsiExpressLogFile, stopwatch.ElapsedMilliseconds);
                    #endregion

                    data.CompletionDateTime = DateTime.Now;

                    if (requests.Count(f => !f.RequestFailed) != requests.Count)
                    {
                        data.HasErrors = true;
                        Logger.Warning("HepsiExpressService > HE_PostProductByMerchant > Execution Type: {ExecutionType}, Completed partially for MerchantId: {MerchantId} for Execution Data: {@data}", fileName: hepsiExpressLogFile, data?.ExecutionType, data?.MerchantId, data);
                    }
                    else
                    {
                        data.HasErrors = false;
                        Logger.Information("HepsiExpressService > HE_PostProductByMerchant > Execution Type: {ExecutionType}, Completed successfully for MerchantId: {MerchantId} for Execution Data: {@data}", fileName: hepsiExpressLogFile, data?.ExecutionType, data?.MerchantId, data);
                    }
                }
            }
            catch (Exception ex)
            {
                data.CompletionDateTime = DateTime.Now;
                data.HasErrors = true;
                data.ExceptionsString = ex.Message;

                Logger.Error("HepsiExpressService > HE_PostProductByMerchant > Execution Type: {ExecutionType}, Failed with Exception for MerchantId: {MerchantId} for Execution Data: {@data}, Exception: {exception}", fileName: hepsiExpressLogFile, data?.ExecutionType, data?.MerchantId, data, ex);
            }
            return data;
        }

        private async Task<CustomVerifyResult> HE_VerifyPostsByGuidAsync(IPriceStockDalService priceStockDalService, CustomVerifyResult data, JobType ExecutionType)
        {
            Logger.Information("HEService > HE_VerifyPriceStock > Verifiable Data: {MerchantVerifiesCount}", fileName: hepsiExpressLogFile, data.MerchantVerifies?.Count ?? 0);

            data.StartDateTime = DateTime.Now;
            string guid = data.Guid;
            string merchantId = _apiDefinition.SupplierId;

            try
            {
                var result = await _hepsiExpressClient.GetRequestsControlById(merchantId, guid);

                data.RequestCompletionDateTime = DateTime.Now;
                List<PazarYeriLog> pazarYeriLogs = new List<PazarYeriLog>();
                List<PazarYeriJobResultDetails> detailPazarYeriJobList = new List<PazarYeriJobResultDetails>();
                PazarYeriLog logEntry;
                if (result.ResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    var results = JsonConvert.DeserializeObject<HEGetRequestResponseDto>(result.StringContent);
                    var requestItems = results;
                    var dataItems = data.MerchantVerifies;

                    if (requestItems.RequestItems != null && requestItems.RequestItems.Any())
                    {
                        foreach (var requestItm in requestItems.RequestItems)
                        {
                            var tmp = dataItems.FirstOrDefault(f => f.BARKOD == requestItm.HepsiburadaSku);

                            if (tmp != null)
                            {
                                requestItm.RefId = tmp.REF_ID;
                                requestItm.DetailId = tmp.DETAIL_ID;
                                requestItm.ThreadNo = tmp.THREAD_NO;
                            }
                        }
                    }
                    if (requestItems != null)
                    {
                        var tmp = dataItems.FirstOrDefault(f => f.GUID == requestItems.id);

                        if (tmp != null)
                        {
                            requestItems.RefId = tmp.REF_ID;
                            requestItems.DetailId = tmp.DETAIL_ID;
                            requestItems.ThreadNo = tmp.THREAD_NO;
                        }
                    }

                    try
                    {
                        var pazarYeriJobResultDetails = await priceStockDalService.GetPazarYeriJobResultDetailsAsync(requestItems.RefId, requestItems.ThreadNo, data.Guid, data.MerchantId);

                        if (requestItems.RequestItems != null && requestItems.RequestItems.Any())
                        {
                            foreach (var itm in pazarYeriJobResultDetails)
                            {
                                #region Update PazaryeriJobResultDetails

                                if (requestItems.RequestItems?.Any(x => x.HepsiburadaSku == itm.PazarYeriMalNo) ?? false)
                                {
                                    itm.HasVerified = Character.H;
                                    itm.HasErrors = Character.E;
                                }
                                else
                                {
                                    itm.HasVerified = Character.E;
                                    itm.HasErrors = Character.H;
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
                                        LogType = requestItems.Status,
                                        Request = $"Guid: {guid}",
                                        Response = JsonConvert.SerializeObject(requestItems),
                                        HasErrors = Character.E,
                                        DetailId = itm.DetailId,
                                        PazarYeriNo = PazarYeri.HepsiExpress,
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
                        Logger.Error("HEService > UpdateRefIdDetailIdByGuid, MerchantId: {merchantId}, Guid: {guid}, ExecutionType: {ExecutionType}, Exception: {exception}, Requests: {@requestItems}", fileName: hepsiExpressLogFile, merchantId, guid, ExecutionType, ex, requestItems ?? null);
                    }
                    data.CompletionDateTime = DateTime.Now;
                }
                else
                {
                    var requestItems = new HEGetRequestResponseDto();

                    foreach (var dataitm in data.MerchantVerifies)
                    {
                        var requestItem = new RequestIdResponseItem
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
                        var pazarYeriJobResultDetails = await priceStockDalService.GetPazarYeriJobResultDetailsAsync(requestItems.RefId, requestItems.ThreadNo, data.Guid, data.MerchantId);
                        var HasError = (requestItems.Status == null || requestItems.Status.ToUpper() == "SUCCESS") ? Character.H : Character.E;
                        var HasVerified = requestItems.Status == null ? Character.C : requestItems.Status.Substring(0, 1);

                        if (requestItems.RequestItems != null)
                        {
                            foreach (var itm in requestItems.RequestItems)
                            {
                                HasError = itm.ErrorMessages == null && requestItems.Status == "Done" ? Character.H : Character.E;

                                var detail = await priceStockDalService.GetPazarYeriJobResultDetailAsync(requestItems.RefId, requestItems.DetailId, requestItems.ThreadNo, data.Guid, data.MerchantId);

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
                                    ThreadNo = itm.ThreadNo,
                                    LogType = itm.Status,
                                    Request = $"MerchantId: {merchantId}, Guid: {guid}",
                                    Response = JsonConvert.SerializeObject(itm.ErrorMessages),
                                    HasErrors = Character.E,
                                    DetailId = itm.DetailId,
                                    PazarYeriNo = PazarYeri.HepsiExpress,
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
                        Logger.Error("HEService > UpdateRefIdDetailIdByGuid, MerchantId: {merchantId}, Guid: {guid}, ExecutionType: {ExecutionType}, Exception: {exception}, Requests: {@requestItems}", fileName: hepsiExpressLogFile, merchantId, guid, ExecutionType, ex, requestItems ?? null);
                    }
                    data.CompletionDateTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                data.HasErrors = true;
                data.CompletionDateTime = DateTime.Now;
                data.ExceptionString = $"HEService > HE_VerifyPostsByGuid > Execution Type: {ExecutionType}, Exception: {ex.Message}";
                Logger.Error("HEService > HE_VerifyPostsByGuid > Execution Type: {ExecutionType}, Exception: {exception}", fileName: hepsiExpressLogFile, ExecutionType, ex);
            }
            return data;
        }

        #endregion

    }
}