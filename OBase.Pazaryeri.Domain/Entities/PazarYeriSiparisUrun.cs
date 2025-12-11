using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
    public class PazarYeriSiparisUrun:IEntity
    {
        public long ObaseSiparisId { get; set; }
        public string? PySiparisNo { get; set; }
        public string? PazarYeriBirimId { get; set; }
        public string? ObaseMalNo { get; set; }
        public string? PazarYeriMalNo { get; set; }
        public string? AltUrunPazarYeriMalNo { get; set; }
        public decimal? Miktar { get; set; }
        public decimal? GuncelMiktar { get; set; }
        public string? IsCancelledEH { get; set; }
        public string? IsAlternativeEH { get; set; }
        public string? IsCollectedEH { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? AltUrunMiktar { get; set; }
        public decimal? MinMiktar { get; set; }
        public decimal? MaxMiktar { get; set; }
        public string? AltUrunObaseMalNo { get; set; }
    }
}
