using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEInTransitRequestDto
    {
        public class Root
        {
            [JsonProperty("shippedDate")]
            public DateTime shippedDate { get; set; }

            [JsonProperty("estimatedArrivalDate")]
            public DateTime estimatedArrivalDate { get; set; }

            [JsonProperty("trackingUrl")]
            public string trackingUrl { get; set; }

            [JsonProperty("trackingNumber")]
            public string trackingNumber { get; set; }

            [JsonProperty("trackingPhoneNumber")]
            public string trackingPhoneNumber { get; set; }

            [JsonProperty("tax")]
            public int tax { get; set; }

            [JsonProperty("cost")]
            public int cost { get; set; }

            [JsonProperty("deci")]
            public int deci { get; set; }
        }
    }
}