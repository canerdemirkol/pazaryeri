using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class TmpPyPromosyonDetay : IEntity
    {
        public decimal? Id { get; set; }
        public decimal? PromosyonNo { get; set; }
        public string? Sku { get; set; }
        public string? Active { get; set; }
        public string? DiscountSubtype { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? MaxQuantity { get; set; }
    }
}
