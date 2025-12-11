using System.Text.Json.Serialization;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{

    public class YemekSepetiOrderDto : BaseOrderDto
    {
        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        [JsonPropertyName("external_order_id")]
        public string ExternalOrderId { get; set; }

        [JsonPropertyName("order_code")]
        public string OrderCode { get; set; }

        [JsonPropertyName("client")]
        public Client Client { get; set; }

        [JsonPropertyName("customer")]
        public Customers Customer { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("items")]
        public Item[] Items { get; set; }

        [JsonPropertyName("order_type")]
        public string OrderType { get; set; }

        [JsonPropertyName("transport_type")]
        public string TransportType { get; set; }

        [JsonPropertyName("payment")]
        public Payment Payment { get; set; }

        [JsonPropertyName("cancellation")]
        public Cancellation Cancellation { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("sys")]
        public Sys Sys { get; set; }

        [JsonPropertyName("isPreorder")]
        public bool IsPreorder { get; set; }
    }

    public class Client
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("chain_id")]
        public string ChainId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("store_id")]
        public string StoreId { get; set; }

        [JsonPropertyName("external_partner_config_id")]
        public string ExternalPartnerConfigId { get; set; }
    }


    public class Customers
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("national_id")]
        public string NationalId { get; set; }

        [JsonPropertyName("tax_id")]
        public string TaxId { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("delivery_address")]
        public DeliveryAddress DeliveryAddress { get; set; }
    }

    public class DeliveryAddress
    {
        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("latitude")]
        public decimal? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public decimal? Longitude { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("block")]
        public string Block { get; set; }

        [JsonPropertyName("building")]
        public string Building { get; set; }

        [JsonPropertyName("apartment")]
        public string Apartment { get; set; }

        [JsonPropertyName("entrance")]
        public string Entrance { get; set; }

        [JsonPropertyName("intercom")]
        public string Intercom { get; set; }

        [JsonPropertyName("floor")]
        public string Floor { get; set; }

        [JsonPropertyName("suburb")]
        public string Suburb { get; set; }

        [JsonPropertyName("zipcode")]
        public string Zipcode { get; set; }

        [JsonPropertyName("instructions")]
        public string Instructions { get; set; }

        [JsonPropertyName("formattedAddress")]
        public string FormattedAddress { get; set; }
    }

    public class Payment
    {
        [JsonPropertyName("sub_total")]
        public float SubTotal { get; set; }

        [JsonPropertyName("order_total")]
        public float OrderTotal { get; set; }

        [JsonPropertyName("delivery_fee")]
        public int DeliveryFee { get; set; }

        [JsonPropertyName("service_fee")]
        public int ServiceFee { get; set; }

        [JsonPropertyName("difference_to_minimum")]
        public int DifferenceToMinimum { get; set; }

        [JsonPropertyName("discounts")]
        public object[] Discounts { get; set; }

        [JsonPropertyName("discount")]
        public int Discount { get; set; }

        [JsonPropertyName("container_charge")]
        public int ContainerCharge { get; set; }

        [JsonPropertyName("total_taxes")]
        public float TotalTaxes { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class Cancellation
    {
        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("cancelled_by")]
        public string CancelledBy { get; set; }
    }

    public class Sys
    {
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("created_by")]
        public string CreatedBy { get; set; }

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonPropertyName("updated_by")]
        public string UpdatedBy { get; set; }

        [JsonPropertyName("webhook_status")]
        public string WebhookStatus { get; set; }
    }

    public class Item
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("sku")]
        public string Sku { get; set; }

        [JsonPropertyName("barcode")]
        public string[]? Barcode { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("pricing")]
        public Pricing Pricing { get; set; }

        [JsonPropertyName("original_pricing")]
        public OriginalPricing OriginalPricing { get; set; }

        [JsonPropertyName("container_deposit")]
        public int? ContainerDeposit { get; set; }

        [JsonPropertyName("discount")]
        public int? Discount { get; set; }

        [JsonPropertyName("instructions")]
        public string Instructions { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
        
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }
    }

    public class Pricing
    {
        [JsonPropertyName("pricing_type")]
        public string PricingType { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("vat_percent")]
        public int? VatPercent { get; set; }

        [JsonPropertyName("total_price")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("min_quantity")]
        public decimal MinQuantity { get; set; }

        [JsonPropertyName("max_quantity")]
        public decimal MaxQuantity { get; set; }

        [JsonPropertyName("weight")]
        public decimal? Weight { get; set; }
    }

    public class OriginalPricing
    {
        [JsonPropertyName("pricing_type")]
        public string PricingType { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("vat_percent")]
        public int VatPercent { get; set; }

        [JsonPropertyName("total_price")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("min_quantity")]
        public decimal MinQuantity { get; set; }

        [JsonPropertyName("max_quantity")]
        public decimal MaxQuantity { get; set; }

        [JsonPropertyName("weight")]
        public float? Weight { get; set; }
    }
}