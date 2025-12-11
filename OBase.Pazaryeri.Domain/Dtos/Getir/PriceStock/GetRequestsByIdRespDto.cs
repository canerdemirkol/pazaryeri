using Newtonsoft.Json;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.PriceStock
{
    public class GetRequestsByIdRespDto
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("ErrorsMessages")]
        public List<string> ErrorMessages { get; set; }
        [JsonProperty("Errors")]
        public List<BaseRequestIdResponseItem> RequestItems { get; set; } = new List<BaseRequestIdResponseItem>();
        [JsonProperty("processed")]
        public int processed { get; set; }
        [JsonProperty("total")]
        public int total { get; set; }
        [JsonProperty("createdAt")]
        public DateTime createdAt { get; set; }
        [JsonIgnore]
        public int RefId { get; set; }
        [JsonIgnore]
        public long DetailId { get; set; }
        [JsonIgnore]
        public int Thread_No { get; set; }
    }
}