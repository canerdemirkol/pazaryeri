using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Order
{
    public class PazarYeriBirimTanimDalService : BaseDalService, IPazarYeriBirimTanimDalService
    {
        public PazarYeriBirimTanimDalService(IRepository repository) : base(repository) { }
        public async Task<IEnumerable<PazarYeriBirimTanim>> GetMerchantListAsync(string merchantNo)
        {
            return await _repository.GetTable<PazarYeriBirimTanim>().Include(s => s.Birim).Where(x => x.PazarYeriNo == merchantNo).ToListAsync();
           
        }
        public async Task<PazarYeriBirimTanim> GetStoreDetailAsync(string merchantNo,string marketplaceUnitNumber)
        {
           return await _repository.GetTable<PazarYeriBirimTanim>().Where(x => x.PazarYeriNo == merchantNo && x.PazarYeriBirimNo== marketplaceUnitNumber).Include(s => s.Birim).Select(x => new PazarYeriBirimTanim
            {
                BirimAdi = x.Birim.BirimAdi,
                BirimNo = x.BirimNo,
                PazarYeriNo = x.PazarYeriNo,
                PazarYeriBirimNo = x.PazarYeriBirimNo
            }).FirstOrDefaultAsync();
            
        }
        public async Task<IEnumerable<PazarYeriBirimTanim>> GetStoreDetailsListAsync(string merchantNo, bool onlyActive = false)
        {
            var query = _repository.GetTable<PazarYeriBirimTanim>()
                .Where(x => x.PazarYeriNo == merchantNo);

            if (onlyActive)
            {
                query = query.Where(x => x.AktifPasif == CommonConstants.Aktif);
            }

            query = query.Include(s => s.Birim);

            return await query.Select(x => new PazarYeriBirimTanim
            {
                BirimAdi = x.Birim.BirimAdi,
                BirimNo = x.BirimNo,
                PazarYeriNo = x.PazarYeriNo,
                PazarYeriBirimNo = x.PazarYeriBirimNo
            }).ToListAsync();

        }
        public async Task<IEnumerable<string>> GetPyStoreNoListAsync(string merchantNo)
        {          
           return await _repository.GetTable<PazarYeriBirimTanim>().Where(x => x.PazarYeriNo == merchantNo && x.AktifPasif == CommonConstants.Aktif).Select(x => x.PazarYeriBirimNo).ToListAsync();            
        }    
    }
}