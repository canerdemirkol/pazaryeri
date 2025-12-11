using Hangfire.Server;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Helper;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
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

using Item = QPService.Item;
using Payment = QPService.Payment;
using PazarYeriAktarim = OBase.Pazaryeri.Domain.Entities.PazarYeriAktarim;

namespace OBase.Pazaryeri.Business.Services.Concrete.Order.OrderConverter
{
    public class HepsiExpressOrderConverter : BaseOrderConverter<HEOrderDto>
    {
        protected override string MerchantName => "HEPSIEXPRESS";
        protected override int ChannelId => SaleChannelId.HepsiExpress;
        protected override int PaymentMethodId => PazarYeriPaymentId.HepsiExpress;

        protected override decimal GetDeliveryActionId(HEOrderDto order)
        {
            var firstOrderItem = order.Items.FirstOrDefault();
            return decimal.Parse(firstOrderItem.OrderNumber);
        }

        protected override decimal GetDiscountTotalYTL(HEOrderDto order) => Convert.ToDecimal(order.Items.Sum(x => x.HbDiscount.TotalPrice.Amount));

        protected override string GetMarketplaceDeliveryModel(HEOrderDto order) => string.Empty;

        protected override string GetOrderNumber(HEOrderDto order)
        {
            var firstOrderItem = order.Items.FirstOrDefault();
            return firstOrderItem.Id.ToString();
        }

        protected override string GetUserCode(HEOrderDto order, long seqId) => seqId.ToString();

        protected override string GetUserPassword() => string.Empty;

        protected override void SetAddressInfo(DeliveryAction deliveryAction, HEOrderDto order)
        {
            var firstOrderItem = order.Items.FirstOrDefault();

            deliveryAction.ToAddress = new Addres
            {
                Address = firstOrderItem.ShippingAddress?.AddressAddress ?? "",
                Name = firstOrderItem.ShippingAddress?.AddressAddress ?? "",
                City = new CityObj() { id = 01, Value = firstOrderItem.ShippingAddress?.City },
                District = new DistrictObj() { id = 02, Value = firstOrderItem.ShippingAddress?.District },
                Tel1 = firstOrderItem.ShippingAddress?.PhoneNumber,
            };


            deliveryAction.FromAddress = new Addres
            {
                Name = firstOrderItem.ShippingAddress?.AddressAddress,
                Address = firstOrderItem.ShippingAddress?.AddressAddress,
                City = new CityObj() { id = 02, Value = firstOrderItem.ShippingAddress?.City },
                District = new DistrictObj() { id = 02, Value = firstOrderItem.ShippingAddress?.District },
            };
        }

        protected override void SetDeliveryInfo(DeliveryAction deliveryAction, HEOrderDto order)
        {
            base.SetDeliveryInfo(deliveryAction, order);

        }
        protected override void SetCommonDeliveryActionProperties(DeliveryAction deliveryAction, HEOrderDto order, long seqId, string merchantNo = "")
        {
            var firstOrderItem = order.Items.FirstOrDefault();

            deliveryAction.DeliveryTime = "00.00 - 23.59";
            deliveryAction.OrderPriority = true;
            deliveryAction.MaxCapturePercentage = 0;
            deliveryAction.SerivePricePayer = "TP";
            deliveryAction.ShippingServiceName = "";
            deliveryAction.MinDeliveryTotalPriceYTL = decimal.Zero;
            deliveryAction.ServicePriceYTL = decimal.Zero;
            deliveryAction.DeliveryTimeGroup = 2359;
            deliveryAction.PayerCompanyCode = "0";


            deliveryAction.UserCode = GetUserCode(order, seqId);
            deliveryAction.UserPwd = GetUserPassword();
            deliveryAction.OrderId = seqId;
            deliveryAction.id = GetDeliveryActionId(order);
            deliveryAction.DiscountTotalYTL = GetDiscountTotalYTL(order);


            deliveryAction.UpdateDeliveryAction = true;
            deliveryAction.CollectOrder = false;
            deliveryAction.CustomerNote = string.Empty;
            deliveryAction.CrmMessage = string.Empty;


            deliveryAction.OrderTime = UnixTimeStampToDateTime(firstOrderItem.DueDate.Ticks, merchantNo).ToString("HH:mm");
            deliveryAction.OrderDate = UnixTimeStampToDateTime(firstOrderItem.DueDate.Ticks, merchantNo).ToString("dd.MM.yyyy");
            deliveryAction.LastPickupTime = UnixTimeStampToDateTime(firstOrderItem.DueDate.Ticks, merchantNo).AddHours(-1).ToString("HH:mm");
            deliveryAction.ClaimDate = UnixTimeStampToDateTime(firstOrderItem.DueDate.Ticks, merchantNo).ToString("dd.MM.yyyy");
            deliveryAction.DeliveryDate = UnixTimeStampToDateTime(firstOrderItem.DueDate.Ticks, merchantNo).ToString("dd.MM.yyyy");
        }


        protected override void SetPaymentInfo(DeliveryAction deliveryAction, HEOrderDto order)
        {
            var firstOrderItem = order.Items.FirstOrDefault();

            deliveryAction.Payments = new[]
            {
                new Payment
                {
                    PaymentMethod = new PaymentMethod { id = PaymentMethodId, Value = $"ONLINE ODEME {MerchantName}" },
                    PaymentOption = new PaymentOption { id = PaymentMethodId, Value = $"ONLINE ODEME {MerchantName}" },
                    AmountYTL=(decimal)firstOrderItem.TotalPrice.Amount,
                    CaptureAmountYTL=(decimal)firstOrderItem.TotalPrice.Amount,
                    InstallmentCount=0,
                    id=PaymentMethodId,
                    PointsUsedYTL=0
                }
            };
        }

        protected override void SetCustomerInfo(DeliveryAction deliveryAction, HEOrderDto order)
        {

            var firstOrderItem = order.Items.FirstOrDefault();
            string customerId = firstOrderItem.CustomerId.ToString();
            deliveryAction.Customer = new QPService.Customer
            {
                CustomerId = customerId,
                City = new CityObj { id = 02, Value = firstOrderItem.Invoice.Address.City },
                District = new DistrictObj { id = 01, Value = firstOrderItem.Invoice.Address.District },
                CustomerType = 0,
                Name = firstOrderItem.CustomerName,
                Email = firstOrderItem.Invoice.Address.Email,
                Tel1 = firstOrderItem.Invoice.Address.PhoneNumber
            };
        }

        protected override void SetInvoiceInfo(DeliveryAction deliveryAction, HEOrderDto order)
        {
            var firstOrderItem = order.Items.FirstOrDefault();
            firstOrderItem.Invoice.Address = firstOrderItem.Invoice.Address ?? new Address()
            {
                City = String.IsNullOrEmpty(firstOrderItem.Invoice.Address?.City) ? "Şehir Belirtilmemiş" : firstOrderItem.Invoice.Address?.City,
                AddressAddress = String.IsNullOrEmpty(firstOrderItem.Invoice.Address?.AddressAddress) ? "Adres Belirtilmemiş" : firstOrderItem.Invoice.Address?.AddressAddress,
                Town = String.IsNullOrEmpty(firstOrderItem.Invoice.Address?.Town) ? "İlçe Belirtilmemiş" : firstOrderItem.Invoice.Address?.Town,
                District = String.IsNullOrEmpty(firstOrderItem.Invoice.Address?.District) ? "Semt Belirtilmemiş" : firstOrderItem.Invoice.Address?.District,
                CountryCode = "TR",
            }; 

            deliveryAction.CustomerInvoice = new CustomerInvoice
            {
                Address = firstOrderItem.Invoice.Address.AddressAddress,
                City = new CityObj() { id = 01, Value = firstOrderItem.Invoice.Address.City },
                District = new DistrictObj() { id = 02, Value = firstOrderItem.ShippingAddress?.District },
                Name = firstOrderItem.Invoice.Address.Name,
                TaxNo = string.Empty,
                InvoiceTotalYTL = (decimal)Convert.ToDouble(order.Items.Sum(x => x.TotalPrice.Amount) - order.Items.Sum(x => x.HbDiscount.TotalPrice.Amount)),
                PrepareInvoice = true
            };
        }

        protected override void SetItems(DeliveryAction deliveryAction, HEOrderDto order, PazarYeriBirimTanim store, IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts, SachetProduct[] sachetProduct, long seqId)
        {
            var items = new List<Item>();
            int i = 0;
            foreach (var line in order.Items)
            {
                var newId = seqId + i;
                var transferProduct = transferProducts.FirstOrDefault(x => x.PazarYeriMalNo == line.MerchantSku && x.BirimNo == store.BirimNo);
                var product = transferProduct != null ? products.FirstOrDefault(x => x.MalNo.Trim() == transferProduct.MalNo?.ToString().Trim()) : null;

                if (product == null || store == null || product.MalNo == null)
                {
                    throw new OrderConversionException($"QP Error With Id {seqId} OrderId {order.Order.Id} ProductBarcode {line.Sku} --> Product Not Found!", null);
                }

                var item = new Item
                {
                    id = long.Parse(newId.ToString()),
                    ProductId = long.Parse(product?.MalNo),
                    ProductModelId = 0,
                    ProductModelName = line.Name,
                    ShopDefinedCode = product?.MalNo,
                    Unit = !string.IsNullOrEmpty(product.PyUrunSatisBirim) && product.PyUrunSatisBirim.ToUpper() == CommonConstants.KG ? product.PyUrunSatisBirim.ToUpper() : CommonConstants.ADET,
                    ShopId = 1,
                    ShopName = "NONE",
                    StoreId = Convert.ToInt32(store?.BirimNo),
                    StoreName = store?.BirimAdi,
                    ProductName = $"{line.Name}",
                    Amount = product.PyUrunSatisDeger != 0 ? (float)(product.PyUrunSatisDeger * line.Quantity) : line.Quantity,
                    VAT = (byte)line.VatRate,
                    PriceYTL = CalculatePrice(line, product, transferProduct),
                    ShopDefinedStoreCode = store?.BirimNo,
                    AlternativeProductRequested = "NO",
                    DM3 = 0,
                    GiftAmount = 0,
                    ReyonOrder = 0
                };

                items.Add(item);
                i++;
            }

           

            deliveryAction.Items = items.ToArray();
        }

        protected override void SetShipmentInfo(DeliveryAction deliveryAction, HEOrderDto order)
        {
            var firstOrderItem = order.Items.FirstOrDefault();
            firstOrderItem.ShippingAddress = firstOrderItem.ShippingAddress ?? new Address()
            {
                City = String.IsNullOrEmpty(firstOrderItem.Invoice.Address?.City) ? "Şehir Belirtilmemiş" : firstOrderItem.Invoice.Address?.City,
                AddressAddress = String.IsNullOrEmpty(firstOrderItem.Invoice.Address?.AddressAddress) ? "Adres Belirtilmemiş" : firstOrderItem.Invoice.Address?.AddressAddress,
                Town = String.IsNullOrEmpty(firstOrderItem.Invoice.Address?.Town) ? "İlçe Belirtilmemiş" : firstOrderItem.Invoice.Address?.Town,
                District = String.IsNullOrEmpty(firstOrderItem.Invoice.Address?.District) ? "Semt Belirtilmemiş" : firstOrderItem.Invoice.Address?.District,
                CountryCode = "TR",
            };
        }

        private List<Item> CreateSachetItems(SachetProduct sachetProduct, PazarYeriBirimTanim store)
        {
            List<Item> sachetItem = new List<Item>();

            return sachetItem;
        }

        private decimal CalculatePrice(HEItem item, PazarYeriMalTanim product, PazarYeriAktarim transferProduct)
        {
            decimal priceytl = transferProduct?.IndirimliSatisFiyat > 0 ? (decimal)transferProduct?.IndirimliSatisFiyat : (decimal)transferProduct?.SatisFiyat;
            if (product?.PyUrunSatisBirim == "KG")
            {
                priceytl = priceytl * (decimal)(1 / product.PyUrunSatisDeger);
            }
            priceytl = priceytl == 0 ? (decimal)item.TotalPrice.Amount : priceytl;
            return priceytl;
        }

    }
}