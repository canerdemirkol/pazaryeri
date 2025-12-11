using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEDeliverRequestDto
    {
        public class Root
        {
            [JsonProperty("receivedDate")]
            public DateTime receivedDate { get; set; }

            [JsonProperty("receivedBy")]
            public string receivedBy { get; set; }
        }
    }
}