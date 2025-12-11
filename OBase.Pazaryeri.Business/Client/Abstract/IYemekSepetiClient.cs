using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using RestEase;

namespace OBase.Pazaryeri.Business.Client.Abstract
{
    public interface IYemekSepetiClient
    {
        #region Price / Stock
        [Header("Cache-Control", "no-cache")]
        [Header("accept", "application/json")]
        [AllowAnyStatusCode]
        [Put("/api/assortment/v1/vendors/{vendorId}/products-bulk")]
        Task<Response<YemekSepetiPriceStockResponseDto>> PushPriceStockAsync([Path] string vendorId, [Body] YemekSepetiPriceStockRequestDto requestDto);

        [Get("{url}")]
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [Header("Content-Type", "text/csv")]
        [AllowAnyStatusCode]
        Task<Response<string>> VerifyPriceStockAsync([Path] string url);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [Header("Content-Type", "text/csv")]
        [AllowAnyStatusCode]
        [Get("")]
        Task<Response<string>> VerifyPriceStockAsync();
        #endregion

        #region Orders
        [Header("Cache-Control", "no-cache")]
        [Header("accept", "application/json")]
        [AllowAnyStatusCode]
        [Put("/v1/orders/{order_id}")]
        Task<Response<YemekSepetiUpdateOrderResponseDto>> UpdateOrderAsync([Path] string order_id, [Body] YemekSepetiUpdateOrderRequestDto requestDto);
        #endregion

        #region Product


        [Get("/api/assortment/v1/vendors/{vendorId}/paged-products")]
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [Header("accept", "application/json")]
        [AllowAnyStatusCode]
        Task<Response<YemekSepetiProductDetailResponseDto>> GetProductDetailsAsync([Path]string vendorId, int page_size,int page);

        #endregion

        #region Promotions - Foodpanda V2
        [Header("Cache-Control", "no-cache")]
        [Header("accept", "application/json")]        
        [AllowAnyStatusCode]
        [Put("/v2/chains/{chain_id}/promotion")]
        Task<Response<YemekSepetiPromotionResponseDto>> UpdateChainPromotionAsync(
            [Path] string chain_id,
            [Body] YemekSepetiUpdatePromotionRequestDto requestDto
        );
        #endregion
    }
}
