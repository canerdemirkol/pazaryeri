using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PazarYeriSiparis : IEntity
    {
        public string PazarYeriNo { get; set; }
        public long Id { get; set; }
        public string PaketId { get; set; }
        public string SiparisId { get; set; }
        public string SiparisNo { get; set; }
        public DateTime? SiparisTarih { get; set; }
        public DateTime? TahminiTeslimBaslangicTarih { get; set; }
        public DateTime? TahminiTeslimBitisTarih { get; set; }
        public double BrutTutar { get; set; }
        public decimal? MaksTutar { get; set; }
        public decimal? MinTutar { get; set; }
        public double ToplamIndirimTutar { get; set; }
        public decimal ToplamTutar { get; set; }
        public string? VergiDairesi { get; set; }
        public string? VergiNumarasi { get; set; }        
        public string? ParaBirimiKodu { get; set; }
        public string? TcKimlikNo { get; set; }
        public DateTime? TeslimatTarihi { get; set; }
        public string? TeslimatAdresTipi { get; set; }   
        public string? MusteriId { get; set; }
        public string? MusteriAdi { get; set; }
        public string? MusteriSoyadi { get; set; }
        public string? MusteriEmail { get; set; }
        public string? KargoTakipNo { get; set; }
        public string? KargoTakipUrl { get; set; }
        public string? KargoGondericiNumarasi { get; set; }
        public string? KargoSaglayiciAdi { get; set; }
        public string? SevkiyatPaketDurumu { get; set; }
        public string? KargoAdresId { get; set; }
        public string? FaturaAdresId { get; set; }
        public int? KoliAdeti { get; set; }
        public int? Desi { get; set; }
        public string? HasSent { get; set; }
        public string? Hata { get; set; }
        public string? DepoAktarildiEH { get; set; }
        public DateTime? InsertDatetime { get; set; }
        public virtual List<PazarYeriSiparisDetay>? PazarYeriSiparisDetails { get; set; }
    }
}