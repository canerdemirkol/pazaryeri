using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Business.Services.Abstract.Sale
{
    public interface ISaleService
    {
        Task<ServiceResponse<SaleInfoResponseDto>> SaveSaleInfo(SaleInfoDto orderDto);
    }
}
