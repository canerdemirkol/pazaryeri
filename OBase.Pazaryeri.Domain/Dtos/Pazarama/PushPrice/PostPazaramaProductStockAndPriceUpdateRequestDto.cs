using Newtonsoft.Json;
using System.Net;

namespace OBase.Pazaryeri.Domain.Dtos.Pazarama.PushPrice
{
    public class PostPazaramaProductStockAndPriceUpdateRequestDto
    {
        public class Item
        {
            [JsonIgnore]
            public long RefId { get; set; }
            [JsonIgnore]
            public long DetailId { get; set; }
            [JsonIgnore]
            public int Thread_No { get; set; }
            [JsonIgnore]
            public string ProductId { get; set; }
            [JsonIgnore]
            public string MerchantBranchId { get; set; }
            [JsonIgnore]
            public string MerchantSKU { get; set; }
            [JsonIgnore]
            public int Quantity { get; set; }
            [JsonIgnore]
            public int? MinBuyingQuantity { get; set; }
            [JsonIgnore]
            public decimal? MaxBuyingQuantity { get; set; }
            [JsonIgnore]
            public decimal? QuantityIncrease { get; set; }
            [JsonIgnore]
            public double? ListingPrice { get; set; }
            [JsonIgnore]
            public double? SellingPrice { get; set; }
            public double? ListPrice { get; set; }
            public double? SelPrice { get; set; }
            public string Code { get; set; }           
            public int StockCount { get; set; }
        }
        public class Root
        {
            public List<Item> Items { get; set; }
        }

        #region JsonIgnore
        [JsonIgnore]
        public string GuidStok { get; set; }
        [JsonIgnore]
        public string GuidPrice { get; set; }
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
        public Root Items { get; set; }
        #endregion 

    }
}
