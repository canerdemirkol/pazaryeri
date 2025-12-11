using System.Net;
using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{
    public class YemekSepetiPriceStockRequestDto
    {
        #region JsonIgnore
        [JsonIgnore]
        public string Guid { get; set; }
        [JsonIgnore]
        public HttpStatusCode HttpStatusCode { get; set; }
        [JsonIgnore]
        public string APIResponse { get; set; }
        [JsonIgnore]
        public bool RequestFailed { get; set; }
        [JsonIgnore]
        public string ResultException { get; set; }
        [JsonIgnore]
        public int DetailsCount { get; set; }
        [JsonIgnore]
        public int Thread_No { get; set; }
        #endregion
        public List<YemekSepetiPushPriceStockRequestProductDto> Products { get; set; }
    }
}
