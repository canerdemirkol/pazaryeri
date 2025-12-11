using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.Trendyol
{
    public class TrendyolPushPriceStockRequestItemDto
    {
        [JsonIgnore]
        public long RefId { get; set; }
        
        [JsonIgnore]
        public long DetailId { get; set; }
        
        [JsonIgnore]
        public int Thread_No { get; set; }
       
        [JsonPropertyName("barcode")]
        public string Barcode { get; set; }
        
        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }

        [JsonPropertyName("salePrice")]
        public double? SalePrice { get; set; }
        
        [JsonPropertyName("listPrice")]
        public double? ListPrice { get; set; }

        [JsonPropertyName("sellingPrice")]
        public double? SellingPrice { get; set; }
        
        [JsonPropertyName("originalPrice")]
        public double? OriginalPrice { get; set; }

        [JsonPropertyName("storeId")]
        public string StoreId { get; set; }
    }
}
