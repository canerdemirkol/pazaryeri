using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using OBase.Pazaryeri.Domain.Entities;
using QPService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Business.Services.Abstract
{
    public interface IOrderConverter<TOrder> where TOrder : BaseOrderDto
    {
        DeliveryAction ConvertToQpOrder(TOrder order, PazarYeriBirimTanim store,
            IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts,
            long seqId, SachetProduct[] sachetProduct = null, string merchantNo = "");
    }
}
