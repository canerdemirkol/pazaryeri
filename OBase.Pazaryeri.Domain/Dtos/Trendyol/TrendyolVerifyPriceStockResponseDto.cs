using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.Trendyol
{
    public class TrendyolVerifyPriceStockResponseDto
    {
        [JsonPropertyName("batchRequestId")]
        public string BatchRequestId { get; set; }

        [JsonPropertyName("items")]
        public List<TrendyolBatchStockPriceResponseItemDto> Items { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("creationDate")]
        public long CreationDate { get; set; }

        [JsonPropertyName("lastModification")]
        public long LastModification { get; set; }

        [JsonPropertyName("sourceType")]
        public string SourceType { get; set; }

        [JsonPropertyName("itemCount")]
        public int ItemCount { get; set; }

        #region Json Ignore
        [JsonIgnore]
        public long RefId { get; set; }
        [JsonIgnore]
        public long DetailId { get; set; }
        [JsonIgnore]
        public int Thread_No { get; set; }
        #endregion
    }
}
