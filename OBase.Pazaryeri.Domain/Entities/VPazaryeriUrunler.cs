using OBase.Pazaryeri.Core.Abstract.Repository;

namespace OBase.Pazaryeri.Domain.Entities
{
    public  class VPazaryeriUrunler:IEntity
    {
		public string BirimNo { get; set; }
		public string? KategoriAdi { get; set; }
		public string? KategoriKod { get; set; }
		public string MalNo { get; set; }
		public string MalSatisBirimKod { get; set; }
		public decimal? MalSatisKdvDeger { get; set; }
		public string? PazarYeriMalAdi { get; set; }
		public string PazarYeriNo { get; set; }
		public decimal SatisFiyat { get; set; }
		public decimal? StokMiktar { get; set; }
	}
}