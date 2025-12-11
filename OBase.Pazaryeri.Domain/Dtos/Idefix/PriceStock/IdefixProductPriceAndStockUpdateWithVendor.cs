using Newtonsoft.Json;
using OBase.Pazaryeri.Domain.Dtos.Idefix.PriceStock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.İdefix.PriceStock
{
    public class IdefixProductPriceAndStockUpdateWithVendor
    {
        public IdefixProductPriceAndStockUpdateWithVendor()
        {
            requestItem = new IdefixProductPriceAndStockUpdateWithVendorRequestDto();
          
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
        [JsonIgnore]
        public string ResultException { get; set; }
        [JsonIgnore]
        public int DetailsCount { get; set; }
        [JsonIgnore]
        public int Thread_No { get; set; }
        #endregion


        [JsonProperty("requestItem")]
        public IdefixProductPriceAndStockUpdateWithVendorRequestDto requestItem { get; set; }
    }
}
