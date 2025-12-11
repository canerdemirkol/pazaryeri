using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos
{
    public class BaseRequestIdResponseItem : ObaseUpdateProductDto
    {
        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("Errors")]
        public List<string> ErrorMessages { get; set; }
    }
}