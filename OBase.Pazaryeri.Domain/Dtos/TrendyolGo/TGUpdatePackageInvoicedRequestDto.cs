using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
    public class TGUpdatePackageInvoicedRequestDto
    {
        [JsonProperty("invoiceAmount")]
        public decimal? InvoiceAmount { get; set; }

        [JsonProperty("bagCount")]
        public int? BagCount { get; set; }

        [JsonProperty("receiptLink")]
        public string ReceiptLink { get; set; }
    }
}