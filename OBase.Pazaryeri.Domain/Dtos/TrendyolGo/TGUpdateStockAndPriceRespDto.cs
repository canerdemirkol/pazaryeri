using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
    public class TGUpdateStockAndPriceRespDto
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("data")]
        public string Data { get; set; }
        [JsonProperty("batchRequestId")]
        public string BatchRequestId { get; set; }
    }
}
