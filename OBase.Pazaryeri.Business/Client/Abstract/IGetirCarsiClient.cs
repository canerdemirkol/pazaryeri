using OBase.Pazaryeri.Domain.Dtos.Getir;
using OBase.Pazaryeri.Domain.Dtos.Getir.Login;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using OBase.Pazaryeri.Domain.Dtos.Getir.PriceStock;
using OBase.Pazaryeri.Domain.Dtos.Getir.Product;
using OBase.Pazaryeri.Domain.Dtos.Getir.Return;
using RestEase;

namespace OBase.Pazaryeri.Business.Client.Abstract
{
    public interface IGetirCarsiClient : IDisposable
    {
        #region General
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/v1/auth/token")]
        Task<Response<AuthTokenResponseDto>> GetToken([Header("Authorization")] string authorization, [Header("Accept")] string accept);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Put("/v1/suppliers/password/reset")]
        Task<Response<GetirResponse>> ResetPassword([Body] ResetPasswordDto resetPasswordDto);
        #endregion

        #region Orders
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/v1/orders/{orderId}/shop/{shopId}/verify")]
        Task<Response<GetirResponse>> Verify([Path] string orderId, [Path] string shopId);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/v1/orders/{orderId}/shop/{shopId}/prepare")]
        Task<Response<GetirResponse>> Prepare([Path] string orderId, [Path] string shopId, [Body] PrepareDto preapareDto);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/v1/orders/{orderId}/shop/{shopId}/handover")]
        Task<Response<GetirResponse>> Handover([Path] string orderId, [Path] string shopId);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/v1/orders/{orderId}/shop/{shopId}/deliver")]
        Task<Response<GetirResponse>> Deliver([Path] string orderId, [Path] string shopId);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/v1/orders/{orderId}/shop/{shopId}/cancel")]
        Task<Response<GetirResponse>> Cancel([Path] string orderId, [Path] string shopId, [Body] CancelDto cancelDto);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/v1/orders/{orderId}/cancel-options")]
        Task<Response<GenericGetirResponse<CancelOptionResponseDto>>> CancelOptions([Path] string orderId);

        #endregion

        #region Price / Stock

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Put("/v1/products/price-and-quantity")]
        Task<Response<GetirPriceAndQuantityOfProductRespDto.Root>> UpdatePriceAndQuantity([Body] GetirPriceAndQuantityOfProductReqDto.Root dto);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Put("/v1/products/price-and-quantity/vendor")]
        Task<Response<GetirPriceAndQuantityOfProductRespDto.Root>> UpdatePriceAndQuantityWithVendor([Body] GetirPriceAndQuantityOfProductWithVendorReqDto.Root dto);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/v1/products/price-and-quantity/batch-requests/{batchRequestId}")]
        Task<Response<GetirGetUpdateProductsResultWithBatchRequesIdtRespDto.Root>> GetBatchRequestsControlById([Path] string batchRequestId);

        #endregion

        #region Return

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/v1/returns/{returnId}")]
        Task<Response<GenericGetirResponse<GetirReturnRespDto>>> GetReturn([Path] string returnId);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/v1/returns/shops/{shopId}?type={type}&page={page}&size={size}")]
        Task<Response<GenericGetirResponse<GetirReturnsRespDto>>> GetReturnedPackagesAsync([Path] string shopId, [Path] string type, [Path] int page, [Path] int size, [Body] ReturnReqBody returnReqBody);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/v1/returns/respond")]
        Task<Response<GenericGetirResponse<GetirReturnsRespDto>>> PostReturn([Body] GetirPostReturnReqBody returnReqBody);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Patch("v1/returns/{returnId}/shops/{shopId}/receive-return")]
        Task<Response<GetirResponse>> PatchReceiveReturn([Path] string shopId, [Path] string returnId);

        #endregion

        #region Shop
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Get("/v1/shops/{shopId}/products?page={page}&pageSize={size}")]
		Task<Response<GenericGetirResponse<GetirProductDataPaged>>> Products([Path] string shopId, [Path] int page = 1, [Path] int size = 1);
		#endregion
	}
}
