using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
    public class TGGetBatchRequestResultDto
    {
        public class Image
        {
            [JsonProperty("url")]
            public string Url { get; set; }
        }

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
            [JsonProperty("brand")]
            public string Brand { get; set; }

            [JsonProperty("productBarcode")]
            public string ProductBarcode { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("categoryName")]
            public string CategoryName { get; set; }

            [JsonProperty("listPrice")]
            public double? ListPrice { get; set; }

            [JsonProperty("salePrice")]
            public double? SalePrice { get; set; }

            [JsonProperty("vatRate")]
            public int VatRate { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("stockCode")]
            public string StockCode { get; set; }

            [JsonProperty("images")]
            public List<Image> Images { get; set; }

            [JsonProperty("productMainId")]
            public string ProductMainId { get; set; }

            [JsonProperty("dimensionalWeight")]
            public int DimensionalWeight { get; set; }

            [JsonProperty("attributes")]
            public List<object> Attributes { get; set; }
        }

        public class RequestItem
        {
            [JsonProperty("product")]
            public Product Product { get; set; }
        }

        public class Item
        {
            [JsonProperty("requestItem")]
            public RequestItem RequestItem { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("failureReasons")]
            public List<object> FailureReasons { get; set; }
        }

        public class Root
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

            [JsonProperty("items")]
            public List<Item> Items { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("creationDate")]
            public long? CreationDate { get; set; }

            [JsonProperty("lastModification")]
            public long? LastModification { get; set; }

            [JsonProperty("sourceType")]
            public string SourceType { get; set; }

            [JsonProperty("itemCount")]
            public int? ItemCount { get; set; }
        }
    }
}
