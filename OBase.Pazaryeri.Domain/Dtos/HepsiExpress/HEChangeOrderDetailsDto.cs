using Newtonsoft.Json;
using System.ComponentModel;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEChangeOrderDetailsDto
    {
        [JsonProperty("newGram")]
        public long NewGram { get; set; }

        [JsonProperty("newMerchantListingPrice")]
        public NewMerchantListingPrice NewMerchantListingPrice { get; set; }

        [JsonProperty("newQuantity")]
        public long NewQuantity { get; set; }

        [JsonProperty("newSku")]
        [DefaultValue("")]
        public string NewSku { get; set; } = "";

        [JsonProperty("newMerchantSku")]
        [DefaultValue("")]
        public string NewMerchantSku { get; set; } = "";

        [JsonProperty("newLineItemId")]
        [DefaultValue("")]
        public string NewLineItemId { get; set; } = "";
    }

    public partial class NewMerchantListingPrice
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        [DefaultValue("")]
        public string Currency { get; set; } = "";
    }
}