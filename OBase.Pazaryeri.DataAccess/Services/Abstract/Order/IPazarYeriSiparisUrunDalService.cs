using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Order
{
    public interface IPazarYeriSiparisUrunDalService : IBaseDalService
    {
        Task<PazarYeriSiparisUrun> GetOrderProductByIdAsync(long obaseOrderId, string obaseMalNo);
        Task<IEnumerable<PazarYeriSiparisUrun>> GetOrderProductsByIdAsync(long obaseOrderId);
        Task<decimal> GetSubProductPriceInfoAsync(string obaseProductNo, string merchantNo);
        Task UpdateProductAsync(PazarYeriSiparisUrun moddel);
        Task AddProductsAsync(List<PazarYeriSiparisUrun> products);
    }
}
