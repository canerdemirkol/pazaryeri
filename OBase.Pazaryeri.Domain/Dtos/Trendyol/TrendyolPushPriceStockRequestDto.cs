using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.Trendyol
{
    public class TrendyolPushPriceStockRequestDto
    {
        [JsonPropertyName("items")]
        public List<TrendyolPushPriceStockRequestItemDto> Items { get; set; }
    }
}
