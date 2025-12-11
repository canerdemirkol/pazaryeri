using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
    public class TGUpdateStockAndPriceDto
    {

        [JsonIgnore]
        public long RefId { get; set; }
        [JsonIgnore]
        public long DetailId { get; set; }
        [JsonIgnore]
        public int Thread_No { get; set; }
        [JsonProperty("barcode")]
        public string barcode { get; set; }
        [JsonProperty("quantity")]
        public int quantity { get; set; }
        [JsonProperty("sellingPrice")]
        public double sellingPrice { get; set; }
        [JsonProperty("originalPrice")]
        public double originalPrice { get; set; }
        [JsonProperty("storeId")]
        public string storeId { get; set; }
    }
}
