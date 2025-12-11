using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Order
{
	public class PazarYeriAktarimDalService : BaseDalService, IPazarYeriAktarimDalService
	{
        private readonly IOptions<AppSettings> _appSettings;

        public PazarYeriAktarimDalService(IRepository repository, IOptions<AppSettings> appSettings) : base(repository)
        {
            _appSettings = appSettings;
        }
        public async Task<IEnumerable<PazarYeriAktarim>> GetProductObaseNoAndBarcodeAsync(string merchantNo, string storeId)
		{
			var result = await _repository.GetTable<PazarYeriAktarim>().Where(x => x.PazarYeriNo == merchantNo && x.PazarYeriBirimNo == storeId).ToListAsync();
			return result;
		}
		public async Task<bool> AddOrderAsync(PazarYeriAktarim order)
		{
			try
			{
				await _repository.GetTable<PazarYeriAktarim>().AddAsync(order);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
		public async Task<IEnumerable<PazarYeriAktarim>> GetPyTransferredProductsAsync(string merchantNo, string storeId, List<string> pazaryeriBarcodeList = null)
		{
			string query = "";
			if (pazaryeriBarcodeList != null && pazaryeriBarcodeList.Any())
			{
                var barkodes = string.Join(",", pazaryeriBarcodeList.Select(b => $"'{b}'"));
                query = string.Format(_appSettings.Value.RawDatabaseQueries.PazarYeriAktarimInBarcodesWithLatestPrice, merchantNo, storeId, barkodes);
			}
			else
			{
                query = string.Format(_appSettings.Value.RawDatabaseQueries.PazarYeriAktarimWithLatestPrice, merchantNo, storeId);
            }
            return await Task.FromResult(_repository.ExecuteSqlCommand<PazarYeriAktarim>(query).ToList());
		}
		public async Task<IEnumerable<PazarYeriAktarim>> GetProductsByProductNoAsync(List<string> ObaseProducts, string merchantNo)
		{
			var result = await _repository.GetTable<PazarYeriAktarim>().Where(x => ObaseProducts.Contains(x.MalNo) && x.PazarYeriNo == merchantNo).Select(x => new PazarYeriAktarim
			{
				MalNo = x.MalNo,
				PazarYeriMalNo = x.PazarYeriMalNo,
				SatisFiyat = x.SatisFiyat
			}).ToListAsync();
			return result;
		}
		public async Task<IEnumerable<PazarYeriAktarim>> GetPyTransferredProductListAsync(string merchantNo, string storeId, List<string> skuList = null)
		{
            string query = "";
			if (skuList != null && skuList.Any()) 
			{
                var malNos = string.Join(",", skuList.Select(b => $"'{b}'"));
                query = string.Format(_appSettings.Value.RawDatabaseQueries.MalSatisFiyatSelectWithMalNos, merchantNo, storeId, malNos);
            }
			else
			{
                query = string.Format(_appSettings.Value.RawDatabaseQueries.MalSatisFiyatSelect, merchantNo, storeId);
            }				
			return await Task.FromResult(_repository.ExecuteSqlCommand<PazarYeriAktarim>(query).ToList());
		}
	}
}