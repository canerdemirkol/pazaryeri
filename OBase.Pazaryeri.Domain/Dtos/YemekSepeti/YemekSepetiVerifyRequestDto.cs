using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{
    public class YemekSepetiVerifyRequestDto
    {

        [JsonPropertyName("job_id")]
        public int JobId { get; set; }
        [JsonPropertyName("platform_vendor_id")]
        public string PlatformVendorId { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("download_url")]
        public string DownloadUrl { get; set; }
    }
}
