using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Getir.Return;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;

namespace OBase.Pazaryeri.Business.Services.Abstract.Return
{
    public interface IGetirCarsiReturnService : IDisposable
    {
        Task<List<GetirReturnsItemDtoWithShopId>> GetGetirClaimsAsync(string status, ReturnReqBody startend);
        Task<bool> SaveClaimToDbAsync(GetirReturnsItemDtoWithShopId claim);
        Task SendClaimToQPAsync(GetirReturnsItemDtoWithShopId claim);
        Task<CommonResponseDto> AcceptClaimAsync(PostProductReturnRequestDto request);
        Task<CommonResponseDto> RejectClaimAsync(PostProductReturnRequestDto request);

    }
}
