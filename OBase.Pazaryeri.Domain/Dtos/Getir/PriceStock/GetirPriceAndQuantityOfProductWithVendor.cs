using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.PriceStock
{
    public class GetirPriceAndQuantityOfProductWithVendor
    {
        public GetirPriceAndQuantityOfProductWithVendor()
        {
            requestItems = new List<GetirPriceAndQuantityOfProductWithVendorReqDto.Root>();
            requestItem = new GetirPriceAndQuantityOfProductWithVendorReqDto.Root();
        }

        #region JsonIgnore
        [JsonIgnore]
        public string Guid { get; set; }
        [JsonIgnore]
        public HttpStatusCode HttpStatusCode { get; set; }
        [JsonIgnore]
        public string APIResponse { get; set; }
        [JsonIgnore]
        public bool RequestFailed { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public string ResultException { get; set; }
        [JsonIgnore]
        public int DetailsCount { get; set; }
        [JsonIgnore]
        public int Thread_No { get; set; }
        #endregion

        [JsonProperty("requestItemss")]
        public List<GetirPriceAndQuantityOfProductWithVendorReqDto.Root> requestItems { get; set; }

        [JsonProperty("requestItems")]
        public GetirPriceAndQuantityOfProductWithVendorReqDto.Root requestItem { get; set; }
    }
}
