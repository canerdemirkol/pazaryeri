using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Order
{
    public interface IPazarYeriBirimTanimDalService : IBaseDalService
    {
        Task<IEnumerable<PazarYeriBirimTanim>> GetMerchantListAsync(string merchantNo);
        Task<PazarYeriBirimTanim> GetStoreDetailAsync(string merchantNo, string marketplaceUnitNumber);
        Task<IEnumerable<string>> GetPyStoreNoListAsync(string merchantNo);

        /// <summary>
        /// Belirtilen mağazaya (merchant) ait birim tanımlarını getirir.
        /// </summary>
        /// <param name="merchantNo">Mağazaya ait benzersiz numara.</param>
        /// <param name="onlyActive">
        /// Eğer <c>true</c> ise sadece aktif birim tanımları döner.
        /// Eğer <c>false</c> ise hem aktif hem de pasif olan tüm birim tanımları döner.
        /// Varsayılan değer <c>false</c>'dur.
        /// </param>
        /// <returns>Birim tanımlarının listesi.</returns>
        Task<IEnumerable<PazarYeriBirimTanim>> GetStoreDetailsListAsync(string merchantNo, bool onlyActive = false);

    }
}
