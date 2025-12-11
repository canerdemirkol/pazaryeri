using Newtonsoft.Json;
using static OBase.Pazaryeri.Domain.Helper.ConverterHelper;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEFailedResponseDto
    {
        [JsonProperty("statusCode")]
        public long StatusCode { get; set; }

        [JsonProperty("code")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Code { get; set; }

        [JsonProperty("defaultMessage")]
        public object DefaultMessage { get; set; }

        [JsonProperty("correlationId")]
        public Guid CorrelationId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}