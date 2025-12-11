using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
	public class PazarYeriSiparisIadeDetay : IEntity
	{
		public string? Aciklama { get; set; }
		public string ClaimDetayId { get; set; }
		public string? ClaimId { get; set; }
		public string? ClaimItemStatus { get; set; }
		public string? CozumlendiEH { get; set; }
		public string? MusteriIadeSebepAd { get; set; }
		public decimal? MusteriIadeSebepId { get; set; }
		public string? MusteriIadeSebepKod { get; set; }
		public string? MusteriNot { get; set; }
		public string? OrderLineItemId { get; set; }
		public string? PyIadeSebepAd { get; set; }
		public decimal? PyIadeSebepId { get; set; }
		public string? PyIadeSebepKod { get; set; }
		public string? ClaimImageUrls { get; set; }
		public decimal? Miktar { get; set; }
		public int? Sayisi { get; set; }
		public virtual PazarYeriSiparisIade? PazarYeriSiparisIade { get; set; }
	}
}
