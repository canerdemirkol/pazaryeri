using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.PriceStock
{
    public class GetirGetUpdateProductsResultWithBatchRequesIdtRespDto
    {
        public class Product
        {
            #region Json Ignore
            [JsonIgnore]
            public long RefId { get; set; }
            [JsonIgnore]
            public long DetailId { get; set; }
            [JsonIgnore]
            public int Thread_No { get; set; }
            #endregion
            [JsonProperty("getirId")]
            public string GetirId { get; set; }

            [JsonProperty("vendorId")]
            public string VendorId { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("price")]
            public double Price { get; set; }

            [JsonProperty("oldPrice")]
            public double OldPrice { get; set; }

            [JsonProperty("shopId")]
            public string ShopId { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("errorMessage")]
            public string ErrorMessage { get; set; }
        }

        public class Data
        {
            #region Json Ignore
            [JsonIgnore]
            public long RefId { get; set; }
            [JsonIgnore]
            public long DetailId { get; set; }
            [JsonIgnore]
            public int Thread_No { get; set; }
            #endregion
            [JsonProperty("batchRequestId")]
            public string BatchRequestId { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("totalCount")]
            public int TotalCount { get; set; }

            [JsonProperty("products")]
            public List<Product> Products { get; set; }
        }

        public class Root
        {
            [JsonProperty("meta")]
            public Meta Meta { get; set; }

            [JsonProperty("data")]
            public Data Data { get; set; }
        }
    }
}
