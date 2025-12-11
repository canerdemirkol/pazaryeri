using Newtonsoft.Json;
namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEUpdateProductsRespDto
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public ResponseData Data { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

    }
}