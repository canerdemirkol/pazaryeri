using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{
    public class YemekSepetiPushPriceStockRequestProductDto
    {
        [JsonIgnore]
        public long RefId { get; set; }
        [JsonIgnore]
        public long DetailId { get; set; }
        [JsonIgnore]
        public int Thread_No { get; set; }
        public string Sku { get; set; }
        //public string Barcode { get; set; }
        public bool Active { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
}
