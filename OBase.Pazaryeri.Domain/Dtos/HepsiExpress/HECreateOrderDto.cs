using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using  OBase.Pazaryeri.Domain.Helper;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HECreateOrderDto
    {
        [JsonProperty("birimNo")]
        [DefaultValue("")]
        public string BirimNo { get; set; }

        [JsonProperty("customerId")]
        public Guid CustomerId { get; set; }

        [JsonProperty("cargoFirmId")]
        [JsonConverter(typeof(ConverterHelper.ParseStringConverter))]
        public long CargoFirmId { get; set; }

        [JsonProperty("lineItems")]
        public LineItem[] LineItems { get; set; }
    }

    public partial class LineItem
    {
        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }
    }
}
