using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using QPService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBase.Pazaryeri.Domain.Entities;
using PazarYeriAktarim = OBase.Pazaryeri.Domain.Entities.PazarYeriAktarim;

namespace OBase.Pazaryeri.Business.Services.Abstract.Order
{
    public interface IOrderConvertService 
    {
        DeliveryAction ToQpOrder<TOrder>(
         TOrder order,
         PazarYeriBirimTanim store,
         IEnumerable<PazarYeriMalTanim> products,
         IEnumerable<PazarYeriAktarim> transferProducts,
         long seqId,
         SachetProduct[] sachetProduct = null,
         string merchantNo = "")
         where TOrder : BaseOrderDto;
    }
}
