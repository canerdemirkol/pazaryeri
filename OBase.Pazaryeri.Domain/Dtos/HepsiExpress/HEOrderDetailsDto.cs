using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEOrderDetailsDto
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("paymentStatus")]
        public string PaymentStatus { get; set; }

        [JsonProperty("orderDate")]
        public DateTimeOffset OrderDate { get; set; }

        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonProperty("customer")]
        public OrderDetailCustomer Customer { get; set; }

        [JsonProperty("invoice")]
        public OrderDetailInvoice Invoice { get; set; }

        [JsonProperty("deliveryAddress")]
        public OrderDetailDeliveryAddressClass DeliveryAddress { get; set; }

        [JsonProperty("orderNote")]
        public Note OrderNote { get; set; }

        [JsonProperty("items")]
        public OrderDetailItem[] Items { get; set; }
    }
    public class OrderDetailCustomer
    {
        [JsonProperty("customerId")]
        public Guid CustomerId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class OrderDetailDeliveryAddressClass
    {
        [JsonProperty("addressId")]
        public Guid AddressId { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

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

        [JsonProperty("directions")]
        public string Directions { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }
    }

    public class OrderDetailInvoice
    {
        [JsonProperty("turkishIdentityNumber")]
        public string TurkishIdentityNumber { get; set; }

        [JsonProperty("taxNumber")]
        public string TaxNumber { get; set; }

        [JsonProperty("taxOffice")]
        public object TaxOffice { get; set; }

        [JsonProperty("address")]
        public OrderDetailAddress Address { get; set; }
    }


    public class OrderDetailAddress
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

        [JsonProperty("directions")]
        public string Directions { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }
    }

    public class OrderDetailItem
    {
        [JsonProperty("dueDate")]
        public DateTimeOffset DueDate { get; set; }

        [JsonProperty("lastStatusUpdateDate")]
        public DateTimeOffset LastStatusUpdateDate { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("orderId")]
        public Guid OrderId { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("orderDate")]
        public DateTimeOffset OrderDate { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("gram")]
        public long? Gram { get; set; }

        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("totalPrice")]
        public OrderDetailCommission TotalPrice { get; set; }

        [JsonProperty("unitPrice")]
        public OrderDetailCommission UnitPrice { get; set; }

        [JsonProperty("hbDiscount")]
        public OrderDetailHbDiscount HbDiscount { get; set; }

        [JsonProperty("vat")]
        public double Vat { get; set; }

        [JsonProperty("vatRate")]
        public long VatRate { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("customerId")]
        public Guid CustomerId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("shippingAddress")]
        public OrderDetailDeliveryAddressClass ShippingAddress { get; set; }

        [JsonProperty("invoice")]
        public OrderDetailInvoice Invoice { get; set; }

        [JsonProperty("sapNumber")]
        public string SapNumber { get; set; }

        [JsonProperty("dispatchTime")]
        public long DispatchTime { get; set; }

        [JsonProperty("commission")]
        public OrderDetailCommission Commission { get; set; }

        [JsonProperty("commissionRate")]
        public long CommissionRate { get; set; }

        [JsonProperty("paymentTermInDays")]
        public long PaymentTermInDays { get; set; }

        [JsonProperty("commissionType")]
        public long CommissionType { get; set; }

        [JsonProperty("cargoCompanyModel")]
        public OrderDetailCargoCompanyModel CargoCompanyModel { get; set; }

        [JsonProperty("cargoCompany")]
        public string CargoCompany { get; set; }

        [JsonProperty("customizedText01")]
        public string CustomizedText01 { get; set; }

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

        [JsonProperty("isCancellableByHbAdmin")]
        public bool IsCancellableByHbAdmin { get; set; }

        [JsonProperty("deliveryType")]
        public string DeliveryType { get; set; }

        [JsonProperty("deliveryOptionId")]
        public long DeliveryOptionId { get; set; }

        [JsonProperty("slot")]
        public OrderDetailSlot Slot { get; set; }

        [JsonProperty("pickUpTime")]
        public string PickUpTime { get; set; }

        [JsonProperty("discountInfo")]
        public object[] DiscountInfo { get; set; }

        [JsonProperty("merchantSKU")]
        public string MerchantSku { get; set; }

        [JsonProperty("purchasePrice")]
        public OrderDetailCommission PurchasePrice { get; set; }

        [JsonProperty("deliveryNote")]
        public Note DeliveryNote { get; set; }

        [JsonProperty("isCargoChangable")]
        public bool IsCargoChangable { get; set; }

        [JsonProperty("warehouse")]
        public Warehouse Warehouse { get; set; }

        [JsonProperty("deptorDifferenceAmount")]
        public long DeptorDifferenceAmount { get; set; }

        [JsonProperty("isJetDelivery")]
        public bool IsJetDelivery { get; set; }
    }

    public class OrderDetailCargoCompanyModel
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

    public class OrderDetailCommission
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [Range(0, 9999999999999999.99)]
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }

    public class OrderDetailNote
    {
    }

    public class OrderDetailHbDiscount
    {
        [JsonProperty("totalPrice")]
        public OrderDetailCommission TotalPrice { get; set; }

        [JsonProperty("unitPrice")]
        public OrderDetailCommission UnitPrice { get; set; }
    }

    public class OrderDetailSlot
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("timeslot")]
        public string Timeslot { get; set; }
    }

    public partial class Warehouse
    {
        [JsonProperty("shippingModel")]
        public string ShippingModel { get; set; }

        [JsonProperty("shippingAddressLabel")]
        public string ShippingAddressLabel { get; set; }
    }
}
