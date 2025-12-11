using OBase.Pazaryeri.Core.Abstract.Repository;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PyPromosyonEntegrasyon : IEntity
    {
        public long Id { get; set; }
        public DateTime Tarih { get; set; } = DateTime.Now.Date;
        public long PromosyonNo { get; set; }
        public string GonderildiEh { get; set; } = "H";
        public string ServisDurum { get; set; } = "0";
        public int TryCount { get; set; } = 0;
        public string? HataMesaji { get; set; }
        public DateTime? InsertDatetime { get; set; } = DateTime.Now;
    }
}
