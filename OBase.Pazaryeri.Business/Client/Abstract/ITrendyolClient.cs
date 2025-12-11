using OBase.Pazaryeri.Domain.Dtos.Trendyol;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using RestEase;

namespace OBase.Pazaryeri.Business.Client.Abstract
{
    public interface ITrendyolClient
    {
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("sapigw/suppliers/{merchantId}/products/batch-requests/{guid}")]
        Task<Response<TrendyolVerifyPriceStockResponseDto>> GetBatchRequestResultAsync([Path] string merchantId, [Path] string guid);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/sapigw/suppliers/{supplierid}/products/price-and-inventory")]
        Task<Response<TrendyolPushPriceStockResponseDto>> SendPriceStockAsync([Path] string supplierid, [Body] TrendyolPushPriceStockRequestDto requestItem);

        /// <summary>
        /// The method that connects customer and store via Trendyol.
        /// </summary>
        /// <param name="orderNumber"></param>
        /// <param name="pickerPhone"></param>
        /// <param name="sellerId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/sxivrgw/bridge/v2/instant-call?orderNumber={orderNumber}&pickerPhone={pickerPhone}&sellerId={sellerId}&storeId={storeId}")]
        Task<Response<TrendyolCallCustomerResponseDto>> GetInstantCall([Path] string orderNumber, [Path] string pickerPhone, [Path] string sellerId, [Path] string storeId);
    }
}