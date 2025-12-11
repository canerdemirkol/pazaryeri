using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Trendyol;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using RestEase;
using System.Threading.Tasks;
using static OBase.Pazaryeri.Domain.Dtos.TrendyolGo.TGUpdateStockAndPriceReqDto;

namespace OBase.Pazaryeri.Business.Client.Abstract
{
    /// <summary>
    ///
    /// </summary>
	public interface ITrendyolGoClient
	{
        #region Orders
        /// <summary>
        /// Yeni Sipariş Paketlerini Çekme (GetShipmentPackages)
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="status"></param>
        /// <param name="storeId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Get("/integrator/order/grocery/suppliers/{supplierId}/packages?status={status}&storeId={storeId}&sortDirection=DESC&startDate={startDate}&endDate={endDate}&size=200&page={page}")]
        Task<Response<TGShipmentPackageResponseDto>> GetShipmentPackages([Path] string supplierId, [Path] string status, [Path] string storeId, [Path] string startDate, [Path] string endDate, [Path] string page);

        /// <summary>
        /// Tedarik Edememe Bildirimi (updatePackage)
        /// </summary>
        /// <param name="sellerId"></param>
        /// <param name="packageId"></param>
        /// <param name="status"></param>
        /// <returns></returns>        
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Put("/integrator/order/grocery/suppliers/{sellerId}/packages/{packageId}/{status}")]
        Task<Response<CommonResponseDto>> PutUpdatePackage([Path] string sellerId, [Path] string packageId, [Path] string status);


        /// <summary>
        /// Yeni Sipariş Paketlerini Çekme (get orderNumber)
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="orderNumber"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Get("/integrator/order/grocery/suppliers/{supplierId}/packages/order-number/{orderNumber}")]
		Task<Response<TGShipmentPackageResponseDto>> GetOrderPackagesByOrderNumber([Path] string supplierId, [Path] string orderNumber);

        /// <summary>
        /// Sipariş Kabul Edildi Bildirimi (updatePackage)
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Put("/integrator/order/grocery/suppliers/{supplierId}/packages/{packageId}/picked")]
		Task<Response<CommonResponseDto>> Picked([Path] string supplierId, [Path] string packageId);

        /// <summary>
        /// Sipariş Yola Çıktı Bildirimi
        /// </summary>
        /// <param name="sellerId"></param>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Put("/integrator/order/grocery/suppliers/{sellerId}/packages/{packageId}/manual-shipped")]
		Task<Response<CommonResponseDto>> ManualShipped([Path] string sellerId, [Path] string packageId);

        /// <summary>
        /// Sipariş Teslim Edildi Bildrimi
        /// </summary>
        /// <param name="sellerId"></param>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Put("/integrator/order/grocery/suppliers/{sellerId}/packages/{packageId}/manual-delivered")]
		Task<Response<CommonResponseDto>> ManualDelivered([Path] string sellerId, [Path] string packageId);

        /// <summary>
        /// Sipariş Hazırlanmıştır Bildrimi
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="packageId"></param>
        /// <param name="updatePackageAsInvoicedDto"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Put("/integrator/order/grocery/suppliers/{supplierId}/packages/{packageId}/invoiced")]
		Task<Response<CommonResponseDto>> UpdatePackageAsInvoiced([Path] string supplierId, [Path] string packageId, [Body] TGUpdatePackageInvoicedRequestDto updatePackageAsInvoicedDto);


        /// <summary>
        /// Sipariş Hazırlanmıştır Bildrimi
        /// </summary>
        /// <param name="sellerId"></param>
        /// <param name="packageId"></param>
        /// <param name="updatePackageAsUnSuppliedDto"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Put("/integrator/order/grocery/suppliers/{sellerId}/packages/{packageId}/unsupplied")]
		Task<Response<CommonResponseDto>> UpdatePackageAsUnSupplied([Path] string sellerId, [Path] string packageId, [Body] TGUpdatePackageUnSuppliedRequestDto updatePackageAsUnSuppliedDto);


        /// <summary>
        /// Alternatif Ürün Bildirimi
        /// </summary>
        /// <param name="sellerid"></param>
        /// <param name="packageId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Put("/integrator/order/grocery/suppliers/{sellerId}/packages/{packageId}/mark-alternative")]
		Task<Response<CommonResponseDto>> MarkAlternative([Path] string sellerId, [Path] string packageId, [Body] TGMarkAlternativeRequestDto dto);


        /// <summary>
        /// GET Invoice amount
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
        [Get("/integrator/order/grocery/suppliers/{supplierId}/orders/{orderId}/invoice-amount")]
        Task<Response<TGInvoiceLimitResponseDto>> GetInvoiceAmount([Path] string supplierId, [Path] string orderId);

        /// <summary>
        /// The method that connects customer and store via TrendyolGo.
        /// </summary>
        /// <param name="supplierId"></param>
        /// <param name="packageId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/integrator/order/grocery/suppliers/{supplierId}/packages/{packageId}/bridge")]
        Task<Response<TrendyolCallCustomerResponseDto>> GetInstantCall([Path] string supplierId, [Path] string packageId, [Body] TGGetInstantCallRequestDto dto);

        #endregion

        #region Price / Stock
        /// <summary>
        /// Şube Bazlı Ürün Stok ve Fiyat Güncellemesi
        /// storeupdatePriceAndInventory -- düzeltildi
        /// </summary>
        /// <param name="sellerId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Post("/integrator/product/grocery/suppliers/{sellerId}/products/price-and-inventory")]
		Task<Response<TGUpdateStockAndPriceRespDto>> StoreUpdatePriceAndInventory([Path] string sellerId, [Body] RequestItems dto);


        /// <summary>
        /// Toplu İşlem Kontrolü (getBatchRequestResult) -- düzeltildi
        /// </summary>
        /// <param name="supplierid"></param>
        /// <param name="batchRequestId"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Get("/integrator/product/grocery/suppliers/{sellerId}/products/batch-requests/{batchRequestId}")]
		Task<Response<TGGetBatchRequestResultDto.Root>> GetBatchRequestResult([Path] string sellerId, [Path] string batchRequestId);
        #endregion

        #region Iade

        /// <summary>
        /// İadesi Oluşan Siparişleri Çekme
        /// </summary>
        /// <param name="sellerId"></param>
        /// <param name="status"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Get("/integrator/claim/grocery/suppliers/{sellerId}/claims?claimItemStatus={status}&startDate={startDate}&endDate={endDate}&size=200&page={page}")]
		Task<Response<TGGetReturnedPackagesResponseDto>> GetReturnedPackagesAsync([Path] string sellerId, [Path] string status, [Path] string startDate, [Path] string endDate, [Path] string page);


        /// <summary>
        /// Iade Onaylama - Reddetme
        /// </summary>
        /// <param name="sellerId"></param>
        /// <param name="claimId"></param>
        /// <param name="acceptClaimDto"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Put("/integrator/claim/grocery/suppliers/{sellerId}/claims/{claimId}/accept")]
		Task<Response<CommonResponseDto>> AcceptClaimAsync([Path] string sellerId, [Path] string claimId, [Body] TGAcceptClaimRequestDto acceptClaimDto);


        /// <summary>
        /// Iade Onaylama - Reddetme
        /// </summary>
        /// <param name="sellerId"></param>
        /// <param name="claimId"></param>
        /// <param name="rejectClaimDto"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
		[Header("Cache-Control", "no-cache")]
		[AllowAnyStatusCode]
		[Put("/integrator/claim/grocery/suppliers/{sellerId}/claims/{claimId}/reject")]
		Task<Response<CommonResponseDto>> RejectClaimAsync([Path] string sellerId, [Path] string claimId, [Body] TGRejectClaimRequestDto rejectClaimDto);
        #endregion        
    }
}