using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PazarYeriSiparisEkBilgi : IEntity
    {
        public long ObaseSiparisId { get; set; }
        public string? PySiparisNo { get; set; }
        public decimal? GuncelFaturaTutar { get; set; }
        public int PosetSayisi { get; set; }
        public decimal PosetTutari { get; set; }       
        public decimal? GonderimUcreti { get; set; }   
    }
}