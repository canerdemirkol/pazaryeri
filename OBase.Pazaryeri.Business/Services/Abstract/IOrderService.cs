using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.Business.Services.Abstract
{
    public interface IOrderService
    {
        Task<CommonResponseDto> OrderUpdatePackageStatus(OrderStatuUpdateRequestDto orderDto, PazarYeriSiparis orderEntity);
        Task<CommonResponseDto> ClaimStatuUpdate(PostProductReturnRequestDto claimDto);
    }
}