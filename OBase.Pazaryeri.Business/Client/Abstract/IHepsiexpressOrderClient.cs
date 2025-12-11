using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using RestEase;
namespace OBase.Pazaryeri.Business.Client.Abstract
{
    public interface IHepsiExpressOrderClient : IDisposable
    {
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Put("orders/merchantid/{merchantid}/ordernumber/{OrderNumber}/pick")]
        Task<Response<HEUpdateProductRequestDto.Root>> PutAsPicked([Path] string merchantid, [Path] string OrderNumber);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("lineitems/merchantid/{merchantid}/ordernumber/{OrderNumber}/cancel")]
        Task<Response<HEUpdateProductRequestDto.Root>> PutOrderAsCanceled([Path] string merchantid, [Path] string OrderNumber, [Body] HEReasonDto model);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("packages/merchantid/{merchantid}")]
        Task<Response<HEUpdateProductRequestDto.Root>> CompleteOrder([Path] string merchantid, [Body] HECompleteOrderRequestDto model);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("lineitems/merchantid/{merchantid}/id/{lineitemid}/cancelbymerchant")]
        Task<Response<HEUpdateProductRequestDto.Root>> CancelOrderByLineItemId([Path] string merchantid, [Path] string lineitemid, [Body] HEReasonDto model);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("packages/merchantid/{merchantid}/packagenumber/{packageNumber}/unpack")]
        Task<Response<HEUpdateProductRequestDto.Root>> UnPackOrder([Path] string merchantid, [Path] string packageNumber);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("v2/orders/merchantid/{merchantid}")]
        Task<Response<HEPutUpdatePackageResponseDto.Root>> CreateHexOrder([Path] string merchantid, [Body] HECreateOrderDto dto);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/lineitems/v2/merchantid/{merchantid}/id/{lineitemid}/change")]
        Task<Response<HEPutUpdatePackageResponseDto.Root>> ChangeHexOrder([Path] string merchantid, [Path] string lineitemid, [Body] HEChangeOrderDetailsDto dto);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Get("/orders/merchantid/{merchantid}/ordernumber/{ordernumber}")]
        Task<Response<HEPutUpdatePackageResponseDto.Root>> GetOrderDetails([Path] string merchantid, [Path] string ordernumber);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("packages/merchantid/{merchantid}/packagenumber/{packagenumber}/intransit")]
        Task<Response<CommonResponseDto>> InTransitOrder([Path] string merchantid, [Path] string packagenumber, [Body] HEInTransitRequestDto.Root model);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("packages/merchantid/{merchantid}/packagenumber/{packagenumber}/undeliver")]
        Task<Response<CommonResponseDto>> UnDeliverOrder([Path] string merchantid, [Path] string packagenumber, [Body] HEUnDeliverRequestDto.Root model);

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("packages/merchantid/{merchantid}/packagenumber/{packagenumber}/deliver")]
        Task<Response<CommonResponseDto>> DeliverOrder([Path] string merchantid, [Path] string packagenumber, [Body] HEDeliverRequestDto.Root model);
    }
}