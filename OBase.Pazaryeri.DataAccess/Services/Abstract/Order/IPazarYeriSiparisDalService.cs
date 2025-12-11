using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Order
{
    public interface IPazarYeriSiparisDalService : IBaseDalService
    {
        Task<bool> OrderExistByIdAsync(string orderId, string merchantNo);
        Task<PazarYeriSiparis> GetOrderByIdAsync(long orderId);
        Task<PazarYeriSiparis> GetOrderByIdWithDetailsAsync(long orderId);
        Task<PazarYeriSiparis> GetOrderByPackageIdAsync(string orderPackageId);
        Task<PazarYeriSiparis> GetOrderByOrderIdAsync(string orderId);
        Task<IEnumerable<PazarYeriSiparis>> GetOrderInformationsByIdAsync(long orderId);
        Task<PazarYeriSiparis> GetOrderWithOrderIdAsync(string orderId, string merchantNo);
        Task<PazarYeriSiparis> GetOldestOrderWithOrderIdAsync(string orderId, string merchantNo);

		Task AddOrderAsync(PazarYeriSiparis order);
        Task UpdateOrderAsync(PazarYeriSiparis model);
        Task<bool> OrderExistAsync(string orderPackageId, string merchantNo);
        Task<long> GetSeqId();
        Task<long> GetOrderIdByIdAsync(string orderPackageId, string merchantNo);
        Task<long> GetOrderWareHouseTransferredCountAsync(string orderNo, string merchantNo);
        Task<long> GetOrdersCountWithSameOrderIdAsync(string orderNo, string merchantNo);

        Task<PazarYeriSiparis> GetCreatedOrderByOrderNumberAsync(string orderNo, string merchantNo, string sevkiyatPaketDurumu = "Created");

        Task InsertEmailHareketAsync(string subject, string body);
        Task UpdateOrderStatusAsync(string orderId , string status , string merchantNo);

    }
}