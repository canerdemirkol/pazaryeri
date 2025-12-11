using Newtonsoft.Json;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.Orders
{
    //public class OrderDto
    //{
    //    public List<Order> orders { get; set; }
    //}
    public class GetirOrderDto:BaseOrderDto
    {
        [JsonIgnore]
        public string merchantNo { get; set; }

        [JsonProperty("orderID")]
        public string orderId { get; set; }
        public string orderType { get; set; }
        public long orderNumber { get; set; }

        public string confirmationId { get; set; } //Getir getirsin seçildi ise kurye doğrulama kodu olarak kullanılıyor

        [JsonProperty("shopID")]
        public string shopId { get; set; }

        [JsonProperty("supplierID")]
        public string supplierId { get; set; }

        [JsonProperty("chainID")]
        public string chainId { get; set; }
        public decimal? maxPossibleAmountMerchantCanCharge { get; set; }
        public decimal? minPossibleAmountMerchantCanCharge { get; set; }
        public bool isPromoApplied { get; set; }
        public string clientNote { get; set; }
        public string clientBatchedNote { get; set; }
        public bool doNotKnock { get; set; }
        public int? shopMinBasketAmount { get; set; }
        public int? getirMinBasketAmount { get; set; }
        public string cancelDate { get; set; }
        public decimal totalPrice { get; set; }
        public DateTime? checkoutDate { get; set; }
        public int deliveryType { get; set; }
        public int? paymentMethod { get; set; }
        public string shippingFirm { get; set; }
        public bool isAlternativeProductEnabled { get; set; }

        public Customer customer { get; set; }
        public InvoiceAddress invoiceAddress { get; set; }
        public PaymentMethodText paymentMethodText { get; set; }
        public List<Product> products { get; set; }
        public PackagingInfo packagingInfo { get; set; }
        public DeliveryInfo deliveryInfo { get; set; }
    }

    public class DeliveryInfo
    {
        public string id { get; set; }
        public string address { get; set; }
        public string aptNo { get; set; }
        public string floor { get; set; }
        public string doorNo { get; set; }
        public string city { get; set; }
        public string town { get; set; }
        public string identityNumber { get; set; }
    }

    public class PackagingInfo
    {
        public Bag bag { get; set; }
        public int? totalPackageableProductVolume { get; set; }
        public int? bagCalculatedCount { get; set; }
        public int? bagCount { get; set; }
        public decimal? totalPackagingPrice { get; set; }

        [JsonIgnore]
        public string bagNumber { get; set; }
        [JsonIgnore]
        public decimal? BagUnitPrice { get; set; }
    }

    public class Bag
    {
        public Name name { get; set; }
        public string imgUrl { get; set; }
        public string wideImgUrl { get; set; }
        public int? maxCount { get; set; }
        public int? minCount { get; set; }
        public decimal? unitPrice { get; set; }
        public int? unitVolume { get; set; }
    }

    public class Product
    {
        public string sourceId { get; set; }
        public int? maxTotalWeight { get; set; }
        public int? minTotalWeight { get; set; }
        public int? totalWeight { get; set; }
        public int? finalTotalAmount { get; set; }
        public bool isPackagable { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
        public string product { get; set; }
        public string catalogProductId { get; set; }
        public int? count { get; set; }
        public int? finalCount { get; set; }
        public Name name { get; set; }
        public decimal? price { get; set; }
        public decimal? totalPrice { get; set; }
        public decimal? finalTotalPrice { get; set; }
        public bool hasBarcode { get; set; }
        public string barcode { get; set; }
        public List<string> barcodes { get; set; }
        public string vendorId { get; set; }
        public string type { get; set; }
        public string note { get; set; }
        public decimal maxPossiblePrice { get; set; }
        public decimal minPossiblePrice { get; set; }
        public bool? isProvisionAvailable { get; set; }
        [JsonIgnore]
        public int vatRate { get; set; } = 20;
    }

    public class Name
    {
        public string tr { get; set; }
        public string en { get; set; }
    }

    public class PaymentMethodText
    {
        public string tr { get; set; }
        public string en { get; set; }
    }

    public class InvoiceAddress
    {
        public string id { get; set; }
        public string address { get; set; }
        public string aptNo { get; set; }
        public string floor { get; set; }
        public string doorNo { get; set; }
        public string city { get; set; }
        public string town { get; set; }
        public string identityNumber { get; set; }

        [JsonIgnore]
        public int cityCode { get; set; } = 01;
        [JsonIgnore]
        public string phone { get; set; } = "5555555555";
        [JsonIgnore]
        public string district { get; set; } = "Semt belirtilmemiş.";
        [JsonIgnore]
        public int districtId { get; set; } = 02;
        [JsonIgnore]
        public string countryCode { get; set; } = "TR";
    }

    public class Customer
    {
        [JsonIgnore]
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string name { get; set; }
        public string email { get; set; }
        public string clientPhoneNumber { get; set; }
        public string clientMaskedPhoneNumber { get; set; }
        public Location location { get; set; }
    }
    public class Location
    {
        public decimal? lat { get; set; }
        public decimal? lon { get; set; }
    }
}