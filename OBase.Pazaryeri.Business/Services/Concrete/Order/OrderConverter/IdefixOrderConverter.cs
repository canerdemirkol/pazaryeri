using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.Idefix;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using QPService;
using static OBase.Pazaryeri.Domain.Constants.Constants;

using Item = QPService.Item;
using Payment = QPService.Payment;
using PazarYeriAktarim = OBase.Pazaryeri.Domain.Entities.PazarYeriAktarim;

namespace OBase.Pazaryeri.Business.Services.Concrete.Order.OrderConverter
{
    public class IdefixOrderConverter : BaseOrderConverter<IdefixOrderDto>
    {
        protected override string MerchantName => "Idefix";
        protected override int ChannelId => SaleChannelId.Idefix;
        protected override int PaymentMethodId => PazarYeriPaymentId.Idefix;

        protected override string GetOrderNumber(IdefixOrderDto order) => order.OrderNumber;

        protected override void SetCommonDeliveryActionProperties(DeliveryAction deliveryAction, IdefixOrderDto order, long seqId, string merchantNo)
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

            deliveryAction.DeliveryDate =order.EstimatedDeliveryDate.ToString("dd.MM.yyyy");
            deliveryAction.OrderDate = order.OrderDate.ToString("dd.MM.yyyy");
            deliveryAction.ClaimDate = order.OrderDate.ToString("dd.MM.yyyy");
            deliveryAction.OrderTime = order.OrderDate.ToString("HH:mm");
            deliveryAction.LastPickupTime = order.EstimatedDeliveryDate.AddHours(-1).ToString("HH:mm");
            deliveryAction.CustomerNote = "";

            deliveryAction.UpdateDeliveryAction = true;
            deliveryAction.CollectOrder = false;
        }
        protected override void SetAddressInfo(DeliveryAction deliveryAction, IdefixOrderDto order)
        {

            deliveryAction.ToAddress = new Addres
            {
                Address = order.ShippingAddress?.Address1 ?? "",
                Name = order.ShippingAddress?.Address1 ?? "",
                City = new CityObj() { id = order.ShippingAddress.CityPlate, Value = order.ShippingAddress.City },
                District = new DistrictObj() { id = order.ShippingAddress.CountyId, Value = order.ShippingAddress.County },
                Tel1 = order.ShippingAddress.Phone
            };


            deliveryAction.FromAddress = new Addres
            {
                Address = order.ShippingAddress?.Address1 ?? "",
                Name = order.ShippingAddress?.Address1 ?? "",
                City = new CityObj() { id = order.ShippingAddress.CityPlate, Value = order.ShippingAddress.City },
                District = new DistrictObj() { id = order.ShippingAddress.CountyId, Value = order.ShippingAddress.County }
            };

        }

        protected override void SetCustomerInfo(DeliveryAction deliveryAction, IdefixOrderDto order)
        {

            deliveryAction.Customer = new QPService.Customer
            {
                CustomerId = order.CustomerId.ToString(),
                City = new CityObj { id = order.InvoiceAddress.CityPlate, Value = order.InvoiceAddress.City },
                District = new DistrictObj { id = order.InvoiceAddress.CountyId, Value = order.InvoiceAddress.County },
                Name = $"{order.CustomerContactName}",
                Email = order.CustomerContactMail,
                Tel1 = order.ShippingAddress.Phone,
                CustomerType = 0
            };
        }

        protected override void SetDeliveryInfo(DeliveryAction deliveryAction, IdefixOrderDto order)
        {
            base.SetDeliveryInfo(deliveryAction, order);
        }

        protected override string GetMarketplaceDeliveryModel(IdefixOrderDto order)
        {
            return order.DeliveryType == IdefixConstants.OrderTransportType.DLVRY ?
                QpDeliveryModels.MarketPlace : QpDeliveryModels.Store;
        }

        protected override void SetInvoiceInfo(DeliveryAction deliveryAction, IdefixOrderDto order)
        {
            order.InvoiceAddress = new Domain.Dtos.Idefix.Address()
            {
                City = string.IsNullOrEmpty(order.InvoiceAddress?.City) ? "Şehir Belirtilmemiş" : order.InvoiceAddress?.City,
                CityId = order.InvoiceAddress.CityId,
                CityPlate = order.InvoiceAddress.CityPlate,
                Address1 = string.IsNullOrEmpty(order.InvoiceAddress?.Address1) ? "Adres Belirtilmemiş" : order.InvoiceAddress?.Address1,               
                FullAddress = order.InvoiceAddress?.FullAddress,
                County = string.IsNullOrEmpty(order.InvoiceAddress.County) ? "İlçe Belirtilmemiş" : order.InvoiceAddress.County,
                CountyId = order.InvoiceAddress.CountyId,
                CountryCode = "TR",
                Phone = order.InvoiceAddress.Phone,              
                Neighboorhood= string.IsNullOrEmpty(order.InvoiceAddress?.Neighboorhood) ? "Mahalle Belirtilmemiş" : order.InvoiceAddress?.Neighboorhood,
                NeighboorhoodId=order.InvoiceAddress.NeighboorhoodId,
                
            };

            if (order.InvoiceAddress != null)
            {
                order.InvoiceAddress.City = string.IsNullOrEmpty(order.InvoiceAddress.City) ? "Şehir Belirtilmemiş" : order.InvoiceAddress.City;
                order.InvoiceAddress.Neighboorhood = string.IsNullOrEmpty(order.InvoiceAddress.Neighboorhood) ? "İlçe Belirtilmemiş" : order.InvoiceAddress.Neighboorhood;
            }


            deliveryAction.CustomerInvoice = new CustomerInvoice
            {
                Address = order.InvoiceAddress.Address1,
                City = new CityObj() { id = order.InvoiceAddress.CityId, Value = order.InvoiceAddress.City },
                District = new DistrictObj() { id = order.InvoiceAddress.CountyId, Value = order.ShippingAddress.County },               
                Name = $"{order.InvoiceAddress.FirstName} {order.InvoiceAddress.LastName}",
                TaxNo = (order.InvoiceAddress?.IdentificationNumber ?? "").Length > 11 ? (order.InvoiceAddress?.IdentificationNumber ?? "")[..11] : (order.InvoiceAddress?.IdentificationNumber ?? ""),
                InvoiceTotalYTL = order.TotalPrice,
                PrepareInvoice = true
            };

        }

        protected override void SetShipmentInfo(DeliveryAction deliveryAction, IdefixOrderDto order)
        {
            order.ShippingAddress = new Domain.Dtos.Idefix.Address()
            {
                City = string.IsNullOrEmpty(order.ShippingAddress?.City) ? "Şehir Belirtilmemiş" : order.ShippingAddress?.City,
                CityId= order.ShippingAddress.CityId,
                CityPlate = order.ShippingAddress.CityPlate,
                Address1 = string.IsNullOrEmpty(order.ShippingAddress?.Address1) ? "Adres Belirtilmemiş" : order.ShippingAddress?.Address1,
                FullAddress = order.ShippingAddress?.FullAddress,
                County = string.IsNullOrEmpty(order.ShippingAddress.County) ? "İlçe Belirtilmemiş" : order.ShippingAddress.County,
                CountyId = order.ShippingAddress.CountyId,               
                CountryCode = "TR",
                Phone = order.ShippingAddress.Phone,
                Neighboorhood = string.IsNullOrEmpty(order.ShippingAddress?.Neighboorhood) ? "Mahalle Belirtilmemiş" : order.ShippingAddress?.Neighboorhood,
                NeighboorhoodId = order.ShippingAddress.NeighboorhoodId,
            };
        }

        protected override void SetPaymentInfo(DeliveryAction deliveryAction, IdefixOrderDto order)
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

        protected override void SetItems(DeliveryAction deliveryAction, IdefixOrderDto order, PazarYeriBirimTanim store, IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts, SachetProduct[] sachetProduct,long seqId)
        {
            var items = new List<Item>();

            var groupItemsByBarcode = GroupItemsByBarcode(order.Items);

            foreach (var line in groupItemsByBarcode)
            {
                var transferProduct = transferProducts.FirstOrDefault(x => x.PazarYeriMalNo == line.Barcode && x.BirimNo == store.BirimNo);
                var product = transferProduct != null ? products.FirstOrDefault(x => x.MalNo.Trim() == transferProduct.MalNo?.ToString().Trim()) : null;
                if (product == null || store == null || product.MalNo == null)
                {
                    throw new OrderConversionException($"QP Error With Id {seqId} OrderId {order.OrderNumber} ProductBarcode {line.Barcode} --> Product Not Found!", null);
                }
                product.PyUrunSatisDeger = product.PyUrunSatisDeger ?? 0;
                var item = new Item
                {
                    id = line.Id,
                    ProductId = long.Parse(product.MalNo),
                    ProductModelId = 0,
                    ProductModelName = line.ProductName,
                    ShopDefinedCode = product.MalNo,
                    Unit = !string.IsNullOrEmpty(product.PyUrunSatisBirim) && product.PyUrunSatisBirim.ToUpper() == CommonConstants.KG ? product.PyUrunSatisBirim.ToUpper() : CommonConstants.ADET,
                    ShopId = 1,
                    ShopName = "NONE",
                    StoreId = Convert.ToInt32(store?.BirimNo),
                    StoreName = store?.BirimAdi,
                    ProductName = $"{line.BrandName} {line.ProductName}",
                    Amount = order.Items?.Count(x => x.Barcode.Trim() == line.Barcode.Trim()) ?? 1,
                    VAT = (byte)line.VatRate,
                    PriceYTL = line.Price,
                    ShopDefinedStoreCode = store?.BirimNo,
                    AlternativeProductRequested = "NO",
                    DM3=0,
                    GiftAmount = 0,
                    ReyonOrder=0
                };

                items.Add(item);
            }
          
            if (sachetProduct != null)
            {
                var sachetItems = CreateSachetItems(sachetProduct, store);
                if (sachetItems.Any())
                    items.AddRange(sachetItems);
            }

            deliveryAction.Items = items.ToArray();
        }
        protected override string GetUserCode(IdefixOrderDto order, long seqId)=> order.CustomerId.ToString();

        protected override string GetUserPassword()=>string.Empty;

        protected override decimal GetDeliveryActionId(IdefixOrderDto order)
        {
            return order.Id;
        }

        protected override decimal GetDiscountTotalYTL(IdefixOrderDto order)
        {
            return Convert.ToDecimal(order.TotalDiscount);
        }

        private decimal CalculatePrice(Domain.Dtos.Idefix.ProductItem line, PazarYeriMalTanim product, PazarYeriAktarim transferProduct)
        {
            decimal priceytl = 0;
            if (product?.PyUrunSatisBirim == null) { priceytl = line.Price; }
            else if (product.PyUrunSatisBirim.ToUpper() == CommonConstants.KG)
            {
                priceytl = (transferProduct?.IndirimliSatisFiyat > 0 ? transferProduct?.IndirimliSatisFiyat : transferProduct?.SatisFiyat) ?? 0;
            }

            priceytl = priceytl == 0 ? line.Price : priceytl;
            return line.Price;
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



        private List<OBase.Pazaryeri.Domain.Dtos.Idefix.ProductItem> GroupItemsByBarcode(List<OBase.Pazaryeri.Domain.Dtos.Idefix.ProductItem> idefixOrderItems)
        {
            var groupedItems = idefixOrderItems
                .GroupBy(i => i.Barcode)
                .Select(g => new OBase.Pazaryeri.Domain.Dtos.Idefix.ProductItem
                {
                    ProductName = g.First().ProductName,
                    Id = g.First().Id,
                    Barcode = g.Key,
                    ErpId = g.First().ErpId,
                    Image = g.First().Image,
                    Price = g.First().Price,
                    InterestPrice = g.First().InterestPrice,
                    TotalDiscount = g.First().TotalDiscount,
                    PlatformDiscount = g.First().PlatformDiscount,
                    VendorDiscount = g.First().VendorDiscount,
                    DiscountedTotalPrice = g.First().DiscountedTotalPrice,
                    Currency = g.First().Currency,
                    ItemStatus = g.First().ItemStatus,
                    ComparePrice = g.First().ComparePrice,
                    VendorAmount = g.First().VendorAmount,
                    StateDescription = g.First().StateDescription,
                    BrandName = g.First().BrandName,
                    MerchantSku = g.First().MerchantSku,
                    VatRate = g.First().VatRate,
                    CommissionAmount = g.First().CommissionAmount,
                    EarningAmount = g.First().EarningAmount,
                    WithholdingAmount = g.First().WithholdingAmount,
                    LastShipmentDate = g.First().LastShipmentDate,
                    CustomizableNote = g.First().CustomizableNote
                }).ToList();
            return groupedItems;
        }
    }
}