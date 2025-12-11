using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.Trendyol
{
    public class TrendyolBatchStockPriceResponseItemDto
    {
        [JsonPropertyName("requestItem")]
        public TrendyolBatchStockPriceResponseRequestItemDto RequestItem { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("failureReasons")]
        public List<object> FailureReasons { get; set; }
    }
}
