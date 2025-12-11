using System.Data;
using System.Net;
using Hangfire;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.BackgroundJobs.YemekSepeti;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.DataAccess.Services.Abstract.PriceStock;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using OBase.Pazaryeri.Domain.Dtos.Getir.PriceStock;
using System.Linq;
using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace OBase.Pazaryeri.Business.Services.Concrete.PushPrice
{
    public class YemekSepetiPushPriceStockService : IYemekSepetiPushPriceStockService
    {
        #region Variables

        private readonly IPriceStockDalService _priceStockDalService;
        private readonly IYemekSepetiClient _yemekSepetiClient;
        private readonly IBaseService _baseService;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string ysLogfile = nameof(PazarYerleri.YemekSepeti);
        private readonly IServiceScopeFactory _serviceScopeFactory;
        #endregion

        #region Constructor

        public YemekSepetiPushPriceStockService(IPriceStockDalService priceStockDalService, IYemekSepetiClient yemekSepetiClient, IBaseService baseService, IOptions<AppSettings> options, IServiceScopeFactory serviceScopeFactory)
        {
            _priceStockDalService = priceStockDalService;
            _yemekSepetiClient = yemekSepetiClient;
            _baseService = baseService;
            _apiDefinition = options.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Yemeksepeti);
            _serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Methods

        public async Task PushPriceStockAsync(Dictionary<string, string> properties)
        {
            int NumberofProducts = int.Parse(properties[YemekSepetiConstants.Parameters.NumberofProducts]);
            string merchantNo = _apiDefinition?.Merchantno;
            JobType executionType = JobType.All;

            List<long> refIds = await _priceStockDalService.GetAvailableRefIdsByMerchantNoAsync(executionType, merchantNo);

            Logger.Information("YSService > PushPriceStock > Number of Jobs Available for Execution Type {ExecutionType}: {Count}", fileName: ysLogfile, executionType, (refIds?.Count ?? 0));

            foreach (var refId in refIds)
            {
                var pazarYeriBirimNos = await _priceStockDalService.GetStoreNoListAsync(refId, merchantNo);
                List<Task> merchTasks = new();
                ConcurrentBag<CustomResult> dataLst = new();

                Logger.Information("YSService > PushPriceStock > Execution Type: {ExecutionType}, Number of Merchants: {Count}", fileName: ysLogfile, executionType, (pazarYeriBirimNos?.Count ?? 0));

                if (pazarYeriBirimNos.Count == 0)
                    continue;


                foreach (var pazarYeriBirimNo in pazarYeriBirimNos)
                {
                    merchTasks.Add(Task.Run(async () =>
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var priceStockService = scope.ServiceProvider.GetRequiredService<IPriceStockDalService>();

                        var data = new CustomResult() { MerchantId = pazarYeriBirimNo, CreationDateTime = DateTime.Now };

                        data.RefId = refId;
                        data.NumberofProducts = NumberofProducts;
                        data.ThreadId = Environment.CurrentManagedThreadId;
                        data.InternalThreadCount = 0;
                        data.FailedThreadCount = 0;
                        data.SuccessfulThreadCount = 0;
                        data.ExecutionType = executionType;
                        data.MerchantNo = merchantNo;
                        data.MerchantId = pazarYeriBirimNo;
                        data.CreationDateTime = DateTime.Now;
                        data = await PushProductByMerchant(priceStockService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("YSService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {@data}", fileName: ysLogfile, executionType, data);
                    }));
                }
                await Task.WhenAll(merchTasks);

                var totalCount = dataLst.Count();
                var FailedCount = dataLst.Count(f => f.HasErrors);
                var SuccessCount = dataLst?.Count(f => !f.HasErrors);
                var HasErrors = (totalCount != SuccessCount) ? Character.E : Character.H;
                var HasErrorStr = HasErrors == Character.E ? "partially" : "successfully";
                var srvLogErrorPhrase = (totalCount != SuccessCount) ? ", LOG TABLOLARINI KONTROL EDINIZ" : "";
                var srvLogError = $"TOPLAM BIRIM SAY.: {totalCount}, BASARILI BIRIM SAY.: {SuccessCount}, BASARISIZ BIRIM SAY.: {FailedCount}{srvLogErrorPhrase}";
                var SrvLogStatus = (totalCount != SuccessCount) ? "HATA" : "TAMAMLANDI";

                var pazarYeriJobResult = await _priceStockDalService.GetPazarYeriJobResultByRefIdAsync(refId);
                if (pazarYeriJobResult != null) pazarYeriJobResult.NumberOfThreads = dataLst?.Count;
                {
                    pazarYeriJobResult.ThreadSize = NumberofProducts;
                    pazarYeriJobResult.NumberOfThreads = dataLst?.Count;
                    pazarYeriJobResult.HasErrors = HasErrors;
                    pazarYeriJobResult.HasSent = Character.E;

                    var updateResponse = await _priceStockDalService.UpdatePazarYeriJobResultAsync(pazarYeriJobResult);

                    if (!updateResponse)
                        Logger.Warning($"YSService > PushPriceStock > Execution Type: {executionType}, Cannot update Repo for Ref_Id: {refId}", fileName: ysLogfile);

                }

                await _priceStockDalService.UpdateServiceLog(refId, SrvLogStatus, srvLogError);

                if (totalCount != SuccessCount)
                {
                    Logger.Warning("YSService > PushPriceStock > Execution Type: {ExecutionType}, Task Result Data: {count}", fileName: ysLogfile, executionType, (dataLst?.Count ?? 0));
                }
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
            Logger.Information("YSService > PushPriceStock is running for ExecutionType: {ExecutionType}", fileName: ysLogfile, executionType);
            try
            {
                var pazarYeriBirimNos = await _priceStockDalService.GetStoreNoListAsync(refId, merchantNo);
                if (!pazarYeriBirimNos.Any())
                {
                    Logger.Information($"YSService > PushPriceStock > Execution Type: {executionType} | no merchants found for refId:{refId}", fileName: ysLogfile);
                    return dataLst.ToList();
                }
                List<Task> merchTasks = new();
                Logger.Information("YSService > PushPriceStock > Execution Type: {ExecutionType}, Number of Merchants: {Count}", fileName: ysLogfile, executionType, (pazarYeriBirimNos?.Count ?? 0));

                foreach (var pazarYeriBirimNo in pazarYeriBirimNos)
                {
                    merchTasks.Add(Task.Run(async () =>
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var priceStockService = scope.ServiceProvider.GetRequiredService<IPriceStockDalService>();

                        var data = new CustomResult() { MerchantId = pazarYeriBirimNo, CreationDateTime = DateTime.Now };

                        data.RefId = refId;
                        data.NumberofProducts = NumberofProducts;
                        data.ThreadId = Environment.CurrentManagedThreadId;
                        data.InternalThreadCount = 0;
                        data.FailedThreadCount = 0;
                        data.SuccessfulThreadCount = 0;
                        data.ExecutionType = executionType;
                        data.MerchantNo = merchantNo;
                        data.MerchantId = pazarYeriBirimNo;
                        data.CreationDateTime = DateTime.Now;
                        data = await PushProductByMerchant(priceStockService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("YSService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {@data}", fileName: ysLogfile, executionType, data);
                    }));

                }

                await Task.WhenAll(merchTasks);

            }
            catch (Exception ex)
            {
                Logger.Error("YSService > PushPriceStock, Execution Type: {ExecutionType}, Exception occurred with Exception: {exception}", fileName: ysLogfile, executionType, ex);
            }
            Logger.Information("YSService > PushPriceStock is completed for ExecutionType: {ExecutionType}", fileName: ysLogfile, executionType);

            return dataLst.ToList();
        }

        public ServiceResponse<string> SendVerifyUrl(YemekSepetiVerifyRequestDto request)
        {
            Dictionary<string, string> properties = new()
            {
                { YemekSepetiConstants.Parameters.Request, JsonSerializer.Serialize(request) }
            };
            var jobId = BackgroundJob.Enqueue<YemekSepetiVerifyPriceStockJob>(x => x.RunJobAsync(properties, JobCancellationToken.Null));
            return ServiceResponse<string>.Success($"Datas will consumed by {jobId}");
        }
        public async Task VerifyPriceStockAsync(Dictionary<string, string> properties)
        {

            var request = JsonSerializer.Deserialize<YemekSepetiVerifyRequestDto>(properties[YemekSepetiConstants.Parameters.Request]);

            CustomVerifyResult data = new()
            {
                StartDateTime = DateTime.Now,
                Guid = request.JobId.ToString(),
                DetailsCount = 0,
                FailedCount = 0,
                ProcessingCount = 0,
                SuccessfulCount = 0,
                ExecutionType = JobType.VerifyPriceAndStock
            };

            var pazarYeriJobResultDetails = await _priceStockDalService.GetListAsync<PazarYeriJobResultDetails>(x => x.PazarYeriNo == _apiDefinition.Merchantno
                && x.HasVerified == Character.H
                && x.HasErrors == Character.H
                && x.HasSent == Character.E
                && x.Guid == data.Guid);

            data.MerchantVerifies = pazarYeriJobResultDetails.Select(x => new MerchantVerify
            {
                DETAIL_ID = x.DetailId,
                REF_ID = x.RefId,
                PAZAR_YERI_BIRIM_NO = x.PazarYeriBirimNo,
                PAZAR_YERI_MAL_NO = x.PazarYeriMalNo,
                THREAD_NO = x.ThreadNo,
                MAL_NO = x.MalNo,
                GUID = x.Guid,
                BARKOD = x.Barkod
            }).ToList();

            Logger.Information("YSService > VerifyPriceStockAsync > Verifiable Data: MerchantVerifies Count => {Count}", fileName: ysLogfile, data.MerchantVerifies?.Count);
            try
            {

                List<PazarYeriJobResultDetails> detailsToUpdate = new List<PazarYeriJobResultDetails>();

                var response = await _yemekSepetiClient.VerifyPriceStockAsync(request.DownloadUrl);
                data.RequestCompletionDateTime = DateTime.Now;

                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    var csvData = response.StringContent;
                    string jobId = request.JobId.ToString();
                    var refId = data.MerchantVerifies.FirstOrDefault().REF_ID;
                    var requestItems = ParseCsv(request, csvData, refId);
                    data.MerchantId = request.PlatformVendorId;

                    if (requestItems == null)
                    {
                        Logger.Error($"YSService > VerifyPriceStockAsync > requestItems is null! guid => {data.Guid}", fileName: ysLogfile);
                        return;
                    }

                    if (!requestItems.Any())
                    {
                        Logger.Error($"YSService > VerifyPriceStockAsync > requestItems has no items! guid => {data.Guid}", fileName: ysLogfile);
                        return;
                    }

                    foreach (var requestItm in requestItems)
                    {
                        var dataItem = data.MerchantVerifies.FirstOrDefault(f => f.PAZAR_YERI_MAL_NO == requestItm.Sku);

                        if (dataItem != null)
                        {
                            requestItm.RefId = dataItem.REF_ID;
                            requestItm.DetailId = dataItem.DETAIL_ID;
                            requestItm.ThreadNo = dataItem.THREAD_NO;
                        }
                    }

                    foreach (var item in requestItems)
                    {
                        #region Update PazaryeriJobResultDetails
                        var HasVerified = !string.IsNullOrEmpty(item.State) ? item.State[..1] : string.Empty;
                        var HasError = string.IsNullOrWhiteSpace(item.Errors) ? Character.H : Character.E;
                        if (pazarYeriJobResultDetails.Any())
                        {
                            var detailItem = pazarYeriJobResultDetails.Find(f => f.DetailId == item.DetailId);
                            if (detailItem != null)
                            {
                                detailItem.HasVerified = HasVerified;
                                detailItem.HasErrors = HasError;

                                detailsToUpdate.Add(detailItem);
                                //await _priceStockDalService.UpdatePazarYeriJobResultDetailAsync(detailItem);
                            }
                        }
                        #endregion
                        #region Insert Pazaryeri Log
                        if (HasError == Character.E)
                        {
                            PazarYeriLog logEntry = new()
                            {
                                RefId = item.RefId,
                                ExecutionType = data.ExecutionType.ToString(),
                                PazarYeriBirimNo = data.MerchantId,
                                ThreadNo = item.ThreadNo,
                                LogType = request.Status,
                                Request = properties[YemekSepetiConstants.Parameters.Request],
                                Response = item.Errors,
                                HasErrors = Character.E,
                                DetailId = item.DetailId,
                                PazarYeriNo = PazarYeri.Yemeksepeti,
                                Guid = data.Guid
                            };
                            await _priceStockDalService.InsertPazarYeriLogAsync(logEntry);
                        }
                        #endregion
                    }

                    if (detailsToUpdate.Any())
                        await _priceStockDalService.UpdatePazarYeriJobResultDetailRangeVerifiedAsync(detailsToUpdate);

                    data.HasErrors = false;

                }
                else
                {
                    var requestItems = new GetRequestsByIdRespDto();
                    foreach (var dataitm in data.MerchantVerifies)
                    {
                        var requestItem = new BaseRequestIdResponseItem
                        {
                            DetailId = dataitm.DETAIL_ID,
                            ErrorMessages = new List<string> { response.StringContent },
                            RefId = dataitm.REF_ID,
                            Status = response.ResponseMessage.StatusCode.ToString(),
                            ThreadNo = dataitm.THREAD_NO
                        };
                        requestItems.RequestItems.Add(requestItem);
                    }
                    try
                    {
                        var HasError = (requestItems.Status == null || requestItems.Status.ToUpper() == "SUCCESS") ? Character.H : Character.E;
                        var HasVerified = requestItems.Status == null ? Character.C : requestItems.Status[..1];
                        //if (pazarYeriJobResultDetails != null)
                        //{
                        //    foreach (var item in pazarYeriJobResultDetails)
                        //    {
                        //        item.HasVerified = Character.D;
                        //    }

                        //    await _priceStockDalService.UpdateRangeAsync(pazarYeriJobResultDetails);
                        //}
                        if (requestItems.RequestItems != null)
                        {
                            foreach (var itm in requestItems.RequestItems)
                            {
                                HasError = itm.ErrorMessages == null && requestItems.Status == "Done" ? Character.H : Character.E;

                                var detail = await _priceStockDalService.GetPazarYeriJobResultDetailAsync(requestItems.RefId, requestItems.DetailId, requestItems.Thread_No, data.Guid, data.MerchantId);

                                detail.HasVerified = HasVerified;
                                detail.HasErrors = HasError;

                                detailsToUpdate.Add(detail);
                                //await _priceStockDalService.UpdatePazarYeriJobResultDetailAsync(detail);

                                PazarYeriLog logEntry = new()
                                {
                                    RefId = requestItems.RefId,
                                    ExecutionType = data.ExecutionType.ToString(),
                                    PazarYeriBirimNo = data.MerchantId,
                                    ThreadNo = requestItems.Thread_No,
                                    LogType = requestItems.Status,
                                    Request = properties[YemekSepetiConstants.Parameters.Request],
                                    Response = string.Join(',', itm.ErrorMessages),
                                    HasErrors = Character.E,
                                    DetailId = requestItems.DetailId,
                                    PazarYeriNo = PazarYeri.Yemeksepeti,
                                    Guid = data.Guid
                                };
                                await _priceStockDalService.InsertPazarYeriLogAsync(logEntry);
                            }
                        }
                        data.HasErrors = false;

                        if (detailsToUpdate.Any())
                            await _priceStockDalService.UpdatePazarYeriJobResultDetailRangeVerifiedAsync(detailsToUpdate);
                    }
                    catch (Exception ex)
                    {
                        data.HasErrors = true;
                        Logger.Error("YSService > VerifyPriceStockAsync MerchantId: {merchantId}, Guid: {guid}, ExecutionType: {ExecutionType}, Exception: {exception}, Requests: {@requestItems}", fileName: ysLogfile, data.MerchantId, data.Guid, data.ExecutionType, ex, requestItems);
                    }
                    data.CompletionDateTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                data.HasErrors = true;
                data.CompletionDateTime = DateTime.Now;
                data.ExceptionString = $"YSService > VerifyPriceStockAsync > Execution Type: {data.ExecutionType}, Exception: {ex.Message}";
                Logger.Error("YSService > VerifyPriceStockAsync > Execution Type: {ExecutionType}, Exception: {exception}", fileName: ysLogfile, data.ExecutionType, ex);
            }

        }

        #endregion

        #region Private Methods

        private List<YemekSepetiVerifyResponseProductDto> ParseCsv(YemekSepetiVerifyRequestDto request, string csvData, long refId)
        {
            var products = new List<YemekSepetiVerifyResponseProductDto>();

            // Split the data by newline characters to get rows
            string[] rows = csvData.Split('\n');

            // Skip the first row as it contains headers
            for (int i = 1; i < rows.Length; i++)
            {
                string row = rows[i].Trim();

                if (string.IsNullOrEmpty(row))
                    continue;

                // Split each row by commas to get individual values
                string[] values = row.Split(',');

                try
                {
                    YemekSepetiVerifyResponseProductDto product = new()
                    {
                        Sku = values[0].Trim('\"'),
                        Code = values[1].Trim('\"'),
                        State = values[2].Trim('\"'),
                        Errors = values[3].Trim('\"'),
                        RowNumber = int.Parse(values[4].Trim('\"')),
                        PieceBarcode = values[5].Trim('\"')
                    };

                    products.Add(product);
                }
                catch (Exception ex)
                {
                    Logger.Error("YSService > VerifyPriceStockAsync > ParseCsv > Request: {request} Inner Exception for Ref_Id: {RefId} Exception: {exception}", fileName: ysLogfile, request, refId, ex);
                }
            }

            return products;
        }

        private async Task<CustomResult> PushProductByMerchant(IPriceStockDalService priceStockDalService, CustomResult data)
        {

            data.StartDateTime = DateTime.Now;
            List<YemekSepetiPriceStockRequestDto> requests = new();

            try
            {
                var jobdetails = await priceStockDalService.GetJobResultDetailsByIdMerchAsync(data.RefId, data.MerchantId, data.MerchantNo);

                data.DetailsCount = jobdetails.Count;

                jobdetails = _baseService.CalculateThreads(data.DetailsCount, data.NumberofProducts, jobdetails.ToList(), data.RefId, data.MerchantId, data.ThreadId).ToList();
                data.InternalThreadCount = jobdetails.Count;

                if (jobdetails.Any())
                {
                    #region Conventional For Loop
                    for (int i = 1; i <= jobdetails.Max(f => f.ThreadNo); i++)
                    {
                        YemekSepetiPriceStockRequestDto req = new()
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
                        req.Products = tmpdetails.Select(x => new YemekSepetiPushPriceStockRequestProductDto
                        {
                            Price = x.IndirimliSatisFiyat.HasValue && x.IndirimliSatisFiyat > 0 ? x.IndirimliSatisFiyat.Value : (x.SatisFiyat ?? 0),
                            Quantity = x.StokMiktar ?? 0,
                            Active = x.AktifPasifEh == "E",
                            Sku = x.PazarYeriMalNo,
                            RefId = x.RefId,
                            DetailId = x.DetailId,
                            Thread_No = x.ThreadNo
                        }).ToList();

                        var requestStr = JsonSerializer.Serialize(req);

                        try
                        {
                            var result = await _yemekSepetiClient.PushPriceStockAsync(data.MerchantId, req);

                            req.RequestFailed = !(result.ResponseMessage.IsSuccessStatusCode);
                            req.HttpStatusCode = result.ResponseMessage.StatusCode;
                            req.APIResponse = result.StringContent;
                            var jsonOptions = new JsonSerializerOptions();
                            jsonOptions.Converters.Add(new JsonStringEnumConverter());
                            Logger.Information("YSService PushProductByMerchant> PushPriceStockAsync() Result {result}", ysLogfile, result.StringContent);
                            var resultObject = JsonSerializer.Deserialize<YemekSepetiPriceStockResponseDto>(result.StringContent, jsonOptions);
                            req.Guid = !req.RequestFailed ? resultObject.JobId?.ToString() ?? "" : string.Empty;

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
                                Guid = !req.RequestFailed ? resultObject.JobId?.ToString() ?? "" : string.Empty
                            };
                            //await priceStockDalService.InsertPazarYeriLogAsync(logEntry);
                            Logger.Information("YSService PushProductByMerchant> PushPriceStockAsync() logEntry {@logEntry}", ysLogfile, logEntry);
                        }
                        catch (Exception ex)
                        {
                            req.RequestFailed = true;
                            req.ResultException = $"YSService > PushProductByMerchant > Execution Type: {data.ExecutionType}, Inner Exception for Ref_Id: {data.RefId}, MerchantId: {data.MerchantId}, ThreadId: {data.ThreadId}, Thread_No: {req.Thread_No} - HTTP: {ex.Message}, with StackTrace: {ex.StackTrace}";


                            Logger.Error("YSService > PushProductByMerchant > Execution Type: {ExecutionType}, Inner Exception for Ref_Id: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}, Thread_No: {ThreadNo} - Exception: {exception}", fileName: ysLogfile, (data?.ExecutionType.ToString() ?? ""), data?.RefId.ToString() ?? "", data.MerchantId ?? "", (data?.ThreadId.ToString() ?? ""), (req?.Thread_No.ToString() ?? ""), ex);

                            PazarYeriLog logEntry = new()
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
                        List<PazarYeriJobResultDetails> updatedDetailList = new();
                        foreach (var individualItem in requestItem.Products)
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
                            if (updatedDetailList.Count > 0)
                                await priceStockDalService.UpdatePazarYeriJobResultDetailRangeAsync(updatedDetailList);

                            Logger.Information("YSService > PushProductByMerchant > Execution Type: {ExecutionType}, Requests updates successfully in Repo Db/for RefId: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId} , ItemCount:{Count}", fileName: ysLogfile, data?.ExecutionType, data?.RefId, data?.MerchantId, requestItem?.Thread_No,updatedDetailList.Count);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("YSService > PushProductByMerchant > Execution Type: {ExecutionType}, Cannot update requests successfully in Db/Repo for RefId: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}, ItemCount:{Count},Exception : {ex}", fileName: ysLogfile, data?.ExecutionType, data?.RefId, data?.MerchantId, requestItem?.Thread_No,updatedDetailList.Count, ex);
                        }
                    }

                    stopwatch.Stop();
                    Logger.Information("YSService UpdatePazarYeriJobResultDetailRangeAsync finished in {elapsedTime}ms.", ysLogfile, stopwatch.ElapsedMilliseconds);

                    #endregion

                    data.CompletionDateTime = DateTime.Now;

                    if (requests.Count(f => !f.RequestFailed) != requests.Count)
                    {
                        data.HasErrors = true;
                        Logger.Warning("YSService > PushProductByMerchant > Execution Type: {ExecutionType}, Completed partially for MerchantId: {MerchantId} for Execution Data: {data}", fileName: ysLogfile, data?.ExecutionType, data?.MerchantId, JsonSerializer.Serialize(data) ?? "");
                    }
                    else
                    {
                        data.HasErrors = false;
                        Logger.Information("YSService > PushProductByMerchant > Execution Type: {ExecutionType}, Completed successfully for MerchantId: {MerchantId} for Execution Data: {data}", fileName: ysLogfile, data?.ExecutionType, data?.MerchantId, JsonSerializer.Serialize(data) ?? "");
                    }
                }
                else
                {
                    Logger.Warning("YSService > PushProductByMerchant > Execution Type: {ExecutionType}, No Details found for RefId: {RefId}, MerchantId: {MerchantId}", fileName: ysLogfile, data.ExecutionType, data.RefId, data.MerchantId);
                }
            }

            catch (Exception ex)
            {
                data.CompletionDateTime = DateTime.Now;
                data.HasErrors = true;
                data.ExceptionsString = ex.Message;

                Logger.Error("YSService > PushProductByMerchant > Execution Type: {ExecutionType}, Failed with Exception for MerchantId: {MerchantId} for Execution Data: {data}, Exception: {exception}", fileName: ysLogfile, data?.ExecutionType, data?.MerchantId, JsonSerializer.Serialize(data) ?? "", ex);
            }
            return data;
        }

        #endregion
    }
}