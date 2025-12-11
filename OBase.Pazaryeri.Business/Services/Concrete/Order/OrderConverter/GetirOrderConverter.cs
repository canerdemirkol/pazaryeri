using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using OBase.Pazaryeri.Domain.Entities;
using QPService;
using static OBase.Pazaryeri.Domain.Constants.Constants;

using Item = QPService.Item;
using Payment = QPService.Payment;
using PazarYeriAktarim = OBase.Pazaryeri.Domain.Entities.PazarYeriAktarim;

namespace OBase.Pazaryeri.Business.Services.Concrete.Order.OrderConverter
{
    public class GetirOrderConverter : BaseOrderConverter<GetirOrderDto>
    {
        protected override string MerchantName => "Getir";

        protected override int ChannelId => SaleChannelId.Getir;

        protected override int PaymentMethodId => PazarYeriPaymentId.Getir;

        protected override decimal GetDeliveryActionId(GetirOrderDto order)
        {
            return order.orderNumber;
        }

        protected override decimal GetDiscountTotalYTL(GetirOrderDto order)
        {
            return 0m;
        }

        protected override string GetMarketplaceDeliveryModel(GetirOrderDto order)
        {
            return order.deliveryType == 1 ?
               QpDeliveryModels.MarketPlace : QpDeliveryModels.Store;
        }

        protected override string GetOrderNumber(GetirOrderDto order) => order.confirmationId;

        protected override string GetUserCode(GetirOrderDto order, long seqId)
        {
            return order.customer.id;
        }

        protected override string GetUserPassword()
        {
            return string.Empty;
        }

        protected override void SetAddressInfo(DeliveryAction deliveryAction, GetirOrderDto order)
        {
            deliveryAction.ToAddress = new Addres
            {
                Address = order.deliveryInfo?.address ?? "",
                Name = order.deliveryInfo?.address ?? "",
                City = new CityObj() { id = order.invoiceAddress.cityCode, Value = order.invoiceAddress.city },
                District = new DistrictObj() { id = order.invoiceAddress.districtId, Value = order.invoiceAddress.district },
                Tel1 = order.invoiceAddress.phone
            };


            deliveryAction.FromAddress = new Addres
            {
                Address = order.deliveryInfo.address,
                Name = order.deliveryInfo.address,
                City = new CityObj() { id = order.invoiceAddress.cityCode, Value = order.invoiceAddress.city },
                District = new DistrictObj() { id = order.invoiceAddress.districtId, Value = order.invoiceAddress.district }
            };
        }

        protected override void SetCustomerInfo(DeliveryAction deliveryAction, GetirOrderDto order)
        {
            deliveryAction.Customer = new QPService.Customer
            {
                CustomerId = order.customer.id,
                City = new CityObj { id = order.invoiceAddress.cityCode, Value = order.invoiceAddress.city },
                District = new DistrictObj { id = order.invoiceAddress.districtId, Value = order.invoiceAddress.district },
                Name = order.customer.name,
                Email = order.customer.email,
                Tel1 = order.customer.clientPhoneNumber,
                Tel2 = order.customer.clientMaskedPhoneNumber
            };
        }

        protected override void SetInvoiceInfo(DeliveryAction deliveryAction, GetirOrderDto order)
        {
            order.invoiceAddress = new Domain.Dtos.Getir.Orders.InvoiceAddress()
            {
                address = !string.IsNullOrEmpty(order.invoiceAddress?.address ?? "") ? order.invoiceAddress.address : "Adres Belirtilmemiş",
                doorNo = !string.IsNullOrEmpty(order.invoiceAddress?.doorNo ?? "") ? order.invoiceAddress.doorNo : "Kapı No Belirtilmemiş",
                city = !string.IsNullOrEmpty(order.invoiceAddress?.city ?? "") ? order.invoiceAddress.city : "Şehir Belirtilmemiş",
                aptNo = !string.IsNullOrEmpty(order.invoiceAddress?.aptNo ?? "") ? order.invoiceAddress.aptNo : "Apt No  Belirtilmemiş",
                floor = !string.IsNullOrEmpty(order.invoiceAddress?.floor ?? "") ? order.invoiceAddress.floor : "Kat Belirtilmemiş",
                town = !string.IsNullOrEmpty(order.invoiceAddress?.town ?? "") ? order.invoiceAddress.town : "İlçe Belirtilmemiş",
                //order.invoiceAddress?.address ?? "Adres Belirtilmemiş",
            };



            deliveryAction.CustomerInvoice = new CustomerInvoice
            {
                Address = order.invoiceAddress.address,
                City = new CityObj() { id = order.invoiceAddress.cityCode, Value = order.invoiceAddress.city },
                District = new DistrictObj() { id = order.invoiceAddress.districtId, Value = order.invoiceAddress.district },
                Name = order.customer.name,
                TaxNo = order.invoiceAddress.identityNumber,
                InvoiceTotalYTL = order.totalPrice,
                PrepareInvoice = true
            };
        }

        protected override void SetPaymentInfo(DeliveryAction deliveryAction, GetirOrderDto order)
        {
            deliveryAction.Payments = new[]
            {
                new Payment
                {
                    PaymentMethod = new PaymentMethod { id = PaymentMethodId, Value = $"ONLINE ODEME {MerchantName}" },
                    PaymentOption = new PaymentOption { id = PaymentMethodId, Value = $"ONLINE ODEME {MerchantName}" },
                    AmountYTL=decimal.Parse(order.totalPrice.ToString()),
                    CaptureAmountYTL=decimal.Parse(order.totalPrice.ToString()),
                    InstallmentCount=0,
                    id=PaymentMethodId,
                    PointsUsedYTL=0
                }
            };
        }


        protected override void SetDeliveryInfo(DeliveryAction deliveryAction, GetirOrderDto order)
        {
            base.SetDeliveryInfo(deliveryAction, order);
       

            order.deliveryInfo = new DeliveryInfo()
            {
                address = !string.IsNullOrEmpty(order.invoiceAddress?.address ?? "") ? order.invoiceAddress.address : "Adres Belirtilmemiş",
                doorNo = !string.IsNullOrEmpty(order.invoiceAddress?.doorNo ?? "") ? order.invoiceAddress.doorNo : "Kapı No Belirtilmemiş",
                city = !string.IsNullOrEmpty(order.invoiceAddress?.city ?? "") ? order.invoiceAddress.city : "Şehir Belirtilmemiş",
                aptNo = !string.IsNullOrEmpty(order.invoiceAddress?.aptNo ?? "") ? order.invoiceAddress.aptNo : "Apt No  Belirtilmemiş",
                floor = !string.IsNullOrEmpty(order.invoiceAddress?.floor ?? "") ? order.invoiceAddress.floor : "Kat Belirtilmemiş",
                town = !string.IsNullOrEmpty(order.invoiceAddress?.town ?? "") ? order.invoiceAddress.town : "İlçe Belirtilmemiş",
            };    
        }

        protected override void SetCommonDeliveryActionProperties(DeliveryAction deliveryAction, GetirOrderDto order, long seqId, string merchantNo)
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


            deliveryAction.UpdateDeliveryAction = true;
            deliveryAction.CollectOrder = false;
            deliveryAction.CustomerNote = order.clientNote;
            deliveryAction.CrmMessage = "";


            deliveryAction.DeliveryDate = order.checkoutDate?.ToString("dd.MM.yyyy");
            deliveryAction.OrderDate = order.checkoutDate?.ToString("dd.MM.yyyy");
            deliveryAction.OrderTime = order.checkoutDate?.ToString("HH:mm");
        }
        protected override void SetItems(DeliveryAction deliveryAction, GetirOrderDto order, PazarYeriBirimTanim store, IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts, SachetProduct[] sachetProduct, long seqId)
        {
            var items = new List<Item>();
            if (order.packagingInfo != null)
            {
                items.Add(new Item()
                {
                    id = int.Parse(order.packagingInfo.bagNumber),
                    ShopId = 1,
                    ShopName = "NONE",
                    StoreId = Convert.ToInt32(store?.BirimNo),
                    StoreName = store?.BirimAdi,
                    ShopDefinedStoreCode = store?.BirimNo,
                    PriceYTL = order.packagingInfo.BagUnitPrice ?? 0.50m,
                    ProductModelName = "ALISVERIS POSETI PLASTIK..",
                    ProductName = "ALISVERIS POSETI PLASTIK..",
                    ShopDefinedCode = order.packagingInfo.bagNumber,
                    Packable = false,
                    VAT = 0,
                    DM3 = 0,
                    GiftAmount = 0,
                    Unit = CommonConstants.ADET,
                    AlternativeProductRequested = "NO",
                    ReyonOrder = 0,
                    ProductId = int.Parse(order.packagingInfo.bagNumber),
                    Amount = (float)order.packagingInfo.bagCount,
                    ProductModelId = 0

                });
            }

            foreach (var line in order.products)
            {
                PazarYeriAktarim transferProduct = null;
                transferProduct = transferProducts.FirstOrDefault(x => line.vendorId == x.PazarYeriMalNo && x.BirimNo == store.BirimNo);

                var product = transferProduct != null ? products.FirstOrDefault(x => x.MalNo.Trim() == transferProduct.MalNo?.Trim()) : null;
                if (product == null || store == null || product.MalNo == null)
                {
                    throw new Exception($"QP Error With Id {seqId} OrderId {order.orderId} ProductBarcode {string.Join(',', line.barcodes)} --> Product Not Found!");
                }

                if (items.Any(x => x.ProductId == long.Parse(product?.MalNo)))
                {
                    var itemToUpdate = items.FirstOrDefault(x => x.ProductId == long.Parse(product?.MalNo));
                    itemToUpdate.Amount += line.type == GetirConstants.ProductType.Gr ? (float)(line.totalWeight) / 1000 : (line.count ?? 1);
                }
                else
                {
                    items.Add(new Item()
                    {
                        ProductId = long.Parse(product?.MalNo),
                        id = long.Parse(product?.MalNo),
                        ProductModelId = 0,
                        ProductModelName = line.name.tr,
                        ShopDefinedCode = product?.MalNo,
                        Unit = !string.IsNullOrEmpty(product.PyUrunSatisBirim) && product.PyUrunSatisBirim.ToUpper() == CommonConstants.KG ? product.PyUrunSatisBirim.ToUpper() : CommonConstants.ADET,
                        ShopId = 1,
                        ShopName = "NONE",
                        StoreId = Convert.ToInt32(store?.BirimNo),
                        StoreName = store?.BirimAdi,
                        ProductName = line.name.tr,
                        Amount = CalculateAmount(line),
                        VAT = (byte)line.vatRate,
                        DM3 = 0,
                        GiftAmount = 0,
                        PriceYTL = CalculatePrice(line, transferProduct),
                        ShopDefinedStoreCode = store?.BirimNo,
                        AlternativeProductRequested = "NO",
                        ReyonOrder = 0,


                    });
                }

            }
            deliveryAction.Items = items.ToArray();
        }

        protected override void SetShipmentInfo(DeliveryAction deliveryAction, GetirOrderDto order){ }

        private decimal CalculatePrice(OBase.Pazaryeri.Domain.Dtos.Getir.Orders.Product line,PazarYeriAktarim transferProduct)
        {
            return (line.type == GetirConstants.ProductType.Gr
                ? transferProduct?.SatisFiyat ?? ((line.totalPrice / line.totalWeight) * 1000)
                : line.price) ?? 0m;
        }

        private float CalculateAmount(OBase.Pazaryeri.Domain.Dtos.Getir.Orders.Product line)
        {
            return line.type == GetirConstants.ProductType.Gr
                ? (float)(line.totalWeight) / 1000
                : line.count ?? 1;
        }
    }
}