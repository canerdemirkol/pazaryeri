using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.Idefix.Unsupplied
{
    public class UnsuppliedItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("reasonId")]
        public int ReasonId { get; set; }
    }
}
