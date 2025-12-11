using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using RestEase;

namespace OBase.Pazaryeri.Business.Client.Abstract
{
    public interface IHepsiExpressClient : IDisposable
    {
        #region General

        #endregion

        #region Price / Stock
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/listings/merchantid/{merchantid}/inventory-uploads/id/{requestid}")]
        Task<Response<HEGetRequestResponseDto>> GetRequestsControlById([Path] string merchantid, [Path] string requestid);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/listings/merchantid/{merchantid}?offset=0&limit=100")]
        Task<Response<HEListingDetailsDto>> GetListings([Path] string merchantid);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [Header("Content-Type", "application/xml")]
        [AllowAnyStatusCode]
        [Post("/listings/merchantid/{merchantid}/inventory-uploads")]
        Task<Response<HEUpdateProductsRespDto>> UpdateListingProductStock([Path] string merchantid, [Body] Listings dto);
        #endregion

        #region Discount
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/listing-discounts/merchantid/{merchantid}")]
        Task<Response<HEListingDiscountResponseDto>> InsertDiscount([Path] string merchantid, [Body] HEListingDiscountRequestDto dto);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Put("/listing-discounts/id/{discountid}")]
        Task<Response<HEListingDiscountResponseDto>> UpdateDiscount([Path] string discountid, [Body] HEListingDiscountRequestDto dto);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/listing-discounts/id/{discountid}")]
        Task<Response<HEGetListngDiscountResponseDto.DiscountsResult>> GetDiscount([Path] string discountid);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/listing-discounts/merchantId/{merchantid}/status/{status}")]
        Task<Response<HEGetListngDiscountResponseDto.DiscountsResult>> GetByMerchantIdAndStatusDiscount([Path] string merchantid, [Path] string status);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Delete("/listing-discounts/id/{id}")]
        Task<Response<HEListingDiscountResponseDto>> DeleteDiscount([Path] string id);
        #endregion
    }
}
