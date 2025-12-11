using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEListingDiscountRequestDto
    {
        [JsonIgnore]
        public long DetailId { get; set; }

        [JsonIgnore]
        public long RefId { get; set; }

        [JsonProperty("discountName")]
        public string DiscountName { get; set; }

        [JsonProperty("discountAmount")]
        public double DiscountAmount { get; set; }

        [JsonProperty("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        [JsonProperty("includedSkus")]
        public List<string> IncludedSkus { get; set; }

        [JsonProperty("isPaused")]
        public bool? IsPaused { get; set; }
    }
}