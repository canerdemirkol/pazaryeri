using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{
    public class YemekSepetiPromotionResponseDto
    {
        [JsonPropertyName("job_id")]
        public string JobId { get; set; }  // başarılı response için

        [JsonPropertyName("job_status")]
        public string JobStatus { get; set; } // başarılı response için

        [JsonPropertyName("code")]
        public string Code { get; set; } // hata response için

        [JsonPropertyName("message")]
        public string Message { get; set; } // hata response için
    }
}
