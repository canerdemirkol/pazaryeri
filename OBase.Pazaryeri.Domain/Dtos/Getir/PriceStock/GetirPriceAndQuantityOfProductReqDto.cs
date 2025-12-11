using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Getir.PriceStock
{
    public class GetirPriceAndQuantityOfProductReqDto
    {
        public class Product
        {
            [JsonIgnore]
            public int RefId { get; set; }
            [JsonIgnore]
            public long DetailId { get; set; }
            [JsonIgnore]
            public int Thread_No { get; set; }
            [JsonProperty("getirId")]
            public string GetirId { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("price")]
            public double Price { get; set; }

            [JsonProperty("oldPrice")]
            public double? OldPrice { get; set; }

            [JsonProperty("shopId")]
            public string ShopId { get; set; }
        }

        public class Root
        {
            [JsonProperty("products")]
            public List<Product> Products { get; set; }
        }
    }
}
