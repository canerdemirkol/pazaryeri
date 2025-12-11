using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class TmpPyPromosyonTanim:IEntity
    {
        public decimal? Id { get; set; }
        public string? PazaryeriNo { get; set; }
        public decimal? PromosyonNo { get; set; }
        public string? Reason { get; set; }
        public string? Active { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? PurchasedQuantity { get; set; }
        public decimal? OrderLimit { get; set; }
        public string? TumBirimlerEh { get; set; }
        public string? Type { get; set; }
        public string? DisplayName { get; set; }    
    }
}
