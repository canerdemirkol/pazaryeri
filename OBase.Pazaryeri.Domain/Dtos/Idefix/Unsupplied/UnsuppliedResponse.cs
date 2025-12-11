using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.Idefix.Unsupplied
{
    public class UnsuppliedResponse
    {
        [JsonProperty("unsupplied")]
        public bool Unsupplied { get; set; }

        [JsonProperty("items")]
        public List<UnsuppliedItem> Items { get; set; } = new List<UnsuppliedItem>();

    }
}
