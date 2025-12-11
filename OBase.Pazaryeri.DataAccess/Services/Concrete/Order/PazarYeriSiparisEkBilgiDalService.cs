using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using OBase.Pazaryeri.Domain.Entities;
using System.Data;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Order
{
	public class PazarYeriSiparisEkBilgiDalService : BaseDalService, IPazarYeriSiparisEkBilgiDalService
	{
		public PazarYeriSiparisEkBilgiDalService(IRepository repository) : base(repository) { }
		public async Task<PazarYeriSiparisEkBilgi> GetAdditionalDataAsync(long obaseSiparisId)
		{
			var result = await _repository.GetTable<PazarYeriSiparisEkBilgi>().FirstOrDefaultAsync(x => x.ObaseSiparisId == obaseSiparisId);
			return result;
		}
		public async Task<IEnumerable<PazarYeriSiparisEkBilgi>> GetAdditionalDatasByOrderNumberAsync(string siparisNo)
		{
			return await _repository.GetTable<PazarYeriSiparisEkBilgi>().Where(x => x.PySiparisNo == siparisNo).OrderBy(x => x.ObaseSiparisId).ToListAsync();
		}
		public async Task AddAdditionalDataAsync(PazarYeriSiparisEkBilgi model)
		{
			await _repository.AddAsync(model);
		}
		public async Task UpdateOrderAdditionalDataAsync(PazarYeriSiparisEkBilgi moddel)
		{
			await _repository.UpdateAsync(moddel);
		}
	}
}