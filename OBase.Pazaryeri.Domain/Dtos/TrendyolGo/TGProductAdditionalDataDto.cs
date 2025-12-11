namespace OBase.Pazaryeri.Domain.Dtos.TrendyolGo
{
    public class TGProductAdditionalDataDto
    {
        public decimal orderedQuantity { get; set; } = 0;
        public decimal suppliedQuantity { get; set; } = 0;
        public decimal unSuppliedQuantity { get; set; } = 0;
        public bool isUnSupplied { get; set; } = false;
        public bool isFullyUnSupplied { get; set; } = false;
        public int tySuppliedProductCount { get; set; } = 0;
        public decimal tyQuantityCoefficient { get; set; } = 1;
        public bool isWeightedItem { get; set; } = false;
        public string AltProductId { get; set; }
        public string ProductId { get; set; }
        public string LineItemId { get; set; }
        public bool isAlternateProductGiven { get; set; } = false;
        public decimal altSuppliedQuantity { get; set; } = 0;
        public decimal productMPUnitPrice { get; set; } = 0;
    }
}