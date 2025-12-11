using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PazarYeriSiparisDetay : IEntity
    {
        public long Id { get; set; }
        public string LineItemId { get; set; }
        public string? PaketItemId { get; set; }
        public string? PazarYeriBirimId { get; set; }
        public string? ObaseMalNo { get; set; }
        public string? PazarYeriMalNo { get; set; }
        public string? PazarYeriUrunKodu { get; set; }
        public string? PazarYeriMalAdi { get; set; }
        public string? AlternatifUrunEH { get; set; }
        public string? Barkod { get; set; }
        public decimal Miktar { get; set; }
        public decimal NetTutar { get; set; }
        public decimal IndirimTutar { get; set; }
        public decimal KdvTutar { get; set; }
        public decimal BrutTutar { get; set; }
        public double? KdvOran { get; set; }
        public string? ParaBirimiKodu { get; set; }
        public string? SatisKampanyaId { get; set; }
        public string? UrunBoyutu { get; set; }
        public string? UrunRengi { get; set; }
        public string? SiparisUrunDurumAdi { get; set; }
        public string IsCancelledEH { get; set; }
        public string IsAlternativeEH { get; set; }
        public string IsCollectedEH { get; set; }
        public string HasSent { get; set; }
        public string? Hata { get; set; }
        public string? ReasonId { get; set; }
        public decimal? Weight { get; set; }
        public virtual PazarYeriSiparis? PazarYeriSiparis { get; set; }
    }
}