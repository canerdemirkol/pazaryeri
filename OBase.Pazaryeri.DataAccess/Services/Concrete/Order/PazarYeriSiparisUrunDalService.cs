using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Order
{
    public class PazarYeriSiparisUrunDalService : BaseDalService, IPazarYeriSiparisUrunDalService
	{
		public PazarYeriSiparisUrunDalService(IRepository repository) : base(repository) { }
		public async Task<PazarYeriSiparisUrun> GetOrderProductByIdAsync(long obaseOrderId, string obaseMalNo)
		{
			return await _repository.GetTable<PazarYeriSiparisUrun>().FirstOrDefaultAsync(x => x.ObaseSiparisId == obaseOrderId && x.ObaseMalNo == obaseMalNo);
		}
		public async Task<IEnumerable<PazarYeriSiparisUrun>> GetOrderProductsByIdAsync(long obaseOrderId)
		{
			return await _repository.GetTable<PazarYeriSiparisUrun>().Where(x => x.ObaseSiparisId == obaseOrderId).ToListAsync();
		}
		public async Task AddProductsAsync(List<PazarYeriSiparisUrun> products)
		{
			await _repository.AddRangeAsync(products);
		}
		public async Task UpdateProductAsync(PazarYeriSiparisUrun moddel)
		{
			await _repository.UpdateAsync(moddel);
		}
		public async Task<decimal> GetSubProductPriceInfoAsync(string obaseProductNo,string merchantNo)
		{
			var result = await _repository.GetTable<VPazaryeriUrunler>().FirstOrDefaultAsync(x => x.MalNo == obaseProductNo && x.PazarYeriNo== merchantNo);
			return result.SatisFiyat;
		}
	}
}