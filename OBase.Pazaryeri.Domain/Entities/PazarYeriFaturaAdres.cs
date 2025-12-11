using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PazarYeriFaturaAdres : IEntity
    {
        public long Id { get; set; }
        public string FaturaAdresId { get; set; }
        public string? Adi { get; set; }
        public string? Soyadi { get; set; }
        public string? Firma { get; set; }
        public string? Adres1 { get; set; }
        public string? Adres2 { get; set; }
        public string? Sehir { get; set; }
        public string? Semt { get; set; }
        public string? PostaKod { get; set; }
        public string? UlkeKod { get; set; }
        public string? AdSoyad { get; set; }
        public string? TamAdres { get; set; }
    }
}