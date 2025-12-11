using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Idefix.Shipment
{
    public  class UpdateShipmentStatusRequest
    {
        // "picking" veya "invoiced"
        [JsonProperty("status")]
        public string Status { get; set; }

        // Fatura numarası: "invoiced" statüsünde gönderilmesi beklenir
        [JsonProperty("invoiceNumber")]
        public string? InvoiceNumber { get; set; }
    }

}
