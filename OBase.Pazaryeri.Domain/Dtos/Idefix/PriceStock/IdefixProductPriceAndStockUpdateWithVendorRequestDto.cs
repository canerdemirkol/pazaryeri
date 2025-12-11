using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.Idefix.PriceStock
{
    public class IdefixProductPriceAndStockUpdateWithVendorRequestDto
    {
        [JsonProperty("items")]
        public List<InventoryItem> Items { get; set; }
    }

    public class InventoryItem
    {

        [JsonIgnore]
        public long RefId { get; set; }
        [JsonIgnore]
        public long DetailId { get; set; }
        [JsonIgnore]
        public int Thread_No { get; set; }

        [JsonIgnore]
        public List<string> ErrorMessages { get; set; }
        /// <summary>
        /// Ürün barkodu
        /// </summary>
        [JsonProperty("barcode")]
        public string Barcode { get; set; }

        /// <summary>
        /// Ürünün satış fiyatı (kuruş bazlı, örn: 300000 => 3000,00 TL)
        /// </summary>
        [JsonProperty("price")]
        public double Price { get; set; }

        /// <summary>
        /// Karşılaştırma fiyatı (indirimli fiyat gösterimi için)
        ///  Bu fiyatı göndermek istemiyorsaniz 0 olarak göndermelisiniz.
        /// </summary>
        [JsonProperty("comparePrice")]
        public double ComparePrice { get; set; } = 0;

        /// <summary>
        /// Stok miktarı
        /// </summary>
        [JsonProperty("inventoryQuantity")]
        public int InventoryQuantity { get; set; }

        /// <summary>
        /// Teslimat süresi (gün cinsinden)
        /// </summary>
        [JsonProperty("deliveryDuration")]
        public int DeliveryDuration { get; set; }

        /// <summary>
        /// Teslimat tipi (örnek: "regular")
        /// </summary>
        [JsonProperty("deliveryType")]
        public string DeliveryType { get; set; }
    }
}
