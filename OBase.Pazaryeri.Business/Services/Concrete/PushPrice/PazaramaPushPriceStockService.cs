using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Abstract.PriceStock;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Pazarama.PushPrice;
using OBase.Pazaryeri.Domain.Entities;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Net;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Services.Concrete.PushPrice
{
    public class PazaramaPushPriceStockService : IPazaramaPushPriceStockService
    {
        #region Variables

        private readonly ApiDefinitions _apiDefinition;
        private readonly IPriceStockDalService _priceStockDalService;
        private readonly IPazarYeriMalTanimDalService _pazarYeriMalTanimDalService;
        private readonly IPazaramaClient _pazaramaClient;
        private readonly IBaseService _baseService;
        private readonly ICreateDalService _createDalService;
        private readonly string _logFolderName = nameof(PazarYerleri.Pazarama);
        private readonly IServiceScopeFactory _serviceScopeFactory;
        #endregion

        #region Constructor
        public PazaramaPushPriceStockService(IOptionsSnapshot<AppSettings> options, IPriceStockDalService priceStockDalService, IBaseService baseService, IPazarYeriMalTanimDalService pazarYeriMalTanimDalService, IPazaramaClient pazaramaClient, ICreateDalService createDalService, IServiceScopeFactory serviceScopeFactory)
        {
            _apiDefinition = options.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Pazarama);
            _priceStockDalService = priceStockDalService;
            _baseService = baseService;
            _pazarYeriMalTanimDalService = pazarYeriMalTanimDalService;
            _pazaramaClient = pazaramaClient;
            _createDalService = createDalService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Methods

        public async Task PushPriceStockAsync(Dictionary<string, string> properties, JobType executionType)
        {
            int NumberofThreads = int.Parse(properties[TyGoConstants.Parameters.NumberofThreads]);
            int NumberofProducts = int.Parse(properties[TyGoConstants.Parameters.NumberofProducts]);
            int NumberofTries = int.Parse(properties[TyGoConstants.Parameters.NumberofTries]);
            int NumberofSQLRows = int.Parse(properties[TyGoConstants.Parameters.NumberofSQLRows]);
            string merchantNo = _apiDefinition.Merchantno;
            executionType = JobType.All;

            List<long> refIds = await _priceStockDalService.GetAvailableRefIdsByMerchantNoAsync(executionType, merchantNo);
            Logger.Information("PazaramaService > PushPriceStock > Number of Jobs Available for Execution Type: {ExecutionType}, Job Count: {jobCount}", fileName: _logFolderName,executionType , refIds?.Count() ?? 0);

            foreach (long refId in refIds)
            {
                var merchants = await _priceStockDalService.GetStoreNoListAsync(refId, _apiDefinition.Merchantno);
                List<Task> merchTasks = new List<Task>();
                var dataLst = new ConcurrentBag<CustomResult>();

                Logger.Information($"PazaramaService > PushPriceStock > Execution Type: {executionType}, Number of Merchants: {merchants.Count()}", _logFolderName);



                if (merchants.Count == 0)
                    continue;

                foreach (var storeNoItem in merchants)
                {
                    merchTasks.Add(Task.Run(async () =>
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var priceStockService = scope.ServiceProvider.GetRequiredService<IPriceStockDalService>();
                        var pazaryeriMalTanimService = scope.ServiceProvider.GetRequiredService<IPazarYeriMalTanimDalService>();

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
                        data = await PostProductByMerchant(priceStockService,pazaryeriMalTanimService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("PazaramaService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {@data}", fileName: _logFolderName, executionType, data);

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
                        Logger.Warning("PazaramaService > PushPriceStock > Execution Type: {ExecutionType}, Cannot update Repo for Ref_Id: {refId}", fileName: _logFolderName, executionType, refId);

                }

                await _priceStockDalService.UpdateServiceLog(refId, SrvLogStatus, srvLogError);

                if (totalCount != SuccessCount)
                {
                    Logger.Warning("PazaramaService > PushPriceStock > Execution Type: {ExecutionType}, Task Result Data: {count}", fileName: _logFolderName, executionType, dataLst?.Count ?? 0);
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

            List<CustomResult> dataLst = new();
            Logger.Information("PazaramaService > PushPriceStock is running for ExecutionType: {ExecutionType}", fileName: _logFolderName, executionType);
            try
            {
                var merchants = await _priceStockDalService.GetStoreNoListAsync(refId, _apiDefinition.Merchantno);
                if (!merchants.Any())
                {
                    Logger.Information($"PazaramaService > PushPriceStock > Execution Type: {executionType} | no merchants found for refId:{refId}", fileName: _logFolderName);
                    return dataLst.ToList();
                }
                List<Task> merchTasks = new();
                Logger.Information($"PazaramaService > PushPriceStock > Execution Type: {executionType}, Number of Merchants: {merchants.Count}", _logFolderName);

                foreach (var storeNoItem in merchants)
                {
                    merchTasks.Add(Task.Run(async () =>
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var priceStockService = scope.ServiceProvider.GetRequiredService<IPriceStockDalService>();
                        var pazaryeriMalTanimService = scope.ServiceProvider.GetRequiredService<IPazarYeriMalTanimDalService>();

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
                        data = await PostProductByMerchant(priceStockService,pazaryeriMalTanimService, data).ConfigureAwait(false);
                        dataLst.Add(data);
                        Logger.Information("PazaramaService > PushPriceStock > Execution Type: {ExecutionType}, Individual Task Result Data: {@data}", fileName: _logFolderName, executionType, data);

                    }));
                }
                await Task.WhenAll(merchTasks);

            }
            catch (Exception ex)
            {
                Logger.Error("PazaramaService > PushPriceStock, Execution Type: {ExecutionType}, Exception occurred with Exception: {exception}", fileName: _logFolderName, executionType, ex);
            }
            Logger.Information("PazaramaService > PushPriceStock is completed for ExecutionType: {ExecutionType}", fileName: _logFolderName, executionType);
            return dataLst;
        }

        #endregion

        #region Private Methods

        private async Task<CustomResult> PostProductByMerchant(IPriceStockDalService priceStockDalService,IPazarYeriMalTanimDalService pazarYeriMalTanimDalService, CustomResult data)
        {
            data.StartDateTime = DateTime.Now;
            List<PostPazaramaProductStockAndPriceUpdateRequestDto> requests = new();

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
                        PostPazaramaProductStockAndPriceUpdateRequestDto req = new()
                        {
                            GuidStok = string.Empty,
                            GuidPrice = string.Empty,
                            APIResponse = String.Empty,
                            RequestFailed = false,
                            ResultException = String.Empty,
                            HttpStatusCode = HttpStatusCode.Unused,
                            DetailsCount = 0,
                            Thread_No = i
                        };

                        var tmpdetails = jobdetails.Where(f => f.ThreadNo == i);
                        req.DetailsCount = tmpdetails.Count();
                        req.Items = new PostPazaramaProductStockAndPriceUpdateRequestDto.Root();
                        req.Items.Items = new List<PostPazaramaProductStockAndPriceUpdateRequestDto.Item>();

                        foreach (var det in tmpdetails)
                        {
                            var pyUrunSatisDeger = await pazarYeriMalTanimDalService.GetProductSalesValue(data.MerchantNo, det.PazarYeriMalNo);
                            //Logger.Information(pyUrunSatisDeger.ToString(), _logFolderName);

                            PostPazaramaProductStockAndPriceUpdateRequestDto.Item item = new PostPazaramaProductStockAndPriceUpdateRequestDto.Item
                            {
                                ProductId = det.PazarYeriMalNo,                                
                                MerchantBranchId = det.PazarYeriBirimNo,
                                MerchantSKU = det.PazarYeriBirimNo + "|" + det.MalNo,
                                Quantity = det.StokMiktar.Value,
                                MinBuyingQuantity = 1,
                                MaxBuyingQuantity = det.SepeteEklenebilirMiktar,

                                QuantityIncrease = pyUrunSatisDeger,
                                ListingPrice = det.SatisFiyat,
                                SellingPrice = det.IndirimliSatisFiyat == 0 ? det.SatisFiyat : det.IndirimliSatisFiyat,

                                #region ForReference
                                DetailId = det.DetailId,
                                RefId = det.RefId,
                                #endregion

                                #region Pazarama-V2
                                Code = det.PazarYeriMalNo,
                                ListPrice = det.SatisFiyat,
                                SelPrice = det.IndirimliSatisFiyat == 0 ? det.SatisFiyat : det.IndirimliSatisFiyat,
                                StockCount = det.StokMiktar.Value,
                                #endregion
                            };
                            if (pyUrunSatisDeger == 0)
                            {
                                item.QuantityIncrease = 1;
                            }

                            req.Items.Items.Add(item);
                            //Logger.Information("{@det}", fileName: _logFolderName, det);
                        };

                        var requestStr = JsonConvert.SerializeObject(req.Items);

                        #region Update Stock
                        HttpStatusCode httpStatusCodeResultStok= HttpStatusCode.NotImplemented;
                        string stringContentResultStok = "";
                        bool requestFailedStok = false;
                        try
                        {
                            var resultStok = await _pazaramaClient.ProductStockUpdate(req.Items);

                            var resStock = resultStok.GetContent();

                            requestFailedStok = (resultStok.ResponseMessage.IsSuccessStatusCode && resStock.Success);
                            httpStatusCodeResultStok = resultStok.ResponseMessage.StatusCode;
                            stringContentResultStok = resultStok.StringContent;                            

                            if(resStock == null)
                            {
                                Logger.Error("PazaramaService > PostProductByMerchant Update Stock> Execution Type: {ExecutionType}, Api response is null! > for Ref_Id: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}, Thread_No: {ThreadNo} , Request: {@request} , Response: {@result} , ResponseMessage: {@responseMessage} ", fileName: _logFolderName, (data?.ExecutionType.ToString() ?? ""), data?.RefId.ToString() ?? "", data.MerchantId ?? "", (data?.ThreadId.ToString() ?? ""), (req?.Thread_No.ToString() ?? ""),requestStr, stringContentResultStok, resultStok.ResponseMessage.ToString());
                                data.HasErrors = true;
                                return data;
                            }

                            req.GuidStok = resStock.Data.ToString();

                            List<PazarYeriLog> lst = new List<PazarYeriLog>();
                            foreach (var request in req.Items.Items)
                            {
                                PazarYeriLog logEntry = new()
                                {
                                    RefId = data.RefId,
                                    ExecutionType = data.ExecutionType.ToString(),
                                    PazarYeriBirimNo = data.MerchantId,
                                    ThreadNo = req.Thread_No,
                                    LogType = requestFailedStok ? nameof(LogType.AllCompleted) : nameof(LogType.AllFailed),
                                    Request = requestStr,
                                    Response = $"HTTP StatusCode > Update Stock : {httpStatusCodeResultStok} >  {stringContentResultStok}",
                                    HasErrors = requestFailedStok ? Character.H : Character.E,
                                    DetailId = 0,
                                    PazarYeriNo = data.MerchantNo,
                                    Guid = requestFailedStok ? req.GuidStok ?? "" : string.Empty,
                                };
                                lst.Add(logEntry);
                            }
                        }
                        catch (Exception ex)
                        {
                            requestFailedStok = true;
                            req.ResultException = $"PazaramaService > PostProductByMerchant Update Stock > Execution Type: {data.ExecutionType}, Inner Exception for Ref_Id: {data.RefId}, MerchantId: {data.MerchantId}, ThreadId: {data.ThreadId}, Thread_No: {req.Thread_No} - HTTP: {ex.Message}, with StackTrace: {ex.StackTrace}";

                            Logger.Error("PazaramaService > PostProductByMerchant Update Stock > Execution Type: {ExecutionType}, Inner Exception for Ref_Id: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}, Thread_No: {ThreadNo} - Exception: {exception}", fileName: _logFolderName, (data?.ExecutionType.ToString() ?? ""), data?.RefId.ToString() ?? "", data.MerchantId ?? "", (data?.ThreadId.ToString() ?? ""), (req?.Thread_No.ToString() ?? ""), ex);

                            await InsertUpdateLogByRefIdMerchThread(req.Items.Items, nameof(LogType.AllFailed), data.RefId, data.ExecutionType.ToString(), data.MerchantId, req.Thread_No, requestStr, ex.Message, Character.E, data.MerchantNo, req.GuidStok);
                        }
                        #endregion
                       
                        #region Update Price
                        HttpStatusCode httpStatusCodeResultPrice=HttpStatusCode.NotImplemented;
                        string stringContentResultPrice = "";
                        bool requestFailedPrice=false;
                        try
                        {
                            var resultPrice = await _pazaramaClient.ProductPriceUpdate(req.Items);

                            var resPrice = resultPrice.GetContent();

                            requestFailedPrice = (resultPrice.ResponseMessage.IsSuccessStatusCode && resPrice.Success);
                            httpStatusCodeResultPrice = resultPrice.ResponseMessage.StatusCode;
                            stringContentResultPrice = resultPrice.StringContent;


                            if (resPrice == null)
                            {
                                Logger.Error("PazaramaService > PostProductByMerchant Update Price > Execution Type: {ExecutionType}, Api response is null! > for Ref_Id: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}, Thread_No: {ThreadNo} , Request: {@request} , Response: {@result} , ResponseMessage: {@responseMessage} ", fileName: _logFolderName, (data?.ExecutionType.ToString() ?? ""), data?.RefId.ToString() ?? "", data.MerchantId ?? "", (data?.ThreadId.ToString() ?? ""), (req?.Thread_No.ToString() ?? ""), requestStr, stringContentResultPrice, resultPrice.ResponseMessage.ToString());
                                data.HasErrors = true;
                                return data;
                            }

                            req.GuidPrice = resPrice.Data.ToString();

                            List<PazarYeriLog> lst = new List<PazarYeriLog>();
                            foreach (var request in req.Items.Items)
                            {
                                PazarYeriLog logEntry = new()
                                {
                                    RefId = data.RefId,
                                    ExecutionType = data.ExecutionType.ToString(),
                                    PazarYeriBirimNo = data.MerchantId,
                                    ThreadNo = req.Thread_No,
                                    LogType = requestFailedPrice ? nameof(LogType.AllCompleted) : nameof(LogType.AllFailed),
                                    Request = requestStr,
                                    Response = $"HTTP StatusCode > Update Price : {resultPrice.ResponseMessage.StatusCode} >  {resultPrice.StringContent}",
                                    HasErrors = requestFailedPrice ? Character.H : Character.E,
                                    DetailId = 0,
                                    PazarYeriNo = data.MerchantNo,
                                    Guid = requestFailedPrice ? req.GuidPrice ?? "" : string.Empty,
                                };
                                lst.Add(logEntry);
                            }
                        }
                        catch (Exception ex)
                        {
                            requestFailedPrice = true;
                            req.ResultException = $"PazaramaService > PostProductByMerchant Update Price > Execution Type: {data.ExecutionType}, Inner Exception for Ref_Id: {data.RefId}, MerchantId: {data.MerchantId}, ThreadId: {data.ThreadId}, Thread_No: {req.Thread_No} - HTTP: {ex.Message}, with StackTrace: {ex.StackTrace}";

                            Logger.Error("PazaramaService > PostProductByMerchant Update Price > Execution Type: {ExecutionType}, Inner Exception for Ref_Id: {RefId}, MerchantId: {MerchantId}, ThreadId: {ThreadId}, Thread_No: {ThreadNo} - Exception: {exception}", fileName: _logFolderName, (data?.ExecutionType.ToString() ?? ""), data?.RefId.ToString() ?? "", data.MerchantId ?? "", (data?.ThreadId.ToString() ?? ""), (req?.Thread_No.ToString() ?? ""), ex);

                            await InsertUpdateLogByRefIdMerchThread(req.Items.Items, nameof(LogType.AllFailed), data.RefId, data.ExecutionType.ToString(), data.MerchantId, req.Thread_No, requestStr, ex.Message, Character.E, data.MerchantNo, req.GuidPrice);
                        }
                        #endregion

                        req.RequestFailed = !(requestFailedPrice && requestFailedStok);
                        req.HttpStatusCode = httpStatusCodeResultStok == httpStatusCodeResultPrice ? httpStatusCodeResultStok : httpStatusCodeResultPrice;
                        req.APIResponse = $"Stock Response: {stringContentResultStok} - Price Response: {stringContentResultPrice}";
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

                        foreach (var individualItem in requestItem.Items.Items)
                        {
                            long refId = individualItem.RefId;
                            long detailId = individualItem.DetailId;
                            var detail = jobdetails.FirstOrDefault(f => f.RefId == refId && f.DetailId == detailId);

                            if (detail == null) continue;

                            detail.Guid = requestItem.GuidPrice;
                            detail.ThreadNo = requestItem.Thread_No;
                            detail.HasErrors = requestItem.RequestFailed ? Character.E : Character.H;
                            detail.HasSent = Character.E;
                            updatedDetailList.Add(detail);
                        }

                        try
                        {
                            Logger.Information("PazaramaService > PostProductByMerchant > Execution Type: {ExecutionType}, starting to update for RefId: {RefId}, MerchantId: {MerchantId}, ItemCount:{Count}", fileName: _logFolderName, data?.ExecutionType, data?.RefId, data?.MerchantId, updatedDetailList.Count);

                            if (updatedDetailList.Any())
                                await priceStockDalService.UpdatePazarYeriJobResultDetailRangeAsync(updatedDetailList);

                            Logger.Information("PazaramaService > PostProductByMerchant > Execution Type: {ExecutionType}, Requests updates successfully in Repo Db/for RefId: {RefId}, MerchantId: {MerchantId}, ItemCount:{Count}", fileName: _logFolderName, data?.ExecutionType, data?.RefId, data?.MerchantId, updatedDetailList.Count);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("PazaramaService > PostProductByMerchant > Execution Type: {ExecutionType}, Cannot update requests successfully in Db/Repo for RefId: {RefId}, MerchantId: {MerchantId}, ItemCount:{Count},Exception : {ex}", fileName: _logFolderName, data?.ExecutionType, data?.RefId, data?.MerchantId, updatedDetailList.Count, ex);
                        }


                    }

                    stopwatch.Stop();
                    Logger.Information("PazaramaService UpdatePazarYeriJobResultDetailRangeAsync finished in {elapsedTime}ms.", _logFolderName, stopwatch.ElapsedMilliseconds);

                    #endregion

                    data.CompletionDateTime = DateTime.Now;

                    if (requests.Count(f => !f.RequestFailed) != requests.Count)
                    {
                        data.HasErrors = true;
                        Logger.Warning("PazaramaService > PostProductByMerchant > Execution Type: {ExecutionType}, Completed partially for MerchantId: {MerchantId} for Execution Data: {@data}", fileName: _logFolderName, data?.ExecutionType, data?.MerchantId, data);
                    }
                    else
                    {
                        data.HasErrors = false;
                        Logger.Information("PazaramaService > PostProductByMerchant > Execution Type: {ExecutionType}, Completed successfully for MerchantId: {MerchantId} for Execution Data: {@data}", fileName: _logFolderName, data?.ExecutionType, data?.MerchantId, data);
                    }
                }
                else
                {
                    Logger.Warning($"PazaramaService > PostProductByMerchant > Execution Type: {data.ExecutionType}, No Details found for RefId: {data.RefId}, MerchantId: {data.MerchantId}", fileName: _logFolderName);
                }
            }
            catch (Exception ex)
            {
                data.CompletionDateTime = DateTime.Now;
                data.HasErrors = true;
                data.ExceptionsString = ex.Message;

                Logger.Error("PazaramaService > PostProductByMerchant > Execution Type: {ExecutionType}, Failed with Exception for MerchantId: {MerchantId} for Execution Data: {@data}, Exception: {exception}", fileName: _logFolderName, data?.ExecutionType, data?.MerchantId, data, ex);
            }           
            return data;
        }

        private async Task InsertUpdateLogByRefIdMerchThread(List<PostPazaramaProductStockAndPriceUpdateRequestDto.Item> requests, string LogType, long RefId, string ExecutionType, string MerchantId, int ThreadNo, string Request, string Response, string HasErrors, string MerchantNo, string Guid)
        {
            List<PazarYeriLog> lst = new();
            foreach (var req in requests)
            {
                var obj = new PazarYeriLog()
                {
                    RefId = RefId,
                    ExecutionType = ExecutionType,
                    ThreadNo = ThreadNo,
                    LogType = LogType,
                    Request = Request,
                    Response = Response,
                    HasErrors = HasErrors,
                    DetailId = req.DetailId,
                    Guid = Guid,
                    PazarYeriNo = MerchantNo,
                    PazarYeriBirimNo = MerchantId
                };
                lst.Add(obj);
                //TODO
                //dbParams.Add("MerchantId", MerchantId);
                //dbParams.Add("MerchantNo", MerchantNo);
            }
            await _createDalService.AddRangeAsync(lst);
        }

        #endregion

    }
}
