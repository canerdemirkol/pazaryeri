using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Business.Services.Abstract.Order
{
    public interface IYemekSepetiOrderService: IOrderService
    {
        Task<ServiceResponse<CommonResponseDto>> SaveOrderOnQp(YemekSepetiOrderDto orderDto);
    }
}
