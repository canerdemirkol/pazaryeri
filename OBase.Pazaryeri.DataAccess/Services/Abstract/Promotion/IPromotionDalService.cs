using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Promotion
{
    public interface IPromotionDalService : IBaseDalService
    {
        Task<List<PyPromosyonEntegrasyon>> GetPendingPromotionsAsync();
        Task<bool> ExecutePromotionProcedureAsync(long promotionNo, long id);
        Task<(TmpPyPromosyonTanim? Tanim, List<TmpPyPromosyonDetay>? Detaylar,List<TmpPyPromosyonBirim>? Birimler)> GetTmpPromotionDataAsync(long promosyonId, long promosyonNo,string merchantNo);   

        Task<bool> MarkPromotionAsSentAsync(long promotionNo, string pazaryeriPromoNo);
        Task UpdatePromotionErrorAsync(long promotionNo, string errorMessage);
    }
}
