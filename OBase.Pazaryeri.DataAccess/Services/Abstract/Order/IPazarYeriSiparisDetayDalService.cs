using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Order
{
    public interface IPazarYeriSiparisDetayDalService : IBaseDalService
    {
        Task<PazarYeriSiparisDetay> GetOrderDetailByIdAsync(long orderId);
        Task<IEnumerable<PazarYeriSiparisDetay>> GetOrderDetailsdByIdAsync(long orderId);
        Task<IEnumerable<PazarYeriSiparisDetay>> GetOrderDetailsAsync(long orderId, List<string> packageIdList = null);
        Task<PazarYeriSiparisDetay> GetOrderDetailByLineItemIdAsync(string lineItemId);
        decimal GetQuantityByLineItemId(string lineItemId);
        Task AddOrderDetailsAsync(List<PazarYeriSiparisDetay> details);
        Task UpdateOrderDetailAsync(PazarYeriSiparisDetay model);
        Task UpdateOrderDetailRangeAsync(IEnumerable<PazarYeriSiparisDetay> models);
        Task<IEnumerable<PazarYeriSiparisDetay>> GetOrderDetailsByOrderIdProductNoAsync(long orderId, List<string> productNoList = null);
        bool IsItemCancelled(string qpProductId, long orderId);
        bool IsTheOrderToBeCancelled(long orderId);
        Task<IEnumerable<PazarYeriSiparisDetay>> GetOrderDetailsByLineItemIdsAsync(long orderId, List<string> lineItemIdList = null);
    }
}
