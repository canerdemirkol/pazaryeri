using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PazarYeriKargoAdres : IEntity
    {
        public long Id { get; set; }
        public string KargoAdresId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? Adres1 { get; set; }
        public string? Adres2 { get; set; }
        public int? SehirKod { get; set; }
        public string? Sehir { get; set; }
        public int? SemtId { get; set; }
        public string? Semt { get; set; }
        public string? PostaKod { get; set; }
        public string? UlkeKod { get; set; }
        public string? AdSoyad { get; set; }
        public string? TamAdres { get; set; }
    }
}