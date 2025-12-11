using System.Net;
using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.CommonDtos
{
    public class ApinizerFaultWrapperDto
    {
        [JsonPropertyName("fault")]
        public ApinizerFaultResponseDto? Fault { get; set; }
    }

    public class ApinizerFaultResponseDto
    {
        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; set; }
        
        [JsonPropertyName("faultCode")]
        public string? FaultCode { get; set; }
        
        [JsonPropertyName("faultString")]
        public string? FaultString { get; set; }
        
        [JsonPropertyName("faultStatusCode")]
        public string? FaultStatusCode { get; set; }

        [JsonPropertyName("responseFromApi")]
        public string? ResponseFromApi { get; set; }
    }
}
