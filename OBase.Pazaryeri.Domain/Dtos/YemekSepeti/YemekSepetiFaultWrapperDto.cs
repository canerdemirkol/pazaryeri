using System.Net;
using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{
    public class YemekSepetiFaultWrapperDto
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
