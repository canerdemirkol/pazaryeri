using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Order
{
    public interface IPazarYeriSiparisEkBilgiDalService : IBaseDalService
    {
        Task<PazarYeriSiparisEkBilgi> GetAdditionalDataAsync(long obaseSiparisId);
		Task<IEnumerable<PazarYeriSiparisEkBilgi>> GetAdditionalDatasByOrderNumberAsync(string siparisNo);
        Task UpdateOrderAdditionalDataAsync(PazarYeriSiparisEkBilgi model);
        Task AddAdditionalDataAsync(PazarYeriSiparisEkBilgi model);
    }
}
