using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using QPService;
using static OBase.Pazaryeri.Domain.Constants.Constants;

using Item = QPService.Item;
using Payment = QPService.Payment;
using PazarYeriAktarim = OBase.Pazaryeri.Domain.Entities.PazarYeriAktarim;

namespace OBase.Pazaryeri.Business.Services.Concrete.Order.OrderConverter
{
    public class TrendyolGoOrderConverter : BaseOrderConverter<TrendyolGoOrderDto>
    {
        protected override string MerchantName => "TRYGO";
        protected override int ChannelId => SaleChannelId.TrendyolGO;
        protected override int PaymentMethodId => PazarYeriPaymentId.TrendyolGO;

        protected override string GetOrderNumber(TrendyolGoOrderDto order) => order.OrderNumber;

        protected override void SetCommonDeliveryActionProperties(DeliveryAction deliveryAction, TrendyolGoOrderDto order, long seqId, string merchantNo)
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
            deliveryAction.id = GetDeliveryActionId(order);
            deliveryAction.DiscountTotalYTL = GetDiscountTotalYTL(order);

            deliveryAction.DeliveryDate = UnixTimeStampToDateTime(order.EstimatedDeliveryStartDate, merchantNo).ToString("dd.MM.yyyy");
            deliveryAction.OrderDate = UnixTimeStampToDateTime(order.OrderDate, merchantNo).ToString("dd.MM.yyyy");
            deliveryAction.ClaimDate = UnixTimeStampToDateTime(order.OrderDate, merchantNo).ToString("dd.MM.yyyy");
            deliveryAction.OrderTime = UnixTimeStampToDateTime(order.OrderDate, merchantNo).ToString("HH:mm");
            deliveryAction.LastPickupTime = UnixTimeStampToDateTime(order.EstimatedDeliveryEndDate, merchantNo).AddHours(-1).ToString("HH:mm");
            deliveryAction.CustomerNote = "";

            deliveryAction.UpdateDeliveryAction = true;
            deliveryAction.CollectOrder = false;
        }
        protected override void SetAddressInfo(DeliveryAction deliveryAction, TrendyolGoOrderDto order)
        {

            deliveryAction.ToAddress = new Addres
            {
                Address = order.ShipmentAddress?.Address1 ?? "",
                Name = order.ShipmentAddress?.Address1 ?? "",
                City = new CityObj() { id = order.ShipmentAddress.CityCode, Value = order.ShipmentAddress.City },
                District = new DistrictObj() { id = order.ShipmentAddress.DistrictId, Value = order.ShipmentAddress.District },
                Tel1 = order.ShipmentAddress.Phone
            };


            deliveryAction.FromAddress = new Addres
            {
                Address = order.ShipmentAddress?.Address1 ?? "",
                Name = order.ShipmentAddress?.Address1 ?? "",
                City = new CityObj() { id = order.ShipmentAddress.CityCode, Value = order.ShipmentAddress.City },
                District = new DistrictObj() { id = order.ShipmentAddress.DistrictId, Value = order.ShipmentAddress.District }
            };

        }

        protected override void SetCustomerInfo(DeliveryAction deliveryAction, TrendyolGoOrderDto order)
        {

            deliveryAction.Customer = new QPService.Customer
            {
                CustomerId = order.Customer.Id,
                City = new CityObj { id = order.InvoiceAddress.CityCode, Value = order.InvoiceAddress.City },
                District = new DistrictObj { id = order.InvoiceAddress.DistrictId, Value = order.InvoiceAddress.District },
                Name = $"{order.Customer.FirstName} {order.Customer.LastName}",
                Email = order.Customer.Email,
                Tel1 = order.ShipmentAddress.Phone,
                CustomerType = 0
            };
        }

        protected override void SetDeliveryInfo(DeliveryAction deliveryAction, TrendyolGoOrderDto order)
        {
            base.SetDeliveryInfo(deliveryAction, order);
        }

        protected override string GetMarketplaceDeliveryModel(TrendyolGoOrderDto order)
        {
            return order.DeliveryModel == TyGoConstants.GoDelivery ?
                QpDeliveryModels.MarketPlace : QpDeliveryModels.Store;
        }

        protected override void SetInvoiceInfo(DeliveryAction deliveryAction, TrendyolGoOrderDto order)
        {
            order.InvoiceAddress = new Domain.Dtos.TrendyolGo.InvoiceAddress()
            {
                City = "Şehir Belirtilmemiş",
                CityCode = 01,
                Address1 = string.IsNullOrEmpty(order.InvoiceAddress?.Address1) ? "Adres Belirtilmemiş" : order.InvoiceAddress?.Address1,
                Address2 = string.IsNullOrEmpty(order.InvoiceAddress?.Address1) ? "Adres Belirtilmemiş" : order.InvoiceAddress?.Address1,
                AddressDescription = "Adres Belirtilmemiş",
                District = "İlçe Belirtilmemiş",
                DistrictId = 02,
                CountryCode = "TR",
                Phone = "5555555555"
            };

            if (order.InvoiceAddress != null)
            {
                order.InvoiceAddress.City = string.IsNullOrEmpty(order.InvoiceAddress.City) ? "Şehir Belirtilmemiş" : order.InvoiceAddress.City;
                order.InvoiceAddress.District = string.IsNullOrEmpty(order.InvoiceAddress.District) ? "İlçe Belirtilmemiş" : order.InvoiceAddress.District;
            }


            deliveryAction.CustomerInvoice = new CustomerInvoice
            {
                Address = order.InvoiceAddress.Address1,
                City = new CityObj() { id = order.InvoiceAddress.CityId, Value = order.InvoiceAddress.City },
                District = new DistrictObj() { id = order.InvoiceAddress.DistrictId, Value = order.ShipmentAddress.District },
                Name = $"{order.InvoiceAddress.FirstName} {order.InvoiceAddress.LastName}",
                TaxNo = (order.InvoiceAddress?.IdentityNumber ?? "").Length > 11 ? (order.InvoiceAddress?.IdentityNumber ?? "")[..11] : (order.InvoiceAddress?.IdentityNumber ?? ""),
                InvoiceTotalYTL = order.TotalPrice,
                PrepareInvoice = true
            };

        }

        protected override void SetShipmentInfo(DeliveryAction deliveryAction, TrendyolGoOrderDto order)
        {
            order.ShipmentAddress = new ShipmentAddress()
            {
                City = "Şehir Belirtilmemiş",
                CityCode = 01,
                Address1 = String.IsNullOrEmpty(order.InvoiceAddress?.Address1) ? "Adres Belirtilmemiş" : order.InvoiceAddress?.Address1,
                Address2 = String.IsNullOrEmpty(order.InvoiceAddress?.Address1) ? "Adres Belirtilmemiş" : order.InvoiceAddress?.Address1,
                AddressDescription = "Adres Belirtilmemiş",
                District = "İlçe Belirtilmemiş",
                DistrictId = 02,
                CountryCode = "TR",
                Phone = "5555555555"
            };
        }

        protected override void SetPaymentInfo(DeliveryAction deliveryAction, TrendyolGoOrderDto order)
        {
            deliveryAction.Payments = new[]
            {
                new Payment
                {
                    PaymentMethod = new PaymentMethod { id = PaymentMethodId, Value = $"ONLINE ODEME {MerchantName}" },
                    PaymentOption = new PaymentOption { id = PaymentMethodId, Value = $"ONLINE ODEME {MerchantName}" },
                    AmountYTL=decimal.Parse(order.TotalPrice.ToString()),
                    CaptureAmountYTL=decimal.Parse(order.TotalPrice.ToString()),
                    InstallmentCount=0,
                    id=PaymentMethodId,
                    PointsUsedYTL=0
                }
            };
        }

        protected override void SetItems(DeliveryAction deliveryAction, TrendyolGoOrderDto order, PazarYeriBirimTanim store, IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts, SachetProduct[] sachetProduct,long seqId)
        {
            var items = new List<Item>();
            foreach (var line in order.Lines)
            {
                var transferProduct = transferProducts.FirstOrDefault(x => x.PazarYeriMalNo == line.Barcode && x.BirimNo == store.BirimNo);
                var product = transferProduct != null ? products.FirstOrDefault(x => x.MalNo.Trim() == transferProduct.MalNo?.ToString().Trim()) : null;               
                if (product == null || store == null || product.MalNo == null)
                {
                    throw new OrderConversionException($"QP Error With Id {seqId} OrderId {order.OrderId} ProductBarcode {line.Barcode} --> Product Not Found!", null);
                }
                product.PyUrunSatisDeger = product.PyUrunSatisDeger ?? 0;
                var item = new Item
                {
                    id = long.Parse(line.Items.FirstOrDefault().Id),
                    ProductId = long.Parse(product.MalNo),
                    ProductModelId = 0,
                    ProductModelName = line.Product.Name,
                    ShopDefinedCode = product.MalNo,
                    Unit = !string.IsNullOrEmpty(product.PyUrunSatisBirim) && product.PyUrunSatisBirim.ToUpper() == CommonConstants.KG ? product.PyUrunSatisBirim.ToUpper() : CommonConstants.ADET,
                    ShopId = 1,
                    ShopName = "NONE",
                    StoreId = Convert.ToInt32(store?.BirimNo),
                    StoreName = store?.BirimAdi,
                    ProductName = $"{line.Product.BrandName} {line.Product.Name}",
                    Amount = product.PyUrunSatisDeger != 0 ? (float)(product.PyUrunSatisDeger * (line.Items.Count(x => !x.IsAlternative && !x.IsCancelled))) : line.Items.Count(x => !x.IsAlternative && !x.IsCancelled),
                    VAT = (byte)line.VatRatio,
                    PriceYTL = CalculatePrice(line, product, transferProduct),
                    ShopDefinedStoreCode = store?.BirimNo,
                    AlternativeProductRequested = "NO",
                    DM3=0,
                    GiftAmount = 0,
                    ReyonOrder=0
                };

                items.Add(item);
            }

            if (order.TotalCargo > 0 && !string.IsNullOrEmpty(order.CargoProductCode))
            {
                items.Add(CreateShippingFeeItem(order, store));
            }

            if (sachetProduct != null)
            {
                var sachetItems = CreateSachetItems(sachetProduct, store);
                if (sachetItems.Any())
                    items.AddRange(sachetItems);
            }

            deliveryAction.Items = items.ToArray();
        }
        protected override string GetUserCode(TrendyolGoOrderDto order, long seqId)=> order.Customer.Id;

        protected override string GetUserPassword()=>string.Empty;

        protected override decimal GetDeliveryActionId(TrendyolGoOrderDto order)
        {
            return decimal.Parse(order.Id);
        }

        protected override decimal GetDiscountTotalYTL(TrendyolGoOrderDto order)
        {
            return Convert.ToDecimal(order.TotalDiscount);
        }

        private decimal CalculatePrice(Line line, PazarYeriMalTanim product, PazarYeriAktarim transferProduct)
        {
            decimal priceytl = 0;
            if (product?.PyUrunSatisBirim == null) { priceytl = line.Price; }
            else if (product.PyUrunSatisBirim.ToUpper() == CommonConstants.KG)
            {
                priceytl = (transferProduct?.IndirimliSatisFiyat > 0 ? transferProduct?.IndirimliSatisFiyat : transferProduct?.SatisFiyat) ?? 0;
            }

            priceytl = priceytl == 0 ? line.Price : priceytl;
            return priceytl;
        }

        private Item CreateShippingFeeItem(TrendyolGoOrderDto order, PazarYeriBirimTanim store)
        {
            return new Item() {

                id = int.Parse(order.CargoProductCode),
                ShopId = 1,
                ShopName = "NONE",
                StoreId = Convert.ToInt32(store?.BirimNo),
                StoreName = store?.BirimAdi,
                ShopDefinedStoreCode = store?.BirimNo,
                ProductId = int.Parse(order.CargoProductCode),
                Amount = 1,
                ProductModelId = 0,
                PriceYTL = order.TotalCargo ?? 0,
                ProductModelName = "Nakliye Bedeli",
                ProductName = "Nakliye Bedeli",
                ShopDefinedCode = order.CargoProductCode,
                Packable = false,
                VAT = 0,
                DM3 = 0,
                GiftAmount = 0,
                Unit = CommonConstants.ADET,
                AlternativeProductRequested = "NO",
                ReyonOrder = 0

            };
        }

        private List<Item> CreateSachetItems(SachetProduct[] sachetProduct, PazarYeriBirimTanim store)
        {
            List<Item> sachetItem = new List<Item>();
            foreach (var sachet in sachetProduct)
            {
                if (!string.IsNullOrEmpty(sachet.ProductCode))
                {
                    sachetItem.Add(new Item
                    {
                        id = Convert.ToInt64(sachet.ProductCode),
                        ShopId = 1,
                        ShopName = "NONE",
                        StoreId = Convert.ToInt32(store?.BirimNo),
                        StoreName = store?.BirimAdi,
                        ShopDefinedStoreCode = store?.BirimNo,
                        PriceYTL = sachet.Price,
                        ProductModelName = "ALISVERIS POSETI PLASTIK..",
                        ProductName = "ALISVERIS POSETI PLASTIK..",
                        ShopDefinedCode = sachet.ProductCode,
                        Packable = false,
                        VAT = 0,
                        DM3 = 0,
                        GiftAmount = 0,
                        Unit = CommonConstants.ADET,
                        AlternativeProductRequested = "NO",
                        ReyonOrder = 0,
                        ProductId = Convert.ToInt64(sachet.ProductCode),
                        Amount = 1,
                        ProductModelId = 0

                    });
                }
            }
            return sachetItem;
        }
    }
}