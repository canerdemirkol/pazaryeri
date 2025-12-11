using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    internal class VPazaryeriUrunlerConfiguration : IEntityTypeConfiguration<VPazaryeriUrunler>
	{
		public void Configure(EntityTypeBuilder<VPazaryeriUrunler> builder)
		{
			builder.ToView(Db.Table.VPazaryeriUrunler.Name);
			builder.Property(x => x.BirimNo).HasColumnName(Db.Table.VPazaryeriUrunler.Column.BirimNo);
			builder.Property(x => x.KategoriAdi).HasColumnName(Db.Table.VPazaryeriUrunler.Column.KategoriAdi);
			builder.Property(x => x.KategoriKod).HasColumnName(Db.Table.VPazaryeriUrunler.Column.KategoriKod);
			builder.Property(x => x.MalNo).HasColumnName(Db.Table.VPazaryeriUrunler.Column.MalNo);
			builder.Property(x => x.MalSatisBirimKod).HasColumnName(Db.Table.VPazaryeriUrunler.Column.MalSatisBirimKod);
			builder.Property(x => x.MalSatisKdvDeger).HasColumnName(Db.Table.VPazaryeriUrunler.Column.MalSatisKdvDeger);
			builder.Property(x => x.PazarYeriMalAdi).HasColumnName(Db.Table.VPazaryeriUrunler.Column.PazarYeriMalAdi);
			builder.Property(x => x.PazarYeriNo).HasColumnName(Db.Table.VPazaryeriUrunler.Column.PazarYeriNo);
			builder.Property(x => x.SatisFiyat).HasColumnName(Db.Table.VPazaryeriUrunler.Column.SatisFiyat).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
			builder.Property(x => x.StokMiktar).HasColumnName(Db.Table.VPazaryeriUrunler.Column.StokMiktar);
			builder.HasNoKey();
		}
	}
}
