using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEPutUpdatePackageResponseDto
    {
        public class Root
        {
            [JsonPropertyName("invoiceAmount")]
            public double? InvoiceAmount { get; set; }

            [JsonPropertyName("bagCount")]
            public int? BagCount { get; set; }

            [JsonPropertyName("receiptLink")]
            public string ReceiptLink { get; set; }
        }
    }
}