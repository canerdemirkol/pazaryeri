using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PyPromosyonTanim : IEntity
    {
        public string PazarYeriNo { get; set; } = null!;
        public long PromosyonNo { get; set; }
        public string? PyPromosyonNo { get; set; }
        public string PromosyonTipKod { get; set; } = null!;
        public string Durum { get; set; } = "H"; // H=Hazır, O=Onaylı, I=İptal
        public DateTime BaslangicTarih { get; set; }
        public string BaslangicSaat { get; set; } = "00:00";
        public DateTime BitisTarih { get; set; }
        public string BitisSaat { get; set; } = "23:59";
        public decimal MinSiparisMiktar { get; set; } = 1;
        public decimal? MaxSiparisMiktar { get; set; }
        public string? TumBirimlerEh { get; set; } // E/H
        public DateTime InsertDatetime { get; set; } 
    }
}
