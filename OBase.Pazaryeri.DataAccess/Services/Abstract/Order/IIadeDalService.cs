using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Order
{
    public interface IIadeDalService
	{
		Task InsertReturnOrderClaimAsync(PazarYeriSiparisIade iade);
		Task InsertReturnOrderClaimDetailsAsync(PazarYeriSiparisIadeDetay iadeDetay);
		Task UpdateReturnOrderClaimAsAcceptedAsync(string claimId, string status);
		Task UpdateReturnOrderClaimAsRejectedAsync(string claimId, string description, string status);
		Task UpdateClaimAsSentAsync(string claimId);
		Task UpdateClaimsTryCountAsync(PazarYeriSiparisIade iade);
		Task<string[]> GetClaimItemIdsByClaimIdAsync(string claimId);
		Task<string[]> GetClaimItemIdsByOrderIdAsync(string orderId);
		Task<bool> ClaimExistsAsync(string claimId);
		Task<double> GetIdByOrderNumberAscAsync(string orderNumber);
		Task<double> GetIdByOrderNumberDescAsync(string orderNumber);
		Task<double> GetIdByOrderIdAsync(string orderId);
		List<ReturnClaimProductInfo> GetClaimProductInfos(List<string> lineItemIds);
		Task<bool> CheckIfClaimSentToQpAsync(string claimId);
		Task<List<PazarYeriSiparisIade>> GetClaimsToSendQpAsync(string merchantNo);
	}
}
