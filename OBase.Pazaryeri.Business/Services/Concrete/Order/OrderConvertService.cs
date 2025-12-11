using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Factories;
using OBase.Pazaryeri.Business.Helper;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using OBase.Pazaryeri.Domain.Entities;
using Polly;
using Polly.Retry;
using QPService;
using System.Net;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Constants.Constants.YemekSepetiConstants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using static OBase.Pazaryeri.Domain.Helper.CommonHelper;

namespace OBase.Pazaryeri.Business.Services.Concrete.Order
{
    public class OrderConvertService : IOrderConvertService
    {
        #region Variables

        private readonly IOrderConverterFactory _converterFactory;

        #endregion

        #region Ctor

        public OrderConvertService(IOrderConverterFactory converterFactory)
        {
            _converterFactory = converterFactory;
        }

        #endregion

        #region Methods       
        public DeliveryAction ToQpOrder<TOrder>(
           TOrder order,
           PazarYeriBirimTanim store,
           IEnumerable<PazarYeriMalTanim> products,
           IEnumerable<PazarYeriAktarim> transferProducts,
           long seqId,
           SachetProduct[] sachetProduct = null,
           string merchantNo = "")
           where TOrder : BaseOrderDto
        {
            var converter = _converterFactory.GetConverter<TOrder>();
            return converter.ConvertToQpOrder(order, store, products, transferProducts, seqId, sachetProduct, merchantNo);
        }
        #endregion
    }
}