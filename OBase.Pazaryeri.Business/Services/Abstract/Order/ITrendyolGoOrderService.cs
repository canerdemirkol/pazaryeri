using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;

namespace OBase.Pazaryeri.Business.Services.Abstract.Order
{
    public interface ITrendyolGoOrderService : IOrderService
    {
        Task ProcessTyGoCreatedOrdersAsync(Dictionary<string, string> properties);
        Task ProcessTyGoCancelledOrdersAsync(Dictionary<string, string> properties);
        Task<ServiceResponse<CommonResponseDto>> SaveTyGoCreatedOrderAsync(TrendyolGoOrderDto order);
    }
}