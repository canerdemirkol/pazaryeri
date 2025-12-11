using System.Net;
using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.Trendyol
{
    public class TrendyolPushPriceStockRequestMainDto
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


        #region JsonProperty
        [JsonPropertyName("requestItems")]
        public TrendyolPushPriceStockRequestDto RequestItem { get; set; }
        #endregion
    }
}
