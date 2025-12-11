using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;

namespace OBase.Pazaryeri.Business.Services.Abstract.Order
{
    public interface IGetirCarsiOrderService : IOrderService
    {
        Task<ServiceResponse<CommonResponseDto>> SaveOrderOnQp(GetirOrderDto order);
    }
}