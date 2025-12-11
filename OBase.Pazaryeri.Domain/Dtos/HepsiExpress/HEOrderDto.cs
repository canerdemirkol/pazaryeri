using Hangfire.Server;
using Newtonsoft.Json;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OBase.Pazaryeri.Domain.Helper.ConverterHelper;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEOrderDto:BaseOrderDto
    {
        [JsonProperty("order")]
        public Order Order { get; set; }

        [JsonProperty("items")]
        public HEItem[] Items { get; set; }
    }
    public class HEItem
    {
        [JsonProperty("dueDate")]
        public DateTimeOffset DueDate { get; set; }

        [JsonProperty("lastStatusUpdateDate")]
        public DateTimeOffset LastStatusUpdateDate { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("unitPrice")]
        public HandlingFee UnitPrice { get; set; }

        [JsonProperty("hbDiscount")]
        public HbDiscount HbDiscount { get; set; }

        [JsonProperty("vat")]
        public HandlingFee Vat { get; set; }

        [JsonProperty("vatRate")]
        public double VatRate { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("customerId")]
        public Guid CustomerId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("shippingAddress")]
        public Address ShippingAddress { get; set; }

        [JsonProperty("invoice")]
        public Invoice Invoice { get; set; }

        [JsonProperty("sapNumber")]
        public string SapNumber { get; set; }

        [JsonProperty("dispatchTime")]
        public long DispatchTime { get; set; }

        [JsonProperty("commission")]
        public HandlingFee Commission { get; set; }

        [JsonProperty("paymentTermInDays")]
        public long PaymentTermInDays { get; set; }

        [JsonProperty("commissionType")]
        public long CommissionType { get; set; }

        [JsonProperty("cargoCompanyModel")]
        public CargoCompanyModel CargoCompanyModel { get; set; }

        [JsonProperty("cargoCompany")]
        public string CargoCompany { get; set; }

        [JsonProperty("customizedText01")]
        public object CustomizedText01 { get; set; }

        [JsonProperty("customizedText02")]
        public string CustomizedText02 { get; set; }

        [JsonProperty("customizedText03")]
        public string CustomizedText03 { get; set; }

        [JsonProperty("customizedText04")]
        public string CustomizedText04 { get; set; }

        [JsonProperty("customizedTextX")]
        public string CustomizedTextX { get; set; }

        [JsonProperty("creditCardHolderName")]
        public object CreditCardHolderName { get; set; }

        [JsonProperty("isCustomized")]
        public bool IsCustomized { get; set; }

        [JsonProperty("canCreatePackage")]
        public bool CanCreatePackage { get; set; }

        [JsonProperty("isCancellable")]
        public bool IsCancellable { get; set; }

        [JsonProperty("deliveryType")]
        public string DeliveryType { get; set; }

        [JsonProperty("deliveryOptionId")]
        public long DeliveryOptionId { get; set; }

        [JsonProperty("slot")]
        public Slot Slot { get; set; }

        [JsonProperty("pickUpTime")]
        public string PickUpTime { get; set; }

        [JsonProperty("discountInfo")]
        public DiscountInfo[] DiscountInfo { get; set; }

        [JsonProperty("merchantSku")]
        public string MerchantSku { get; set; }

        [JsonProperty("purchasePrice")]
        public HandlingFee PurchasePrice { get; set; }

        [JsonProperty("productImageUrlFormat")]
        public string ProductImageUrlFormat { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("merchantId")]
        public Guid MerchantId { get; set; }

        [JsonProperty("totalPrice")]
        public HandlingFee TotalPrice { get; set; }

        [JsonProperty("totalPriceWithoutDiscount")]
        public HandlingFee TotalPriceWithoutDiscount { get; set; }

        [JsonProperty("isCancellableByHbAdmin")]
        public bool IsCancellableByHbAdmin { get; set; }

        [JsonProperty("discountPriceToBeInvoicedHb")]
        public HandlingFee DiscountPriceToBeInvoicedHb { get; set; }

        [JsonProperty("deliveryNote")]
        public Note DeliveryNote { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("orderDate")]
        public DateTimeOffset OrderDate { get; set; }

        [JsonProperty("orderNote")]
        public Note OrderNote { get; set; }

        [JsonProperty("DeptorDifferenceAmount")]
        public double DeptorDifferenceAmount { get; set; }
    }

    public class CargoCompanyModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("logoUrl")]
        public string LogoUrl { get; set; }

        [JsonProperty("trackingUrl")]
        public object TrackingUrl { get; set; }
    }

    public class HandlingFee
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }

    public class Note
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }
    }

    public class DiscountInfo
    {
        [JsonProperty("campaignId")]
        public long CampaignId { get; set; }

        [JsonProperty("campaignName")]
        public string CampaignName { get; set; }

        [JsonProperty("campaignType")]
        public long CampaignType { get; set; }

        [JsonProperty("conditionOrAward")]
        public long ConditionOrAward { get; set; }

        [JsonProperty("discountTotal")]
        public decimal DiscountTotal { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("campaignDiscountRate")]
        public long CampaignDiscountRate { get; set; }

        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("isProtectedCampaign")]
        public bool IsProtectedCampaign { get; set; }
    }

    public class HbDiscount
    {
        [JsonProperty("totalPrice")]
        public HandlingFee TotalPrice { get; set; }

        [JsonProperty("unitPrice")]
        public HandlingFee UnitPrice { get; set; }
    }

    public class Invoice
    {
        [JsonProperty("turkishIdentityNumber")]
        public object TurkishIdentityNumber { get; set; }

        [JsonProperty("taxNumber")]
        public string TaxNumber { get; set; }

        [JsonProperty("taxOffice")]
        public string TaxOffice { get; set; }

        [JsonProperty("address")]
        public Address Address { get; set; }
    }

    public class Address
    {
        [JsonProperty("addressId")]
        public Guid AddressId { get; set; }

        [JsonProperty("address")]
        public string AddressAddress { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("alternatePhoneNumber")]
        public string AlternatePhoneNumber { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("town")]
        public string Town { get; set; }

        [JsonProperty("coordinates")]
        public Coordinates Coordinates { get; set; }

        [JsonProperty("directions", NullValueHandling = NullValueHandling.Ignore)]
        public string Directions { get; set; }
    }

    public class Coordinates
    {
        [JsonProperty("latitude")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Latitude { get; set; }

        [JsonProperty("longitude")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Longitude { get; set; }
    }

    public class Slot
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("timeSlot")]
        public string TimeSlot { get; set; }

        [JsonProperty("zoneId")]
        public string ZoneId { get; set; }
    }

    public class Order
    {
        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty("maxProvisionAmount")]
        public HandlingFee MaxProvisionAmount { get; set; }

        [JsonProperty("handlingFee")]
        public HandlingFee HandlingFee { get; set; }

        [JsonProperty("isProvision")]
        public bool IsProvision { get; set; }

        [JsonProperty("eta")]
        public long Eta { get; set; }
    }
}