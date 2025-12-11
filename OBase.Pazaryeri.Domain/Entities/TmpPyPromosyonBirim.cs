using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class TmpPyPromosyonBirim : IEntity
    {
        public decimal? Id { get; set; }
        public decimal? PromosyonNo { get; set; }
        public string? BirimNo { get; set; }
    }
}
