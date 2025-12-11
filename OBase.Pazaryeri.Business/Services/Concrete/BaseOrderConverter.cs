using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using QPService;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Services.Concrete
{
    public abstract class BaseOrderConverter<TOrder> : IOrderConverter<TOrder> where TOrder : BaseOrderDto
    {
        protected abstract string MerchantName { get; }
        protected abstract int ChannelId { get; }
        protected abstract int PaymentMethodId { get; }

        public DeliveryAction ConvertToQpOrder(TOrder order, PazarYeriBirimTanim store,
            IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts,
            long seqId, SachetProduct[] sachetProduct = null, string merchantNo = "")
        {
            try
            {
                var deliveryAction = InitializeDeliveryAction(order, store, seqId);
                SetCommonDeliveryActionProperties(deliveryAction, order, seqId, merchantNo);
                SetDeliveryInfo(deliveryAction, order);
                SetShipmentInfo(deliveryAction, order);
                SetInvoiceInfo(deliveryAction, order);              
                
                SetCustomerInfo(deliveryAction, order);
                SetAddressInfo(deliveryAction, order);
                SetPaymentInfo(deliveryAction, order);
                SetItems(deliveryAction, order, store, products, transferProducts, sachetProduct, seqId);

                return deliveryAction;
            }
            catch (Exception ex)
            {
                throw new OrderConversionException($"Error converting {typeof(TOrder).Name} to QP Order", ex);
            }
        }

        protected virtual DeliveryAction InitializeDeliveryAction(TOrder order, PazarYeriBirimTanim store, long seqId)
        {
            return new DeliveryAction
            {
                SaleChannel = new SaleChannelObj
                {
                    ChannelId = ChannelId,
                    ChannelName = MerchantName,
                    ChannelOrderId = GetOrderNumber(order)
                },
                OrderId = seqId
            };
        }

        protected abstract string GetOrderNumber(TOrder order);

        protected abstract void SetCustomerInfo(DeliveryAction deliveryAction, TOrder order);

        protected abstract void SetAddressInfo(DeliveryAction deliveryAction, TOrder order);

        protected abstract void SetInvoiceInfo(DeliveryAction deliveryAction, TOrder order);

        protected abstract void SetShipmentInfo(DeliveryAction deliveryAction, TOrder order);

        protected virtual void SetDeliveryInfo(DeliveryAction deliveryAction, TOrder order)
        {            
            deliveryAction.DeliveryModel = new DeliveryModel
            {
                DeliveryType = "HOME_DELIVERY",
                DeliveryModelId = 2,
                DeliveryModelName = "HOME_DELIVERY",
                MarketplaceDeliveryModel = GetMarketplaceDeliveryModel(order)
            };
        }

        protected abstract void SetItems(DeliveryAction deliveryAction, TOrder order,
            PazarYeriBirimTanim store, IEnumerable<PazarYeriMalTanim> products,
            IEnumerable<PazarYeriAktarim> transferProducts, SachetProduct[] sachetProduct, long seqId);

        protected abstract void SetPaymentInfo(DeliveryAction deliveryAction, TOrder order);

        protected abstract string GetMarketplaceDeliveryModel(TOrder order);

        protected abstract void SetCommonDeliveryActionProperties(DeliveryAction deliveryAction, TOrder order, long seqId, string merchantNo = "");

        protected abstract string GetUserCode(TOrder order, long seqId);
        protected abstract string GetUserPassword();
        protected abstract decimal GetDeliveryActionId(TOrder order);
        protected abstract decimal GetDiscountTotalYTL(TOrder order);

        protected DateTime UnixTimeStampToDateTime(long unixTimeStamp, string merchantNo = "")
        {
            long dateTime = unixTimeStamp;
            if (dateTime <= 1000000000000L) dateTime *= 1000;

            return merchantNo switch
            {
                PazarYeri.HepsiExpress => new DateTime(dateTime).ToLocalTime(),
                PazarYeri.TrendyolGo => (new DateTime(1970, 1, 1)).AddMilliseconds(Convert.ToDouble(dateTime)).ToLocalTime(),
                _ => new DateTime(dateTime).ToLocalTime()
            };
        }
    }

    public class OrderConversionException : Exception
    {
        public OrderConversionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}