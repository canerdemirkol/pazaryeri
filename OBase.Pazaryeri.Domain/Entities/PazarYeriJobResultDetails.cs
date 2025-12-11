using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PazarYeriJobResultDetails : IEntity
    {
        public long DetailId { get; set; }
        public long RefId { get; set; }
        public string PazarYeriNo { get; set; }
        public string MalNo { get; set; }
        public string PazarYeriMalNo { get; set; }
        public string PazarYeriBirimNo { get; set; }
        public string? Barkod { get; set; }
        public double? SatisFiyat { get; set; }
        public int? StokMiktar { get; set; }
        public decimal? SepeteEklenebilirMiktar { get; set; }
        public double? IndirimliSatisFiyat { get; set; }
        public DateTime? IndirimBaslangicTarih { get; set; }
        public DateTime? IndirimBitisTarih { get; set; }
        public int ThreadNo { get; set; }
        public string HasSent { get; set; }
        public string HasErrors { get; set; }
        public string? Guid { get; set; }
        public string HasVerified { get; set; }
        public string? AktifPasifEh { get; set; }
        public DateTime Tarih { get; set; }

        public virtual PazarYeriJobResult? PazarYeriJobResult { get; set; }
    }
}
