using Newtonsoft.Json;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEComplateSuccessOrderDto
    {
        [JsonProperty("packageNumber")]
        public string PackageNumber { get; set; }

        [JsonProperty("barcode")]
        public string Barcode { get; set; }
    }
}