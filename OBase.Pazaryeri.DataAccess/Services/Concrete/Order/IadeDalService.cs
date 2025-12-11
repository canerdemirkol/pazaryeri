using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Order
{
	public class IadeDalService : IIadeDalService
	{
		private readonly ICreateDalService _createDalService;
		protected readonly IRepository _repository;
		private readonly IGetDalService _getDalService;
		private readonly IUpdateDalService _updateDalService;

		public IadeDalService(ICreateDalService createDalService, IGetDalService getDalService, IUpdateDalService updateDalService, IRepository repository)
		{
			_createDalService = createDalService;
			_getDalService = getDalService;
			_updateDalService = updateDalService;
			_repository = repository;
		}
		public async Task<bool> CheckIfClaimSentToQpAsync(string claimId)
		{
			return !await _repository.GetTable<PazarYeriSiparisIade>().AnyAsync(x => x.ClaimId == claimId && x.DepoAktarildiEH == Constants.Character.E);
		}

		public async Task<bool> ClaimExistsAsync(string claimId)
		{
			return await _repository.GetTable<PazarYeriSiparisIade>().AnyAsync(x => x.ClaimId == claimId);
		}

		public async Task<string[]> GetClaimItemIdsByClaimIdAsync(string claimId)
		{
			return await _getDalService.GetTable<PazarYeriSiparisIadeDetay>(x => x.ClaimId == claimId).Select(x => x.ClaimDetayId).ToArrayAsync();
		}

		public async Task<string[]> GetClaimItemIdsByOrderIdAsync(string orderId)
		{
			var claimResult = await _getDalService.GetAsync<PazarYeriSiparisIade>(x => x.Id.ToString() == orderId
			, x => x.PazarYeriSiparisIadeDetay);
			return claimResult.PazarYeriSiparisIadeDetay.Select(x => x.ClaimDetayId).ToArray();
		}

		public List<ReturnClaimProductInfo> GetClaimProductInfos(List<string> lineItemIds)
		{
			string query = @"SELECT 
								PAZAR_YERI_MAL_ADI as PazarYeriMalAdi, 
								OBASE_MAL_NO as ObaseMalNo, 
								count(obase_mal_no) as Miktar, 
								LINE_ITEM_ID as LineItemId
								from pazar_yeri_siparis_detay where 1 = 2";
			foreach (var item in lineItemIds)
			{
				query += " OR LINE_ITEM_ID = '" + item + "'";
			}
			query += "group by OBASE_MAL_NO, PAZAR_YERI_MAL_ADI, LINE_ITEM_ID";
			return _repository.ExecuteSqlCommand<ReturnClaimProductInfo>(query).ToList();
		}

		public async Task<double> GetIdByOrderNumberAscAsync(string orderNumber)
		{
			var result = await _repository.GetTable<PazarYeriSiparis>().OrderBy(x => x.InsertDatetime).FirstOrDefaultAsync(x => x.SiparisNo == orderNumber);
			return result != null ? Convert.ToDouble(result.Id) : 0;
		}

		public async Task<double> GetIdByOrderNumberDescAsync(string orderNumber)
		{
			var result = await _repository.GetTable<PazarYeriSiparis>().OrderByDescending(x => x.InsertDatetime).FirstOrDefaultAsync(x => x.SiparisNo == orderNumber);
			return result != null ? Convert.ToDouble(result.Id) : 0;
		}

        public async Task<double> GetIdByOrderIdAsync(string orderId)
		{
            var result = await _repository.GetTable<PazarYeriSiparis>().FirstOrDefaultAsync(x => x.SiparisId == orderId);
            return result != null ? Convert.ToDouble(result.Id) : 0;
        }

        public async Task InsertReturnOrderClaimAsync(PazarYeriSiparisIade iade)
		{
			await _createDalService.AddAsync(iade);
		}

		public async Task InsertReturnOrderClaimDetailsAsync(PazarYeriSiparisIadeDetay iadeDetay)
		{
			await _createDalService.AddAsync(iadeDetay);
		}
		public async Task<List<PazarYeriSiparisIade>> GetClaimsToSendQpAsync(string merchantNo)
		{
			return await _getDalService.GetListAsync<PazarYeriSiparisIade>(x => x.DepoAktarildiEH == Constants.Character.H && x.DepoAktarimDenemeSayisi < 3 && x.PazarYeriNo == merchantNo, y => y.PazarYeriSiparisIadeDetay);
		}
		public async Task UpdateClaimAsSentAsync(string claimId)
		{
			var claim = await _getDalService.GetAsync<PazarYeriSiparisIade>(x => x.ClaimId == claimId);
			claim.DepoAktarildiEH = Constants.Character.E;
			await _updateDalService.UpdateAsync(claim);
		}

		public async Task UpdateReturnOrderClaimAsAcceptedAsync(string claimId, string status)
		{
			var claim = await _getDalService.GetAsync<PazarYeriSiparisIade>(x => x.ClaimId == claimId, y => y.PazarYeriSiparisIadeDetay);
			claim.BirimAciklama = string.Empty;
			claim.ClaimStatus = status;
			claim.PazarYeriSiparisIadeDetay.ForEach(item =>
			{
				item.ClaimItemStatus = status;
			});
			await _updateDalService.UpdateAsync(claim);

		}

		public async Task UpdateReturnOrderClaimAsRejectedAsync(string claimId, string description, string status)
		{
			var claim = await _getDalService.GetAsync<PazarYeriSiparisIade>(x => x.ClaimId == claimId, y => y.PazarYeriSiparisIadeDetay);
			claim.BirimAciklama = description;
			claim.ClaimStatus = status;
			claim.PazarYeriSiparisIadeDetay.ForEach(item =>
			{
				item.ClaimItemStatus = status;
			});
			await _updateDalService.UpdateAsync(claim);
		}

		public async Task UpdateClaimsTryCountAsync(PazarYeriSiparisIade iade)
		{
			iade.DepoAktarimDenemeSayisi = iade.DepoAktarimDenemeSayisi == null ? 1 : iade.DepoAktarimDenemeSayisi + 1;
			await _updateDalService.UpdateAsync(iade);
		}
	}
}
