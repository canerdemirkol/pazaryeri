using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Order
{
    public class PazarYeriMalTanimDalService : BaseDalService, IPazarYeriMalTanimDalService
    {
        public PazarYeriMalTanimDalService(IRepository repository) : base(repository){}
        public async Task<IEnumerable<PazarYeriMalTanim>> GetProductSalesValueByUnitAsync(string unit,string merchantNo)
        {
            var result = await _repository.GetTable<PazarYeriMalTanim>().Where(x => x.PyUrunSatisBirim == unit && x.PazarYeriNo==merchantNo).Select(s => new PazarYeriMalTanim()
            {
                MalNo = s.MalNo,
                PyUrunSatisDeger = s.PyUrunSatisDeger,
            }).ToListAsync();
            return result;
        }
        public async Task<IEnumerable<PazarYeriMalTanim>> GetProductObaseNoAsync(string merchantNo)
        {
            var result = await _repository.GetTable<PazarYeriMalTanim>().Where(x => x.PazarYeriNo == merchantNo).ToListAsync();
            return result;
        }

        public async Task<IEnumerable<PazarYeriMalTanim>> GetPyProductNosAsync(List<string> pyProductNos, string merchantNo)
        {
            var result = await _repository.GetTable<PazarYeriMalTanim>().Where(x => x.PazarYeriNo == merchantNo && pyProductNos.Contains(x.PazarYeriMalNo)).ToListAsync();
            return result;
        }

        public async Task<string> GetProductMarketPlaceIdByObaseProductIdAsync(string productNo, string merchantNo)
        {
            return await _repository.GetTable<PazarYeriMalTanim>().Where(x => x.PazarYeriNo == merchantNo && x.MalNo == productNo).Select(s => s.PazarYeriMalNo).FirstOrDefaultAsync();

        }
        public async Task<IEnumerable<PazarYeriMalTanim>> GetPyProductsAsync(string merchantNo, List<string> malNos = null)
        {
            var query = _repository.GetTable<PazarYeriMalTanim>()
                .Where(x => x.PazarYeriNo == merchantNo);

            if (malNos != null && malNos.Any())
            {
                var trimmedMalNos = malNos
                    .Where(b => !string.IsNullOrWhiteSpace(b))
                    .Select(b => b.Trim())
                    .ToList();

                query = query.Where(x => trimmedMalNos.Contains(x.MalNo.Trim()));
            }

            return await query.Select(x => new PazarYeriMalTanim()
            {
                MalNo = x.MalNo,
                PazarYeriMalNo = x.PazarYeriMalNo,
                PyUrunSatisBirim = x.PyUrunSatisBirim,
                PyUrunSatisDeger = x.PyUrunSatisDeger,
                ImageUrl = x.ImageUrl
            }).ToListAsync();
        }


        public async Task<decimal> GetProductSalesValue(string MerchantNo, string MerchantSKU)
        {
            return await _repository.GetTable<PazarYeriMalTanim>().Where(x => x.PazarYeriNo == MerchantNo && x.PazarYeriMalNo == MerchantSKU).Select(x => x.PyUrunSatisDeger).FirstOrDefaultAsync() ?? 0;
        }
        public async Task<IEnumerable<PazarYeriMalTanim>> GetProductDetailsAsync(string MerchantNo, string MerchantSKU)
        {
            return await _repository.GetTable<PazarYeriMalTanim>().Where(x => x.PazarYeriNo == MerchantNo && x.PazarYeriMalNo == MerchantSKU)
                .Select(x =>new PazarYeriMalTanim() {
                PyUrunSatisDeger = x.PyUrunSatisDeger ,
                PyUrunSatisBirim=x.PyUrunSatisBirim,
                MalNo = x.MalNo ,
                PazarYeriMalNo=x.PazarYeriMalNo,
                PazarYeriMalAdi=x.PazarYeriMalAdi
            }).ToListAsync();
        }

        public async Task<bool> UpdateProductAsync(PazarYeriMalTanim model)
        {
            try
            {
                await _repository.UpdateAsync(model);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}