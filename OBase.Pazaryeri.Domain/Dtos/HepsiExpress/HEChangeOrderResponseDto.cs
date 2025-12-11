using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEChangeOrderResponseDto
    {
        [JsonProperty("newLineItemId")]
        public string NewLineItemId { get; set; }
    }
}
