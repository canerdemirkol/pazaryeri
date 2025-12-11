using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using RestEase;
using System.Drawing.Printing;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Client.Concrete
{
    public class YemekSepetiClient : IYemekSepetiClient
    {
        private readonly IYemekSepetiClient _yemekSepetiClient;
        private IYemekSepetiClient _yemekSepetiClientWithoutBaseUrl;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string _token;
        public YemekSepetiClient(IOptions<AppSettings> appSettings)
        {
            _apiDefinition = appSettings.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.Yemeksepeti);
        }

        public async Task<Response<YemekSepetiPriceStockResponseDto>> PushPriceStockAsync([Path] string vendorId, [Body] YemekSepetiPriceStockRequestDto requestDto)
        {
            return await _yemekSepetiClient.PushPriceStockAsync(vendorId, requestDto);
        }

        public async Task<Response<string>> VerifyPriceStockAsync(string url)
        {
            var _yemekSepetiClientWithoutBaseUrl = new RestClient(url, (request, cancellationToken) =>
            {
                request.Headers.Add("Authorization", $"Bearer {_token}");
                return Task.CompletedTask;
            })
            {
                JsonSerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            }
        .For<IYemekSepetiClient>();
            return await _yemekSepetiClientWithoutBaseUrl.VerifyPriceStockAsync();
        }

        public async Task<Response<string>> VerifyPriceStockAsync()
        {
            return await _yemekSepetiClientWithoutBaseUrl.VerifyPriceStockAsync();
        }

        public async Task<Response<YemekSepetiUpdateOrderResponseDto>> UpdateOrderAsync([Path] string order_id, [Body] YemekSepetiUpdateOrderRequestDto requestDto)
        {
            return await _yemekSepetiClient.UpdateOrderAsync(order_id,requestDto);
        }
        public async Task<Response<YemekSepetiProductDetailResponseDto>> GetProductDetailsAsync([Path] string vendorId, int page_size, int page)
        {
            return await _yemekSepetiClient.GetProductDetailsAsync(vendorId,page_size, page);
        }

        public async Task<Response<YemekSepetiPromotionResponseDto>> UpdateChainPromotionAsync([Path] string chain_id, [Body] YemekSepetiUpdatePromotionRequestDto requestDto)
        {
            return await _yemekSepetiClient.UpdateChainPromotionAsync(chain_id, requestDto);
        }
    }
}