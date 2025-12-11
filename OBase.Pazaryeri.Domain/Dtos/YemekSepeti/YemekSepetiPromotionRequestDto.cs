using System;
using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{
    public class YemekSepetiUpdatePromotionRequestDto
    {
        [JsonProperty("reason")]
        public string Reason { get; set; } = "COMPETITIVENESS"; // default

        [JsonProperty("active")]
        public bool Active { get; set; } = true;

        [JsonProperty("vendors")]
        public List<string> Vendors { get; set; } = new List<string>();

        [JsonProperty("type")]
        public string? Type { get; set; } // STRIKETHROUGH veya SAME_ITEM_BUNDLE

        [JsonProperty("display_name")]
        public Dictionary<string, string> DisplayName { get; set; } = new Dictionary<string, string>(); // "en_TR": 

        [JsonProperty("conditions")]
        public YemekSepetiPromotionCondition Conditions { get; set; } = new YemekSepetiPromotionCondition();

        [JsonProperty("discount")]
        public List<YemekSepetiPromotionDiscount> Discount { get; set; } = new List<YemekSepetiPromotionDiscount>();
    }
    public class YemekSepetiPromotionCondition
    {
        [JsonProperty("start_time")]
        public DateTime? StartTime { get; set; }

        [JsonProperty("end_time")]
        public DateTime? EndTime { get; set; }
    }

    public class YemekSepetiPromotionDiscount
    {
        [JsonProperty("discount_subtype")]
        public string? DiscountSubtype { get; set; } // PERCENTAGE veya diğer

        [JsonProperty("discount_value")]
        public decimal DiscountValue { get; set; }

        [JsonProperty("sku")]
        public List<string> Sku { get; set; } = new List<string>();
    }
    public enum YemekSepetiDiscountSubtype
    {
        [JsonProperty("PERCENTAGE")]
        PERCENTAGE,

        [JsonProperty("ABSOLUTE")]
        ABSOLUTE,

        [JsonProperty("FINAL_PRICE")]
        FINAL_PRICE
    }

}
