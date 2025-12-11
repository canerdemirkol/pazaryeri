using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.Trendyol
{
    public class TrendyolPushPriceStockResponseDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("data")]
        public string Data { get; set; }
        [JsonPropertyName("batchRequestId")]
        public string BatchRequestId { get; set; }
    }
}
