using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.Trendyol;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using RestEase;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Client.Concrete
{
    public class TrendyolClient : ITrendyolClient
    {
        private readonly ITrendyolClient _client;
        public TrendyolClient(IOptions<AppSettings> appSettings)
        {
            var apiDefinition = appSettings.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.Trendyol);
        }

        public async Task<Response<TrendyolVerifyPriceStockResponseDto>> GetBatchRequestResultAsync(string merchantId, string guid)
        {
            return await _client.GetBatchRequestResultAsync(merchantId, guid);
        }

		public async Task<Response<TrendyolPushPriceStockResponseDto>> SendPriceStockAsync([Path] string supplierid, [Body] TrendyolPushPriceStockRequestDto requestItem)
        {
            return await _client.SendPriceStockAsync(supplierid, requestItem);
        }
        public async Task<Response<TrendyolCallCustomerResponseDto>> GetInstantCall([Path] string orderNumber, [Path] string pickerPhone, [Path] string sellerId, [Path] string storeId)
        {
            return await _client.GetInstantCall(orderNumber, pickerPhone, sellerId, storeId);
        }
    }
}
