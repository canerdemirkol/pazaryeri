using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEEndUserCancellationModel
    {
        [JsonProperty("cancelDate")]
        public DateTimeOffset CancelDate { get; set; }

        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("cancelledBy")]
        public string CancelledBy { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("cancelReasonCode")]
        public string CancelReasonCode { get; set; }
    }
}
