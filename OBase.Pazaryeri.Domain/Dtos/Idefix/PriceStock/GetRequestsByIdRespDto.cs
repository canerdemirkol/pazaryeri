using Newtonsoft.Json;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;

namespace OBase.Pazaryeri.Domain.Dtos.Idefix.PriceStock
{
    public class ProductInventoryItemResponse : InventoryItem
    {

        [JsonProperty("status")]
        public string Status { get; set; }
    }
    public class ProductInventoryItemWithBatchRequesIdResponse : ProductInventoryItemResponse
    {

        [JsonProperty("statusDateCreatedAt")]
        public DateTime StatusDateCreatedAt { get; set; }

        [JsonProperty("failureReasons")]
        public string FailureReasons { get; set; }
    }
}