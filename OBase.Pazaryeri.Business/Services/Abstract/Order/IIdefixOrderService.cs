using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Idefix;
using OBase.Pazaryeri.Domain.Dtos.Idefix.Unsupplied;

namespace OBase.Pazaryeri.Business.Services.Abstract.Order
{
    public interface IIdefixOrderService : IOrderService
    {
        Task ProcessIdefixCreatedOrdersAsync(Dictionary<string, string> properties);
        Task ProcessIdefixCancelledOrdersAsync(Dictionary<string, string> properties);
        Task<ServiceResponse<CommonResponseDto>> SaveIdefixCreatedOrderAsync(IdefixOrderDto order);
        Task<ServiceResponse<UnsuppliedResponse>> MarkShipmentAsUnsuppliedAsync(string vendorId, string shipmentId, UnsuppliedRequest unsupplied);
    }
}