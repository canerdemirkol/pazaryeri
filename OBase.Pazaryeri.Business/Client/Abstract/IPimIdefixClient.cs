using OBase.Pazaryeri.Domain.Dtos.Idefix;
using OBase.Pazaryeri.Domain.Dtos.Idefix.PriceStock;
using RestEase;

namespace OBase.Pazaryeri.Business.Client.Abstract
{
    public interface IPimIdefixClient : IDisposable
    {
        #region General       
        #endregion

        #region Order
        #endregion

        #region Price / Stock     
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/catalog/{vendorId}/inventory-upload")]
        Task<Response<IdefixGenericResponse<ProductInventoryItemResponse>>> UpdatePriceAndQuantityWithVendor([Path] string vendorId,[Body] IdefixProductPriceAndStockUpdateWithVendorRequestDto dto);


        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/catalog/{vendorId}/inventory-result/{batchId}")]
        Task<Response<IdefixGenericResponse<ProductInventoryItemWithBatchRequesIdResponse>>> GetBatchRequestsControlById([Path] string vendorId, [Path] string batchId);
        #endregion

        #region Claim
        #endregion
    }
}
