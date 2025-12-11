using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;

namespace OBase.Pazaryeri.Business.Services.Abstract.Order
{
    public interface IHepsiExpressOrderService : IOrderService
    {
        Task SaveOrderOnQp(HEOrderDto order);
        Task EndUserCancellation(string lineItemId, HEEndUserCancellationModel hepsiExpressEndUserCancellationModel);
    }
}