using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.HepsiExpress
{
    public class HEUpdateProductRequestDto
    {
        public class Attribute
        {
            [JsonPropertyName("attributeId")]
            public int AttributeId { get; set; }

            [JsonPropertyName("attributeName")]
            public string AttributeName { get; set; }

            [JsonPropertyName("attributeValue")]
            public string AttributeValue { get; set; }

            [JsonPropertyName("attributeValueId")]
            public int AttributeValueId { get; set; }
        }

        public class Image
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }
        }

        public class Item
        {
            [JsonPropertyName("approved")]
            public bool Approved { get; set; }

            [JsonPropertyName("attributes")]
            public List<Attribute> Attributes { get; set; }

            [JsonPropertyName("barcode")]
            public string Barcode { get; set; }

            [JsonPropertyName("batchRequestId")]
            public string BatchRequestId { get; set; }

            [JsonPropertyName("brand")]
            public string Brand { get; set; }

            [JsonPropertyName("brandId")]
            public int BrandId { get; set; }
            [JsonPropertyName("categoryId")]
            public int CategoryId { get; set; }

            [JsonPropertyName("categoryName")]
            public string CategoryName { get; set; }

            [JsonPropertyName("createDateTime")]
            public long CreateDateTime { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("hasActiveCampaign")]
            public bool HasActiveCampaign { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("images")]
            public List<Image> Images { get; set; }

            [JsonPropertyName("lastPriceChangeDate")]
            public long LastPriceChangeDate { get; set; }

            [JsonPropertyName("lastStockChangeDate")]
            public long LastStockChangeDate { get; set; }

            [JsonPropertyName("lastUpdateDate")]
            public long LastUpdateDate { get; set; }

            [JsonPropertyName("listPrice")]
            public double ListPrice { get; set; }

            [JsonPropertyName("locked")]
            public bool Locked { get; set; }

            [JsonPropertyName("onSale")]
            public bool OnSale { get; set; }

            [JsonPropertyName("pimCategoryId")]
            public int PimCategoryId { get; set; }

            [JsonPropertyName("platformListingId")]
            public string PlatformListingId { get; set; }

            [JsonPropertyName("productCode")]
            public int ProductCode { get; set; }

            [JsonPropertyName("productContentId")]
            public int ProductContentId { get; set; }

            [JsonPropertyName("productMainId")]
            public string ProductMainId { get; set; }

            [JsonPropertyName("quantity")]
            public int Quantity { get; set; }

            [JsonPropertyName("salePrice")]
            public double SalePrice { get; set; }

            [JsonPropertyName("stockUnitType")]
            public string StockUnitType { get; set; }

            [JsonPropertyName("supplierId")]
            public int SupplierId { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("vatRate")]
            public int VatRate { get; set; }

            [JsonPropertyName("version")]
            public int Version { get; set; }

            [JsonPropertyName("categoryMaxPrice")]
            public int CategoryMaxPrice { get; set; }

            [JsonPropertyName("categoryMinPrice")]
            public double CategoryMinPrice { get; set; }

            [JsonPropertyName("rejected")]
            public bool Rejected { get; set; }

            [JsonPropertyName("rejectReasonDetails")]
            public List<object> RejectReasonDetails { get; set; }

            [JsonPropertyName("blacklisted")]
            public bool Blacklisted { get; set; }
        }

        public class Root
        {
            [JsonPropertyName("items")]
            public List<Item> Items { get; set; }
        }
    }
}
