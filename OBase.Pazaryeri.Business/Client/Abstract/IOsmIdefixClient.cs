using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Idefix;
using OBase.Pazaryeri.Domain.Dtos.Idefix.PriceStock;
using OBase.Pazaryeri.Domain.Dtos.Idefix.Shipment;
using OBase.Pazaryeri.Domain.Dtos.Idefix.Unsupplied;
using RestEase;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Client.Abstract
{
    public interface IOsmIdefixClient : IDisposable
    {
        #region General       
        #endregion

        #region Order

        /// <summary>
        ///  Yeni Sipariş Listesi Çekme (list)
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="page"></param>
        /// <param name="state"> Default state = "created" </param> 
        /// <param name="limit"> Default limit = 200 </param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/{vendorId}/list?sortDirection=DESC")]
        Task<Response<IdefixGenericResponse<IdefixOrderDto>>> GetShipmentList([Path] string vendorId, [Query] string page, [Query] string startDate = "", [Query] string endDate = "", [Query] string state = "", [Query] int limit = 200);


        /// <summary>
        ///  Shipment Statü Bilgileri Güncelleme (update-shipment-status)
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="shipmentId"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/{vendorId}/{shipmentId}/update-shipment-status")]
        Task<Response<CommonResponseDto>> UpdateShipmentStatusAsync([Path] string vendorId, [Path] string shipmentId, [Body] UpdateShipmentStatusRequest body);
        #endregion

        #region Claim
        // <summary>
        /// Tedarik edilemedi bildirimi (unsupplied)
        /// </summary>
        /// <param name="vendorId">Satıcı/Vendor ID</param>
        /// <param name="shipmentId">Shipment ID</param>
        /// <param name="request">Tedarik edilemedi bildirimi için request gövdesi</param>     
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/{vendorId}/{shipmentId}/unsupplied")]
        Task<Response<UnsuppliedResponse>> MarkShipmentAsUnsuppliedAsync([Path] string vendorId, [Path] string shipmentId, [Body] UnsuppliedRequest request);
        #endregion
    }
}
