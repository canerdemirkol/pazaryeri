using Newtonsoft.Json;
using OBase.Pazaryeri.Domain.Dtos.Idefix.PriceStock;

namespace OBase.Pazaryeri.Domain.Dtos.Idefix
{
    public class IdefixGenericResponse<TItem> : IdefixBaseResponse
    {
        [JsonProperty("items")]
        public List<TItem> Items { get; set; }

        [JsonIgnore]
        public long RefId { get; set; }
        [JsonIgnore]
        public long DetailId { get; set; }
        [JsonIgnore]
        public int Thread_No { get; set; }
    }
    public class IdefixErrorResponse
    {
        [JsonIgnore]
        public bool Success { get; set; } = false;
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
