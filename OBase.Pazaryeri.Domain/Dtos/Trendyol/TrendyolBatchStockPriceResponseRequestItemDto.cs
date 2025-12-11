using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.Trendyol
{
    public class TrendyolBatchStockPriceResponseRequestItemDto
    {
        #region Json Ignore
        [JsonIgnore]
        public long RefId { get; set; }
        [JsonIgnore]
        public long DetailId { get; set; }
        [JsonIgnore]
        public int Thread_No { get; set; }
        #endregion

        [JsonPropertyName("barcode")]
        public string Barcode { get; set; }

        [JsonPropertyName("listPrice")]
        public double ListPrice { get; set; }

        [JsonPropertyName("salePrice")]
        public double SalePrice { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("storeId")]
        public string StoreId { get; set; }
    }
}
