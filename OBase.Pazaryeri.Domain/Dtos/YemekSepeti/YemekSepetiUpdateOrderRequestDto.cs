using AutoMapper.Configuration.Annotations;
using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{
    public class YemekSepetiUpdateOrderRequestDto
    {
        [JsonProperty("cancellation")]
        public Cancellations Cancellation { get; set; }

        [JsonProperty("items")]
        public List<OrderItem> Items { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class OrderItem
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pricing")]
        public OrderPricing Pricing { get; set; }

        [JsonProperty("replaced_id")]
        public string ReplacedId { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class OrderPricing
    {
        [JsonProperty("pricing_type")]       
        public string PricingType { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("total_price")]
        public decimal? TotalPrice { get; set; }

        [JsonProperty("weight")]
        public decimal Weight { get; set; }
    }

    public class Cancellations
    {
        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}