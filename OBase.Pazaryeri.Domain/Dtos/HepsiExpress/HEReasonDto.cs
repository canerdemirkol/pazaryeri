using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEReasonDto
    {
        [JsonPropertyName("reasonId")]
        public string ReasonId { get; set; }
    }
}