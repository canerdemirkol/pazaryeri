using System.Text.Json.Serialization;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;

namespace OBase.Pazaryeri.Domain.Dtos.Sale
{
    public class SaleInfoDto : BaseOrderDto
    {
        [JsonPropertyName("OrderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("OrderCode")]
        public string OrderCode { get; set; } = string.Empty;

        [JsonPropertyName("ExternalOrderId")]
        public string ExternalOrderId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; }=string.Empty;

        [JsonPropertyName("StoreCode")]
        public string StoreCode { get; set; }

        [JsonPropertyName("SaleDateUtc")]
        public DateTime SaleDateUtc { get; set; }

        [JsonPropertyName("TotalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("TotalTaxPrice")]
        public decimal TotalTaxPrice { get; set; }

        [JsonPropertyName("TotalReceiptDiscount")]
        public decimal TotalReceiptDiscount { get; set; }

        [JsonPropertyName("TotalProductDiscount")]
        public decimal TotalProductDiscount { get; set; }

        [JsonPropertyName("Customer")]
        public CustomerDto Customer { get; set; }

        [JsonPropertyName("BillingAddress")]
        public BillingAddressDto BillingAddress { get; set; }

        [JsonPropertyName("ShippingAddress")]
        public ShippingAddressDto ShippingAddress { get; set; }

        [JsonPropertyName("Items")]
        public List<ItemDto> Items { get; set; } = new List<ItemDto>();

        [JsonPropertyName("Payments")]
        public List<PaymentDto> Payments { get; set; } = new List<PaymentDto>();

        [JsonPropertyName("Discounts")]
        public List<DiscountDto> Discounts { get; set; } = new List<DiscountDto>();
    }
    /// <summary>
    /// Customer information
    /// </summary>
    public class CustomerDto
    {
        [JsonPropertyName("CustomerId")]
        public string CustomerId { get; set; }

        [JsonPropertyName("CardNo")]
        public string CardNo { get; set; } = string.Empty;

        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("LastName")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Billing address information
    /// </summary>
    public class BillingAddressDto
    {
        [JsonPropertyName("Address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("Address2")]
        public string? Address2 { get; set; } = string.Empty;

        [JsonPropertyName("District")]
        public string District { get; set; } = string.Empty;

        [JsonPropertyName("City")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("TaxNo")]
        public string TaxNo { get; set; } = string.Empty;

        [JsonPropertyName("TaxOffice")]
        public string TaxOffice { get; set; } = string.Empty;
    }

    /// <summary>
    /// Shipping address information
    /// </summary>
    public class ShippingAddressDto
    {
        [JsonPropertyName("Address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("Address2")]
        public string? Address2 { get; set; } = string.Empty;

        [JsonPropertyName("District")]
        public string District { get; set; } = string.Empty;

        [JsonPropertyName("City")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("PhoneNumber")]
        public string? PhoneNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sale item information
    /// </summary>
    public class ItemDto
    {
        [JsonPropertyName("ItemId")]
        public string ItemId { get; set; }

        [JsonPropertyName("ProductCode")]
        public string ProductCode { get; set; } 

        [JsonPropertyName("Barcode")]
        public string Barcode { get; set; }

        [JsonPropertyName("Quantity")]
        public decimal Quantity { get; set; }

        [JsonPropertyName("Price")]
        public decimal Price { get; set; }

        [JsonPropertyName("Total")]
        public decimal Total { get; set; }

        [JsonPropertyName("VatRate")]
        public decimal VatRate { get; set; }

        [JsonPropertyName("TotalVat")]
        public decimal TotalVat { get; set; }

        [JsonPropertyName("ReceiptDiscount")]
        public decimal? ReceiptDiscount { get; set; }

        [JsonPropertyName("ProductDiscount")]
        public decimal? ProductDiscount { get; set; }
    }

    /// <summary>
    /// Payment information
    /// </summary>
    public class PaymentDto
    {
        [JsonPropertyName("Code")]
        public string Code { get; set; } 

        [JsonPropertyName("Total")]
        public decimal? Total { get; set; }

        [JsonPropertyName("InstallmentCount")]
        public int? InstallmentCount { get; set; }
    }

    /// <summary>
    /// Discount information
    /// </summary>
    public class DiscountDto
    {
        [JsonPropertyName("Code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("Total")]
        public decimal? Total { get; set; } 

        [JsonPropertyName("Barcode")]
        public string? Barcode { get; set; } = string.Empty;

        [JsonPropertyName("ProductCode")]
        public string? ProductCode { get; set; } = string.Empty;
    }
}
