using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
    public class TGGetRequestsByIdRespDto: ObaseUpdateProductDto
    {
        [JsonProperty("message")]
        public string message { get; set; }
   
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("ErrorsMessages")]
        public List<string> errorMessages { get; set; }

        [JsonProperty("Errors")]
        public List<BaseRequestIdResponseItem> requestItems { get; set; } = new List<BaseRequestIdResponseItem>();

        [JsonProperty("processed")]
        public int processed { get; set; }

        [JsonProperty("total")]
        public int total { get; set; }

        [JsonProperty("createdAt")]
        public DateTime createdAt { get; set; }
    }   
}