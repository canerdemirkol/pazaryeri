using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using RestEase;

namespace OBase.Pazaryeri.Business.Services.Abstract.Return
{
    public interface ITrendyolGoReturnService
    {
        Task<Response<TGGetReturnedPackagesResponseDto>> GetTyGoClaimsAsync(string startDate, string status, string endDate, string page);
        Task<bool> SaveClaimToDbAsync(ClaimContent claim);
        Task SendClaimToQPAsync(ClaimContent claim);
        Task SendIadeToQPAsync(PazarYeriSiparisIade iade);
        Task<CommonResponseDto> AcceptClaimAsync(PostProductReturnRequestDto request);
        Task<CommonResponseDto> RejectClaimAsync(PostProductReturnRequestDto request);
        Task<List<PazarYeriSiparisIade>> GetClaimsToSendQpAsync(string merchantNo);
        Task UpdateClaimsTryCountAsync(PazarYeriSiparisIade iade);
    }
}
