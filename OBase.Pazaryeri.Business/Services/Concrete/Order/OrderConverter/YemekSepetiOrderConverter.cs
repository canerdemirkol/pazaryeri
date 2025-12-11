
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using OBase.Pazaryeri.Domain.Entities;
using QPService;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;

using Item = QPService.Item;
using Payment = QPService.Payment;
using PazarYeriAktarim = OBase.Pazaryeri.Domain.Entities.PazarYeriAktarim;

namespace OBase.Pazaryeri.Business.Services.Concrete.Order.OrderConverter
{
    public class YemekSepetiOrderConverter : BaseOrderConverter<YemekSepetiOrderDto>
    {
        protected override string MerchantName => "Yemek Sepeti";
        protected override int ChannelId => SaleChannelId.YemekSepeti;
        protected override int PaymentMethodId => PazarYeriPaymentId.YemekSepeti;

        protected override decimal GetDeliveryActionId(YemekSepetiOrderDto order) => 0m;

        protected override decimal GetDiscountTotalYTL(YemekSepetiOrderDto order) =>  0m;
        protected override string GetMarketplaceDeliveryModel(YemekSepetiOrderDto order)
        {
            return order.TransportType == YemekSepetiConstants.OrderTransportType.OD ?
              QpDeliveryModels.MarketPlace : QpDeliveryModels.Store;
        }

        protected override string GetOrderNumber(YemekSepetiOrderDto order) => order.ExternalOrderId;
        protected override string GetUserCode(YemekSepetiOrderDto order, long seqId)=>order.Customer.Id;
        protected override string GetUserPassword()=> string.Empty;

        protected override void SetAddressInfo(DeliveryAction deliveryAction, YemekSepetiOrderDto order)
        {
            var deliveryAddress = order.Customer.DeliveryAddress;
            StringBuilder fullAddress = new();
            if (!string.IsNullOrEmpty(deliveryAddress.Street))
            {
                fullAddress.Append($"Street: {deliveryAddress.Street}");
            }
            if (!string.IsNullOrEmpty(deliveryAddress.Number))
            {
                fullAddress.Append($" No: {deliveryAddress.Number}");
            }
            if (!string.IsNullOrEmpty(deliveryAddress.Entrance))
            {
                fullAddress.Append($" Entrance: {deliveryAddress.Entrance}");
            }
            if (!string.IsNullOrEmpty(deliveryAddress.Floor))
            {
                fullAddress.Append($" Floor:  {deliveryAddress.Floor}");
            }
            if (!string.IsNullOrEmpty(deliveryAddress.Building))
            {
                fullAddress.Append($" Building:  {deliveryAddress.Building}");
            }
            if (!string.IsNullOrEmpty(deliveryAddress.Apartment))
            {
                fullAddress.Append($" Apartment:  {deliveryAddress.Apartment}");
            }
            if (!string.IsNullOrEmpty(deliveryAddress.Intercom))
            {
                fullAddress.Append($" Intercom:  {deliveryAddress.Intercom}");
            }
            if (!string.IsNullOrEmpty(deliveryAddress.Suburb))
            {
                fullAddress.Append($" Suburb:  {deliveryAddress.Suburb}");
            }
            if (!string.IsNullOrEmpty(deliveryAddress.Block))
            {
                fullAddress.Append($" Block:  {deliveryAddress.Block}");
            }


            deliveryAction.ToAddress = new Addres
            {
                Address = fullAddress.ToString(),
                Name = string.Empty,
                City = new CityObj() { id = 1, Value = order.Customer?.DeliveryAddress.City },
                District = new DistrictObj() { id = 1, Value = order.Customer?.DeliveryAddress.Street },
                Tel1 = order.Customer.PhoneNumber
            };
            deliveryAction.FromAddress = new Addres
            {
                Name = string.Empty,
                Address = fullAddress.ToString(),
                City = new CityObj() { id = 1, Value = order.Customer?.DeliveryAddress?.City },
                District = new DistrictObj() { id = 1, Value = order.Customer?.DeliveryAddress.Street }
            };

        }

        protected override void SetCommonDeliveryActionProperties(DeliveryAction deliveryAction, YemekSepetiOrderDto order, long seqId, string merchantNo = "")
        {
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
            deliveryAction.id = seqId;
            deliveryAction.DiscountTotalYTL = GetDiscountTotalYTL(order);

            deliveryAction.DeliveryDate = order.Sys.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy");
            deliveryAction.OrderDate = order.Sys.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy");
            deliveryAction.OrderTime = order.Sys.CreatedAt.ToLocalTime().ToString("HH:mm");
            deliveryAction.CustomerNote = string.Empty;

            deliveryAction.UpdateDeliveryAction = true;
            deliveryAction.CollectOrder = false;
            deliveryAction.CrmMessage = "";
        }

        protected override void SetCustomerInfo(DeliveryAction deliveryAction, YemekSepetiOrderDto order)
        {
            deliveryAction.Customer = new QPService.Customer
            {
                CustomerId = order.Customer.Id,
                City = new CityObj { id = 1, Value = order.Customer?.DeliveryAddress?.City },
                District = new DistrictObj { id = 1, Value = order.Customer?.DeliveryAddress?.Street },
                Name = order.Customer.FirstName,
                Email = string.Empty,
                Tel1 = order.Customer.PhoneNumber,
                Tel2 = string.Empty
            };
        }
        protected override void SetDeliveryInfo(DeliveryAction deliveryAction, YemekSepetiOrderDto order)
        {
            base.SetDeliveryInfo(deliveryAction, order);

            order.Customer.DeliveryAddress = new DeliveryAddress()
            {
                FormattedAddress = !string.IsNullOrEmpty(order.Customer?.DeliveryAddress.FormattedAddress ?? "") ? order.Customer?.DeliveryAddress.FormattedAddress : "Adres Belirtilmemiş",
                Number = !string.IsNullOrEmpty(order.Customer?.DeliveryAddress.Number ?? "") ? order.Customer?.DeliveryAddress.Number : "Kapı No Belirtilmemiş",
                City = !string.IsNullOrEmpty(order.Customer?.DeliveryAddress.City ?? "") ? order.Customer?.DeliveryAddress.City : "Şehir Belirtilmemiş",
                Apartment = !string.IsNullOrEmpty(order.Customer?.DeliveryAddress.Apartment ?? "") ? order.Customer?.DeliveryAddress.Apartment : "Apt No  Belirtilmemiş",
                Floor = !string.IsNullOrEmpty(order.Customer?.DeliveryAddress.Floor ?? "") ? order.Customer?.DeliveryAddress.Floor : "Kat Belirtilmemiş",
                Street = !string.IsNullOrEmpty(order.Customer?.DeliveryAddress.Street ?? "") ? order.Customer?.DeliveryAddress.Street : "Cadde Belirtilmemiş",
            };
        }
        protected override void SetInvoiceInfo(DeliveryAction deliveryAction, YemekSepetiOrderDto order)
        {
            deliveryAction.CustomerInvoice = new CustomerInvoice
            {
                Address = string.Empty,
                City = new CityObj() { id = 1, Value = order.Customer?.DeliveryAddress?.City },
                District = new DistrictObj() { id = 1, Value = order.Customer?.DeliveryAddress.Street },
                Name = order.Customer.FirstName,
                TaxNo = string.Empty,
                InvoiceTotalYTL = Convert.ToDecimal(order.Payment.OrderTotal),
                PrepareInvoice = true
            };
        }

        protected override void SetItems(DeliveryAction deliveryAction, YemekSepetiOrderDto order, PazarYeriBirimTanim store, IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts, SachetProduct[] sachetProduct, long seqId)
        {

            var items = new List<Item>();
            var bagInfo = Array.Find(order.Items, a => a.Sku == sachetProduct[0].ProductCode);

            if (bagInfo is not null)
            {
                items.Add( new Item
                {
                    id = int.Parse(bagInfo.Sku),
                    ShopId = 1,
                    ShopName = "NONE",
                    StoreId = Convert.ToInt32(store?.BirimNo),
                    StoreName = store?.BirimAdi,
                    ShopDefinedStoreCode = store?.BirimNo,
                    PriceYTL = bagInfo.OriginalPricing.UnitPrice,
                    ProductModelName = bagInfo.Name,
                    ProductName = bagInfo.Name,
                    ShopDefinedCode = bagInfo.Sku,
                    Packable = false,
                    VAT = 0,
                    DM3 = 0,
                    GiftAmount = 0,
                    Unit = CommonConstants.ADET,
                    AlternativeProductRequested = "NO",
                    ReyonOrder = 0,
                    ProductId = int.Parse(bagInfo.Sku),
                    Amount = bagInfo.OriginalPricing.Quantity,
                    ProductModelId = 0,
                    MinAmount = bagInfo.OriginalPricing?.MinQuantity,
                    MaxAmount = bagInfo.OriginalPricing?.MaxQuantity
                });
            }

            foreach (var line in order.Items.Where(w => w.Sku != sachetProduct[0].ProductCode))
            {
                PazarYeriAktarim transferProduct = null;
                transferProduct = transferProducts.FirstOrDefault(x => line.Sku == x.PazarYeriMalNo && x.BirimNo == store.BirimNo);

                var product = transferProduct != null ? products.FirstOrDefault(x => x.MalNo.Trim() == transferProduct.MalNo?.Trim()) : null;
                if (product == null || store == null || product.MalNo == null)
                {
                    throw new OrderConversionException($"QP Error With Id {seqId} OrderId {order.OrderId} ProductBarcode {string.Join(',', line.Barcode)} --> Product Not Found!", null);                    
                }

                if (items.Exists(x => x.ProductId == long.Parse(product?.MalNo)))
                {
                    var itemToUpdate = items.FirstOrDefault(x => x.ProductId == long.Parse(product?.MalNo));
                    if (itemToUpdate is not null)
                        itemToUpdate.Amount += line.OriginalPricing.PricingType == GetirConstants.ProductType.Gr ? (line.OriginalPricing.Weight ?? 0) : line.OriginalPricing.Quantity;
                }
                else
                {
                    var xitem = new Item
                    {
                        ProductId = long.Parse(product?.MalNo)
                    };
                    xitem.id = xitem.ProductId;
                    xitem.ProductModelId = 0;
                    xitem.ProductModelName = line.Name;
                    xitem.ShopDefinedCode = product?.MalNo;
                    xitem.Unit =!string.IsNullOrEmpty(product.PyUrunSatisBirim) && product.PyUrunSatisBirim.ToUpper() == CommonConstants.KG ? product.PyUrunSatisBirim.ToUpper() : CommonConstants.ADET;
                    xitem.ShopId = 1;
                    xitem.ShopName = "NONE";
                    xitem.StoreId = Convert.ToInt32(store?.BirimNo);
                    xitem.StoreName = store?.BirimAdi;
                    xitem.ProductName = line.Name;
                    xitem.Amount = line.OriginalPricing.PricingType == YemekSepetiConstants.ProductType.KG ? (line.OriginalPricing.Weight ?? 0) : line.OriginalPricing.Quantity;
                    xitem.VAT = (byte)line.OriginalPricing.VatPercent;
                    xitem.DM3 = 0;
                    xitem.GiftAmount = 0;
                    xitem.PriceYTL = transferProduct.QpFiyat ?? line.OriginalPricing.UnitPrice;
                    xitem.ShopDefinedStoreCode = store?.BirimNo;
                    xitem.AlternativeProductRequested = "NO";
                    xitem.ReyonOrder = 0;
                    xitem.MinAmount = line.OriginalPricing?.MinQuantity;
                    xitem.MaxAmount = line.OriginalPricing?.MaxQuantity;
                    items.Add(xitem);
                }
            }

            deliveryAction.Items = items.ToArray();
        }

        protected override void SetShipmentInfo(DeliveryAction deliveryAction, YemekSepetiOrderDto order){ }

        protected override void SetPaymentInfo(DeliveryAction deliveryAction, YemekSepetiOrderDto order)
        {
            deliveryAction.Payments = new[]
            {
                new Payment
                {
                    PaymentMethod = new PaymentMethod { id = PaymentMethodId, Value = $"ONLINE ODEME {MerchantName}" },
                    PaymentOption = new PaymentOption { id = PaymentMethodId, Value = $"ONLINE ODEME {MerchantName}" },
                    AmountYTL=decimal.Parse(order.Payment.OrderTotal.ToString()),
                    CaptureAmountYTL=decimal.Parse(order.Payment.OrderTotal.ToString()),
                    InstallmentCount=0,
                    id=PaymentMethodId,
                    PointsUsedYTL=0
                }
            };
        }

        private List<Item> CreateSachetItems(SachetProduct sachetProduct, PazarYeriBirimTanim store)
        {
            List<Item> sachetItem = new List<Item>();
            
            return sachetItem;
        }
    }
}