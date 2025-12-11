using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.Getir
{
    public class GenericGetirResponse<T> : GetirResponse
    {
        public T Data { get; set; }

    }
    public class Meta
    {
        [JsonProperty("return-code")]
        public string returnCode { get; set; }

        [JsonProperty("return-message")]
        public string returnMessage { get; set; }
    }
}
