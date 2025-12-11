using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
    public class TGUpdateStockAndPriceReqDto
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
        public RequestItems TyGoRequestItems { get; set; }
        #region JsonProperty
        public class RequestItems
        {
			[JsonProperty("items")]
			public List<TGUpdateStockAndPriceDto> Items { get; set; }

		}
		#endregion
	}
}
