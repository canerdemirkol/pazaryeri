using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEGetRequestResponseDto:ObaseUpdateProductDto
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
        public List<RequestIdResponseItem> RequestItems { get; set; }=new List<RequestIdResponseItem>();

        [JsonProperty("processed")]
        public int processed { get; set; }
        [JsonProperty("total")]
        public int total { get; set; }

        [JsonProperty("createdAt")]
        public DateTime createdAt { get; set; }
     
    }

    public class ResponseData
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("requestItems")]
        public List<RequestIdResponseItem> RequestItems { get; set; }

        [JsonProperty("createdDatetime")]
        public DateTime CreatedDatetime { get; set; }
    }

    public class RequestIdResponseItem : HEUpdateProductDto
    {
        [JsonProperty("Error")]
        public List<HEUpdateProductDto> ErrorsObject { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
        [JsonProperty("Errors")]
        public List<string> ErrorMessages { get; set; }
    }
}