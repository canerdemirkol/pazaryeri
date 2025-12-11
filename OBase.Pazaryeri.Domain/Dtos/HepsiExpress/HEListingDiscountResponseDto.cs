using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEListingDiscountResponseDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("campaignId")]
        public int CampaignId { get; set; }
    }
}
