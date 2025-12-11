using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
    public class TGUpdatePackageUnSuppliedRequestDto
    {
        [JsonProperty("itemIdList")]
        public string[] ItemIdList { get; set; }

        [JsonProperty("causedCancelPackageItemIds")]
        public string[] CausedCancelPackageItemIds { get; set; }

        [JsonProperty("reasonId")]
        public int ReasonId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}