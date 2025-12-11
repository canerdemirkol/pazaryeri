using System.Data;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.DataAccess.Services.Abstract.PriceStock;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Services.Concrete.PushPrice
{
    public class SharedPriceStockOnlyStockService : ISharedPriceStockOnlyStockService
    {
        #region Variables

        private readonly IPriceStockDalService _priceStockDalService;
        private readonly IHepsiExpressPushPriceStockService _hepsiExpressPushPriceStockService;
        private readonly ITrendyolGoPushPriceStockService _trendyolGoPushPriceStockService;
        private readonly ITrendyolPushPriceStockService _trendyolPushPriceStockService;
        private readonly IGetirCarsiPushPriceStockService _getirCarsiPushPriceStockService;
        private readonly IYemekSepetiPushPriceStockService _yemekSepetiPushPriceStockService;
        private readonly IPazaramaPushPriceStockService _pazaramaPushPriceStockService;
        private readonly IIdefixPushPriceStockService _idefixPushPriceStockService;
        private readonly string _logFolderName = CommonConstants.SharedPriceStockOnlyStockJob;

        #endregion

        #region Ctor

        public SharedPriceStockOnlyStockService(IYemekSepetiPushPriceStockService yemekSepetiPushPriceStockService, 
            IPriceStockDalService priceStockDalService,
            ITrendyolGoPushPriceStockService trendyolGoPushPriceStockService,
            IGetirCarsiPushPriceStockService getirCarsiPushPriceStockService,
            IPazaramaPushPriceStockService pazaramaPushPriceStockService,
            IHepsiExpressPushPriceStockService hepsiExpressPushPriceStockService,
            ITrendyolPushPriceStockService trendyolPushPriceStockService,
            IIdefixPushPriceStockService idefixPushPriceStockService)
        {
            _yemekSepetiPushPriceStockService = yemekSepetiPushPriceStockService;
            _priceStockDalService = priceStockDalService;
            _trendyolGoPushPriceStockService = trendyolGoPushPriceStockService;
            _getirCarsiPushPriceStockService = getirCarsiPushPriceStockService;
            _pazaramaPushPriceStockService = pazaramaPushPriceStockService;
            _hepsiExpressPushPriceStockService = hepsiExpressPushPriceStockService;
            _trendyolPushPriceStockService = trendyolPushPriceStockService;
            _idefixPushPriceStockService = idefixPushPriceStockService;
        }

        #endregion

        #region Methods

        public async Task SendPushPriceStockOnlyStocks(Dictionary<string, string> properties)
        {
            JobType executionType = JobType.OnlyStock;
            List<PazarYerleri> pazarYerleri = properties["PazarYerleri"].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => (PazarYerleri)Enum.Parse(typeof(PazarYerleri), x)).ToList();
            properties.TryGetValue("WorkWithOld", out var value);
            bool.TryParse(value ?? "false", out bool workWithOld);
            List<string> merchantNos = null;
            if (workWithOld)
            {
                properties.TryGetValue("MerchantNos", out var merchantNo);
                merchantNos = merchantNo.Split(',').ToList();
            }
            List<long> refIds = await _priceStockDalService.GetAvailableRefIdsAsync(executionType, workWithOld, merchantNos);
            int NumberofProducts = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofProducts]);
            Logger.Information("SharedService > PushPriceStock > Number of Jobs Available for Execution Type {ExecutionType}: {Count}", fileName: _logFolderName, executionType, (refIds?.Count ?? 0));

            foreach (var refId in refIds)
            {

                List<CustomResult> dataLst = new();
                List<Task<List<CustomResult>>> merchTasks = new();

                foreach (var pazarYeri in pazarYerleri)
                {
                    switch (pazarYeri)
                    {
                        case PazarYerleri.HepsiExpress:
                            merchTasks.Add(_hepsiExpressPushPriceStockService.PushPriceStockOnlyStockAsync(properties, refId));
                            break;
                        case PazarYerleri.TrendyolGo:
                            merchTasks.Add(_trendyolGoPushPriceStockService.PushPriceStockOnlyStockAsync(properties, refId));
                            break;
                        case PazarYerleri.Trendyol:
                            merchTasks.Add(_trendyolPushPriceStockService.PushPriceStockOnlyStockAsync(properties, refId));
                            break;
                        case PazarYerleri.GetirCarsi:
                            merchTasks.Add(_getirCarsiPushPriceStockService.PushPriceStockOnlyStockAsync(properties, refId));
                            break;
                        case PazarYerleri.YemekSepeti:
                            merchTasks.Add(_yemekSepetiPushPriceStockService.PushPriceStockOnlyStockAsync(properties, refId));
                            break;
                        case PazarYerleri.Pazarama:
                            merchTasks.Add(_pazaramaPushPriceStockService.PushPriceStockOnlyStockAsync(properties, refId));
                            break;
                        case PazarYerleri.Idefix:
                            merchTasks.Add(_idefixPushPriceStockService.PushPriceStockOnlyStockAsync(properties, refId));
                            break;
                        default:
                            break;
                    }
                }
                var data = await Task.WhenAll(merchTasks);
                dataLst = data.SelectMany(x => x).ToList();
                var totalCount = dataLst?.Count ?? 0;
                var FailedCount = dataLst?.Count(f => f.HasErrors) ?? 0;
                var SuccessCount = dataLst?.Count(f => !f.HasErrors) ?? 0;
                var HasErrors = (totalCount != SuccessCount) ? Character.E : Character.H;
                var HasErrorStr = HasErrors == Character.E ? "partially" : "successfully";
                var srvLogErrorPhrase = (totalCount != SuccessCount) ? ", LOG TABLOLARINI KONTROL EDINIZ" : "";
                var srvLogError = $"TOPLAM ISTEK SAY.: {totalCount}, BASARILI ISTEK SAY.: {SuccessCount}, BASARISIZ ISTEK SAY.: {FailedCount}{srvLogErrorPhrase}";
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
                        Logger.Warning("SharedService > PushPriceStock > Execution Type: {ExecutionType}, Cannot update Repo for Ref_Id: {refId}", fileName: _logFolderName, executionType, refId);

                }

                await _priceStockDalService.UpdateServiceLog(refId, SrvLogStatus, srvLogError);

                if (totalCount != SuccessCount)
                    Logger.Warning("SharedService > PushPriceStock > Execution Type: {ExecutionType}, Task Result Data: {count}", fileName: _logFolderName, executionType, dataLst?.Count ?? 0);

            }
        }

        #endregion
    }
}