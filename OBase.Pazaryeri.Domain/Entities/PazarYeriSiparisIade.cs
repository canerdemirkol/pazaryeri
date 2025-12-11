using OBase.Pazaryeri.Core.Abstract.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Entities
{
	public class PazarYeriSiparisIade : IEntity
	{
		public string? BirimAciklama { get; set; }
		public string ClaimId { get; set; }
		public string? ClaimStatus { get; set; }
		public DateTime? ClaimTarih { get; set; }
		public string? DepoAktarildiEH { get; set; }
		public double? Id { get; set; }
		public string? MusteriAd { get; set; }
		public string? MusteriSoyad { get; set; }
		public string? PazarYeriNo { get; set; }
        public string? PazarYeriBirimNo { get; set; }
        public string? ReturnedSellerEH { get; set; }
		public string? SiparisNo { get; set; }
		public string? SiparisPaketId { get; set; }
		public DateTime? SiparisTarih { get; set; }
        public int? DepoAktarimDenemeSayisi { get; set; }
        public virtual List<PazarYeriSiparisIadeDetay>? PazarYeriSiparisIadeDetay { get; set; }

    }
}
