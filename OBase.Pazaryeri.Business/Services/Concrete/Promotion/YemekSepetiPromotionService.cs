using Hangfire.Common;
using Hangfire.Storage.SQLite.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Client.Concrete;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Promotion;
using OBase.Pazaryeri.DataAccess.Services.Abstract.PriceStock;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Promotion;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using OBase.Pazaryeri.Domain.Entities;
using RestEase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using OBase.Pazaryeri.Business.Helper;

namespace OBase.Pazaryeri.Business.Services.Concrete.Promotion
{
    public class YemekSepetiPromotionService : IYemekSepetiPromotionService
    {
        private readonly IPromotionDalService _promotionDalService;
        private readonly IYemekSepetiClient _yemekSepetiClient;
        private readonly ApiDefinitions _apiDefinition;       
        private readonly string _logFolderName = nameof(PazarYerleriPromosyon.YemekSepetiPromosyon);
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="promotionDalService"></param>
        /// <param name="yemekSepetiClient"></param>
        /// <param name="options"></param>
        public YemekSepetiPromotionService(IPromotionDalService promotionDalService, IYemekSepetiClient yemekSepetiClient, IOptions<AppSettings> options, IServiceScopeFactory serviceScopeFactory)
        {
            _promotionDalService = promotionDalService;
            _yemekSepetiClient = yemekSepetiClient;
            _apiDefinition = options.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Yemeksepeti);
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task PromotionAsync(Dictionary<string, string> properties)
        {
            try
            {
                int NumberofThreads = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofThreads]);
                int NumberofProducts = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofProducts]);
                int NumberofTries = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofTries]);
                int NumberofSQLRows = int.Parse(properties[SharedPriceStockOnlyConstants.Parameters.NumberofSQLRows]);

                var promotions = await _promotionDalService.GetPendingPromotionsAsync();
                Logger.Information("YSService > PromotionAsync > Number of Guids Available for Promotion: {count}", fileName: _logFolderName, promotions?.Count ?? 0);             

                List<Task> merchTasks = new();
                var dataLst = new ConcurrentBag<CustomVerifyResult>();

                foreach (var promo in promotions)
                {

                    merchTasks.Add(Task.Run(async () =>
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var promotinDalService = scope.ServiceProvider.GetRequiredService<IPromotionDalService>();

                        var data = new CustomVerifyResult() { Guid = promo.Id.ToString(), CreationDateTime = DateTime.Now };
                   
                        data.MerchantId = promo.PromosyonNo.ToString();
                        data.DetailsCount = 0;
                        data.FailedCount = 0;
                        data.ProcessingCount = 0;
                        data.SuccessfulCount = 0;
                        data = await YS_PromotionAsync(promotinDalService, data);
                        dataLst.Add(data);
                        Logger.Information("YSService > PromotionAsync > Individual Task Result Promotion : {Guid}", fileName: _logFolderName, data.Guid);
                    }));

                    await Task.WhenAll(merchTasks);

                    var totalCount = dataLst?.Count() ?? 0;
                    var SuccessCount = dataLst?.Count(f => !f.HasErrors) ?? 0;

                    if (totalCount != SuccessCount)
                    {
                        Logger.Warning("YSService > PromotionAsync > Task Result Data: {Count}", fileName: _logFolderName, dataLst?.Count ?? 0);
                    }                   
                }
            }
            catch (Exception ex)
            {
                Logger.Error("YSService > PromotionAsync >  Exception: {exception}", fileName: _logFolderName, ex);
            }
        }

        private async Task<CustomVerifyResult> YS_PromotionAsync(IPromotionDalService promotinDalService, CustomVerifyResult data)
        {
            data!.StartDateTime = DateTime.Now;
            string guid = data.Guid;
            string merchantId = _apiDefinition.SupplierId;
            string chainId = _apiDefinition?.ChainId;
            string merchantNo = _apiDefinition?.Merchantno;

            var promosyonNo = Convert.ToInt64(data.MerchantId);
            var promosyonId = Convert.ToInt64(data.Guid);

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                options.Converters.Add(new JsonStringEnumConverter());

              
                var success = await _promotionDalService.ExecutePromotionProcedureAsync(promosyonNo, promosyonId);
                if (!success) throw new Exception("ExecutePromotionProcedureAsync > Error : Proc_PY_PromotionEntegrasyon ");


                var (tanim, detaylar, birimler) = await _promotionDalService.GetTmpPromotionDataAsync(promosyonId, promosyonNo, merchantNo);

                if (tanim is null) throw new Exception($"ExecutePromotionProcedureAsync > Error : Proc_PY_PromotionEntegrasyon > TmpPyPromosyonTanim tablosu null > promotionNo: {promosyonNo} - promosyonId: {promosyonId}");

                Logger.Information("YSService PromotionService > TmpPyPromosyonTanim >table {@tanim}", _logFolderName, JsonSerializer.Serialize(tanim, options));
                // Eğer tum birimler = 'E' ise Vendors=[*]
                var vendors = tanim?.TumBirimlerEh == "E" ? new string[] { "*" } : birimler?.Select(b => b.BirimNo).ToArray();
              
                var displayName = new Dictionary<string, string>
                {
                    { "en_TR", string.IsNullOrEmpty(tanim.DisplayName) ? "-" : tanim.DisplayName }
                };

                var promotionReq = new YemekSepetiUpdatePromotionRequestDto
                {
                    Active = !string.IsNullOrEmpty(tanim.Active) && tanim.Active.ToUpper() == "TRUE",
                    Vendors = vendors.ToList(),
                    Type = tanim.Type ?? "SAME_ITEM_BUNDLE",
                    DisplayName = displayName,
                    Conditions = new YemekSepetiPromotionCondition { StartTime = tanim.StartTime.ToUtcDateTime(), EndTime = tanim.EndTime.ToUtcDateTime() },
                    Reason = tanim.Reason ?? "COMPETITIVENESS"
                };

                var discount = new List<YemekSepetiPromotionDiscount>();

                Logger.Information("YSService PromotionService > TmpPyPromosyonDetay >table {@detaylar}", _logFolderName, JsonSerializer.Serialize(detaylar, options));

                foreach (var item in detaylar)
                {
                    var discountItem = new YemekSepetiPromotionDiscount
                    {
                      
                        DiscountValue = item.DiscountValue ?? 0,                      
                        Sku = new List<string>() { item.Sku ?? "" },
                        DiscountSubtype = item.DiscountSubtype.ToUpper()
                    };

                    discount.Add(discountItem);
                }
                promotionReq.Discount=discount;
                Logger.Information("YSService PromotionService > UpdateChainPromotionAsync() > chain_id {@chainId} - logEntry {@logEntry}", _logFolderName, chainId ?? "", promotionReq);

                var requestStr = JsonSerializer.Serialize(promotionReq);

                var response = await _yemekSepetiClient.UpdateChainPromotionAsync(chainId, promotionReq);
                data.RequestCompletionDateTime = DateTime.Now;
                Logger.Information("YSService PromotionService > UpdateChainPromotionAsync() Result {result}", _logFolderName, response.StringContent);
                if (response.ResponseMessage.IsSuccessStatusCode)
                {                   
                    var productDetail = JsonSerializer.Deserialize<YemekSepetiPromotionResponseDto>(response.StringContent, options);
                    await _promotionDalService.MarkPromotionAsSentAsync(promosyonNo, productDetail.JobId);
                    data.HasErrors = false;
                }
                else
                {
                    await _promotionDalService.UpdatePromotionErrorAsync(promosyonNo, response.StringContent);
                    data.HasErrors = true;
                    data.ExceptionString = response.StringContent;
                }
                data.CompletionDateTime = DateTime.Now;
               
            }
            catch (Exception ex )
            {
                data.HasErrors = true;
                data.CompletionDateTime = DateTime.Now;
                data.ExceptionString = $"YSService > YS_PromotionAsync > Exception: {ex.Message}";
                Logger.Error("YSService > YS_PromotionAsync > Execution Type: {ExecutionType}, Exception: {exception}", fileName: _logFolderName, ex);
                await _promotionDalService.UpdatePromotionErrorAsync(promosyonNo, $"Error:{ex.Message} - StackTrace:{ex.StackTrace}");
            }
            return data;
        }
    }
}
