using Newtonsoft.Json;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.Idefix
{

    public class IdefixOrderDto : BaseOrderDto
    {
        [JsonPropertyName("invoiceAddress")]
        public Address InvoiceAddress { get; set; }

        [JsonPropertyName("shippingAddress")]
        public Address ShippingAddress { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonPropertyName("totalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("totalDiscount")]
        public double TotalDiscount { get; set; }

        [JsonPropertyName("discountedTotalPrice")]
        public double DiscountedTotalPrice { get; set; }

        [JsonPropertyName("totalPlatformDiscount")]
        public double TotalPlatformDiscount { get; set; }

        [JsonPropertyName("totalVendorDiscount")]
        public double TotalVendorDiscount { get; set; }

        [JsonPropertyName("customerId")]
        public long CustomerId { get; set; }

        [JsonPropertyName("customerContactName")]
        public string CustomerContactName { get; set; }

        [JsonPropertyName("customerContactMail")]
        public string CustomerContactMail { get; set; }

        [JsonPropertyName("customerTcNumber")]
        public string CustomerTcNumber { get; set; }

        [JsonPropertyName("cargoTrackingNumber")]
        public string CargoTrackingNumber { get; set; }

        [JsonPropertyName("cargoTrackingUrl")]
        public string CargoTrackingUrl { get; set; }

        [JsonPropertyName("cargoCompany")]
        public string CargoCompany { get; set; }

        [JsonPropertyName("cargoTypeId")]
        public int CargoTypeId { get; set; }

        [JsonPropertyName("cargoTypeName")]
        public string CargoTypeName { get; set; }

        [JsonPropertyName("cargoProfileId")]
        public int CargoProfileId { get; set; }

        [JsonPropertyName("cargoProfileName")]
        public string CargoProfileName { get; set; }

        [JsonPropertyName("geoCode")]
        public string GeoCode { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("orderDate")]
        public DateTime OrderDate { get; set; }

        [JsonPropertyName("histories")]
        public List<History> Histories { get; set; } = new();

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("statusUpdatedAt")]
        public DateTime? StatusUpdatedAt { get; set; }

        [JsonPropertyName("deliveryType")]
        public string DeliveryType { get; set; }

        [JsonPropertyName("estimatedDeliveryDate")]
        public DateTime EstimatedDeliveryDate { get; set; }

        [JsonPropertyName("vendorTitle")]
        public string VendorTitle { get; set; }

        [JsonPropertyName("vendorId")]
        public int VendorId { get; set; }

        [JsonPropertyName("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonPropertyName("cargoKey")]
        public string CargoKey { get; set; }

        /// <summary>
        /// Product Items
        /// </summary>
        [JsonPropertyName("items")]        
        public List<ProductItem> Items { get; set; }
    }



    public class Address
    {
        [JsonPropertyName("isCommercial")]
        public string IsCommercial { get; set; }

        [JsonPropertyName("isLikeCommercial")]
        public string IsLikeCommercial { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("address1")]
        public string Address1 { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("cityId")]
        public int CityId { get; set; }

        [JsonPropertyName("cityPlate")]
        public int CityPlate { get; set; }

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonPropertyName("neighboorhood")]
        public string Neighboorhood { get; set; }

        [JsonPropertyName("neighboorhoodId")]
        public int NeighboorhoodId { get; set; }

        [JsonPropertyName("county")]
        public string County { get; set; }

        [JsonPropertyName("countyId")]
        public int CountyId { get; set; }

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("fullAddress")]
        public string FullAddress { get; set; }

        [JsonPropertyName("taxOffice")]
        public string TaxOffice { get; set; }

        [JsonPropertyName("taxNumber")]
        public string TaxNumber { get; set; }

        [JsonPropertyName("identificationNumber")]
        public string IdentificationNumber { get; set; }

        [JsonPropertyName("floor")]
        public string Floor { get; set; }

        [JsonPropertyName("buildingNumber")]
        public string BuildingNumber { get; set; }

        [JsonPropertyName("doorNumber")]
        public string DoorNumber { get; set; }
    }

    public class History
    {
        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class ProductItem
    {
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("barcode")]
        public string Barcode { get; set; }

        [JsonPropertyName("erpId")]
        public string ErpId { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("totalDiscount")]
        public decimal? TotalDiscount { get; set; }

        [JsonPropertyName("platformDiscount")]
        public decimal PlatformDiscount { get; set; }

        [JsonPropertyName("vendorDiscount")]
        public decimal VendorDiscount { get; set; }

        [JsonPropertyName("discountedTotalPrice")]
        public decimal DiscountedTotalPrice { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("itemStatus")]
        public string ItemStatus { get; set; }

        [JsonPropertyName("comparePrice")]
        public decimal ComparePrice { get; set; }

        [JsonPropertyName("vendorAmount")]
        public decimal VendorAmount { get; set; }

        [JsonPropertyName("stateDescription")]
        public string StateDescription { get; set; }

        [JsonPropertyName("brandName")]
        public string BrandName { get; set; }

        [JsonPropertyName("merchantSku")]
        public string MerchantSku { get; set; }

        [JsonPropertyName("lastShipmentDate")]
        public DateTime LastShipmentDate { get; set; }

        [JsonPropertyName("VatRate")]
        public double VatRate { get; set; }

        [JsonPropertyName("commissionAmount")]
        public decimal CommissionAmount { get; set; }

        [JsonPropertyName("earningAmount")]
        public decimal EarningAmount { get; set; }

        [JsonPropertyName("withholdingAmount")]
        public decimal? WithholdingAmount { get; set; }

        [JsonPropertyName("customizableNote")]
        public string CustomizableNote { get; set; }

        [JsonPropertyName("interestPrice ")]
        public decimal? InterestPrice { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public decimal? Amount { get; set; }
    }

}