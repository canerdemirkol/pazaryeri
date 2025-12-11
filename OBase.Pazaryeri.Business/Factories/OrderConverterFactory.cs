using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Helper;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Business.Services.Concrete.Order.OrderConverter;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.Idefix;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;

namespace OBase.Pazaryeri.Business.Factories
{
    public class OrderConverterFactory: IOrderConverterFactory
    {
        private readonly Dictionary<Type, object> _converters;

        public OrderConverterFactory()
        {
            _converters = new Dictionary<Type, object>
            {
                { typeof(TrendyolGoOrderDto), new TrendyolGoOrderConverter() },
                { typeof(GetirOrderDto), new GetirOrderConverter() },
                { typeof(YemekSepetiOrderDto), new YemekSepetiOrderConverter() },
                { typeof(HEOrderDto), new HepsiExpressOrderConverter() },
                { typeof(IdefixOrderDto), new IdefixOrderConverter() }
            };
        }

        public IOrderConverter<TOrder> GetConverter<TOrder>() where TOrder : BaseOrderDto
        {
            if (_converters.TryGetValue(typeof(TOrder), out var converter))
            {
                return (IOrderConverter<TOrder>)converter;
            }
            throw new NotSupportedException($"No converter registered for type {typeof(TOrder).Name}");
        }
    }
}