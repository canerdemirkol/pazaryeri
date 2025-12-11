using Newtonsoft.Json;
using OBase.Pazaryeri.Domain.Helper;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HECompleteOrderRequestDto
    {
        public HECompleteOrderRequestDto()
        {
            LineItemRequests = new List<LineItemRequest>();
        }
        [JsonProperty("parcelQuantity")]
        public long? ParcelQuantity { get; set; }

        [JsonProperty("deci")]
        public long? Deci { get; set; }

        [JsonProperty("cargoCompany")]
        public string CargoCompany { get; set; }

        [JsonProperty("carrier")]
        public string Carrier { get; set; }

        [JsonProperty("lineItemRequests")]
        public List<LineItemRequest> LineItemRequests { get; set; }
    }

    public partial class LineItemRequest
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("quantity")]
        [JsonConverter(typeof(ConverterHelper.ParseStringConverter))]
        public decimal Quantity { get; set; }
    }
}