using Newtonsoft.Json;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{

    public class TrendyolGoOrderDto : BaseOrderDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("sellerId")]
        public int SellerId { get; set; }

        [JsonProperty("storeId")]
        public string StoreId { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("packageStatus")]
        public string PackageStatus { get; set; }

        [JsonProperty("deliveryModel")]
        public string DeliveryModel { get; set; }

        [JsonProperty("zoneId")]
        public string ZoneId { get; set; }

        [JsonProperty("scheduleType")]
        public string ScheduleType { get; set; }

        [JsonProperty("timeSlotId")]
        public string TimeSlotId { get; set; }

        [JsonProperty("eta")]
        public string Eta { get; set; }

        [JsonProperty("estimatedDeliveryStartDate")]
        public long EstimatedDeliveryStartDate { get; set; }

        [JsonProperty("estimatedDeliveryEndDate")]
        public long EstimatedDeliveryEndDate { get; set; }

        [JsonProperty("shipmentAddress")]
        public ShipmentAddress ShipmentAddress { get; set; }

        [JsonProperty("invoiceAddress")]
        public InvoiceAddress InvoiceAddress { get; set; }

        [JsonProperty("cargoInfo")]
        public CargoInfo CargoInfo { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("grossAmount")]
        public double GrossAmount { get; set; }

        [JsonProperty("totalDiscount")]
        public double TotalDiscount { get; set; }

        [JsonProperty("totalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonProperty("lines")]
        public List<Line> Lines { get; set; }

        [JsonProperty("orderDate")]
        public long OrderDate { get; set; }

        [JsonProperty("lastModifiedDate")]
        public long LastModifiedDate { get; set; }

        [JsonProperty("totalCargo")]
        public decimal? TotalCargo { get; set; }
        [JsonIgnore]
        public string CargoProductCode { get; set; }
    }
    public class Customer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class ShipmentAddress
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("cityCode")]
        public int CityCode { get; set; }

        [JsonProperty("cityId")]
        public int CityId { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("districtId")]
        public int DistrictId { get; set; }

        [JsonProperty("neighborhoodId")]
        public int NeighborhoodId { get; set; }

        [JsonProperty("neighborhood")]
        public string Neighborhood { get; set; }

        [JsonProperty("apartmentNumber")]
        public string ApartmentNumber { get; set; }

        [JsonProperty("floor")]
        public string Floor { get; set; }

        [JsonProperty("doorNumber")]
        public string DoorNumber { get; set; }

        [JsonProperty("addressDescription")]
        public string AddressDescription { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("identityNumber")]
        public string IdentityNumber { get; set; }
    }

    public class InvoiceAddress
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("cityCode")]
        public int CityCode { get; set; }

        [JsonProperty("cityId")]
        public int CityId { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("districtId")]
        public int DistrictId { get; set; }

        [JsonProperty("neighborhoodId")]
        public int NeighborhoodId { get; set; }

        [JsonProperty("neighborhood")]
        public string Neighborhood { get; set; }

        [JsonProperty("apartmentNumber")]
        public string ApartmentNumber { get; set; }

        [JsonProperty("floor")]
        public string Floor { get; set; }

        [JsonProperty("doorNumber")]
        public string DoorNumber { get; set; }

        [JsonProperty("addressDescription")]
        public string AddressDescription { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("identityNumber")]
        public string IdentityNumber { get; set; }
    }

    public class CargoInfo
    {
        [JsonProperty("trackingLink")]
        public string TrackingLink { get; set; }

        [JsonProperty("providerName")]
        public string ProviderName { get; set; }
    }
    public class Line
    {
        [JsonProperty("salesCampaignId")]
        public int SalesCampaignId { get; set; }

        [JsonProperty("merchantSku")]
        public string MerchantSku { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("barcode")]
        public string Barcode { get; set; }

        [JsonProperty("vatBaseAmount")]
        public decimal VatBaseAmount { get; set; }

        [JsonProperty("vatRatio")]
        public int VatRatio { get; set; }

        [JsonProperty("product")]
        public Product Product { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }

    public class Product
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("brandName")]
        public string BrandName { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("imageUrls")]
        public List<string> ImageUrls { get; set; }
    }

    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("packageItemId")]
        public string PackageItemId { get; set; }

        [JsonProperty("isCancelled")]
        public bool IsCancelled { get; set; }
        [JsonProperty("isAlternative")]
        public bool IsAlternative { get; set; }
        [JsonProperty("isCollected")]
        public bool IsCollected { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("discount")]
        public decimal Discount { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; } = 1;

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("discountAmount")]
        public decimal DiscountAmount { get; set; }

        [JsonProperty("kdvTutar")]
        public decimal KdvTutar { get; set; }

        [JsonProperty("paraBirimiKodu")]
        public string ParaBirimiKodu { get; set; }
    }
}