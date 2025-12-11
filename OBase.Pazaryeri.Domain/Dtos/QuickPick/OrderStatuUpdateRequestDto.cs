using Newtonsoft.Json;
using System.ComponentModel;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Domain.Dtos.QuickPick
{
    public class OrderStatuUpdateRequestDto
    {
    
        public OrderStatuUpdateRequestDto()
        {
            ProductQuantities = new List<ProductQuantity>();
        }
        public long OrderId { get; set; }
        [JsonIgnore]
        public string? PackageId { get; set; }
        [JsonIgnore]
        public string? SiparisNo { get; set; }
        [JsonIgnore]
        public string? MerchantId { get; set; }
        [DefaultValue("")]
        public string? PackageNumber { get; set; }
        public string? CancelNote { get; set; }
        public List<ProductQuantity> ProductQuantities { get; set; }
        public StatusEnums Status { get; set; }
        public string? ReasonId { get; set; }
        public long? ParcelQuantity { get; set; }
        public long? Deci { get; set; }
        [DefaultValue("")]
        public string? CargoCompany { get; set; }
        [DefaultValue("")]
        public string? Carrier { get; set; }
    }
    public class ProductQuantity
    {
        [DefaultValue("")]
        public string? ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string? ReasonId { get; set; }
        [DefaultValue("")]
        public string? AltProductId { get; set; }
    }
}