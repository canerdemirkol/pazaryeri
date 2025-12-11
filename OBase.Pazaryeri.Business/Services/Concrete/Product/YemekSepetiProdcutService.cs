using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Product;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using System.Text;
using System.Text.Json;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Services.Concrete.Product
{
    public class YemekSepetiProdcutService : IYemekSepetiProdcutService
    {
        #region Private
        private readonly IPazarYeriMalTanimDalService _malTanimDalService;
        private readonly IYemekSepetiClient _yemekSepetiClient;
        private readonly IOptions<AppSettings> _appSetting;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string ysLogfile = nameof(PazarYerleri.YemekSepeti);
        #endregion

        #region Const
        public YemekSepetiProdcutService(IPazarYeriMalTanimDalService malTanimDalService, IYemekSepetiClient yemekSepetiClient, IOptions<AppSettings> options)
        {
            _malTanimDalService = malTanimDalService;
            _yemekSepetiClient = yemekSepetiClient;
            _appSetting = options;
            _apiDefinition = options.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Yemeksepeti);
        }
        #endregion

        #region Metod

        public async Task UpdatePYProductImageAsync()
        {
            string vendorId = _apiDefinition.MerchantId;

            int index = 1;
            int totalPageSize = 1;
            int pageSize = 500;

            string urlSeperator = _appSetting.Value.ImageSize.UrlSeperator;
            string imageWidth = $"/{_appSetting.Value.ImageSize.Width}";
            string imageLength = $"/{_appSetting.Value.ImageSize.Length}/";
            string resizePathParameter = _appSetting.Value.ImageSize.ResizePathParameter;

            while (index <= totalPageSize)
            {
                Logger.Information("UpdatePYProductImageAsync Yemek Sepeti Request : VendorId: {vendorId}, PageSize: {vepageSizendorId} ,Page: {page} ", fileName: ysLogfile, vendorId, pageSize, index);
                var productResponse = await _yemekSepetiClient.GetProductDetailsAsync(vendorId, pageSize, index);
                Logger.Information("UpdatePYProductImageAsync  Yemek Sepeti Response :{@response} ", fileName: ysLogfile, productResponse.StringContent);
                if (productResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    YemekSepetiProductDetailResponseDto productDetail = JsonSerializer.Deserialize<YemekSepetiProductDetailResponseDto>(productResponse.StringContent);
                    totalPageSize = productDetail.TotalPage;
                    index++;

                    List<string> productNos = productDetail.Products.Select(s => s.Sku).ToList();
                    var productDetailsDb = await _malTanimDalService.GetPyProductNosAsync(productNos, PazarYeri.Yemeksepeti);
                    if (productDetailsDb.Any())
                    {
                        foreach (var item in productDetail.Products)
                        {
                            var product = productDetailsDb.FirstOrDefault(w => w.PazarYeriMalNo == item.Sku);
                            if (product == null)
                            { continue; }
                            StringBuilder imageUrl = new();
                            foreach (string url in item.Images)
                            {
                                var urlArray = url.Split(urlSeperator);
                                if (urlArray.Length == 2)
                                {
                                    string baseUrl = urlArray[0] + urlSeperator;
                                    imageUrl.Append($"{baseUrl + resizePathParameter + imageWidth + imageLength + urlArray[1]},");
                                }
                                else
                                {
                                    imageUrl.Append($"{url},");
                                }
                            }
                            product.ImageUrl = imageUrl.ToString();
                            var updateResult = await _malTanimDalService.UpdateProductAsync(product);
                            if (!updateResult)
                            {
                                Logger.Error("UpdatePYProductImageAsync {PazarYeriMalNo}  nolu ürünün image urli güncellenemedi.", product.PazarYeriMalNo);
                            }
                        }
                    }

                }
                else
                {
                    break;
                }
            }

        }

        #endregion
    }
}