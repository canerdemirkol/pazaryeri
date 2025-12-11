using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Dtos.YemekSepeti
{
    public class YemekSepetiProductDetailResponseDto
    {
        [JsonPropertyName("page_index")]
        public int PageIndex { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPage { get; set; }

        [JsonPropertyName("total_records")]
        public int TotalRecords { get; set; }

        [JsonPropertyName("products")]
        public List<Product> Products { get; set; }
    }

    public class Product
    {
        [JsonPropertyName("remote_product_id")]
        public string RemoteProductId { get; set; }

        [JsonPropertyName("sku")]
        public string Sku { get; set; }

        [JsonPropertyName("barcodes")]
        public List<string> Barcodes { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("translations")]
        public Translations Translations { get; set; }

        [JsonPropertyName("images")]
        public string[] Images { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("price")]
        public float Price { get; set; }

        [JsonPropertyName("categories")]
        public Category[] Categories { get; set; }

        [JsonPropertyName("master_category_path")]
        public string MasterCategoryPath { get; set; }
    }

    public class Translations
    {
        [JsonPropertyName("en_TR")]
        public string EnTr { get; set; }

        [JsonPropertyName("tr_TR")]
        public string TrTr { get; set; }
    }

    public class Category
    {
        [JsonPropertyName("global_id")]
        public string GlobalId { get; set; }

        [JsonPropertyName("name")]
        public Name Name { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }
    }

    public class Name
    {
        [JsonPropertyName("tr_TR")]
        public string TrTr { get; set; }
    }

    public class YemekSepetiProductDetailErrorResponseDto
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }
    }

}