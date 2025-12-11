using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
    public class TGGetInstantCallRequestDto
    {
        [JsonProperty("sellerPhoneNumber")]
        public string sellerPhoneNumber { get; set; }
    }

}