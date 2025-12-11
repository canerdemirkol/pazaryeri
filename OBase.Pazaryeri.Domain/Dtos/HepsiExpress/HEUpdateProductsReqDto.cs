using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEUpdateProductsReqDto
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
        [XmlElement(ElementName = "listing")]
        public List<HEUpdateProductDto> requestItems { get; set; }

        [XmlElement(ElementName = "listings")]
        public Listings requestItem { get; set; }
        public HEListingDiscountRequestDto requestListingDiscount { get; set; }
        #endregion 
    }
}
