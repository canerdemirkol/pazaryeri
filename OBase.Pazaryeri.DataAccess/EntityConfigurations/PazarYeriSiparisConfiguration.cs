using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
	public class PazarYeriSiparisConfiguration : IEntityTypeConfiguration<PazarYeriSiparis>
	{
		public void Configure(EntityTypeBuilder<PazarYeriSiparis> builder)
		{
			builder.ToTable(Db.Table.PazarYeriSiparis.Name);
			builder.HasKey(x => x.Id);

			builder.Property(x => x.Id).HasColumnName(Db.Table.PazarYeriSiparis.Column.Id);
			builder.Property(x => x.PaketId).HasColumnName(Db.Table.PazarYeriSiparis.Column.PaketId);
			builder.Property(x => x.ParaBirimiKodu).HasColumnName(Db.Table.PazarYeriSiparis.Column.ParaBirimiKodu);
			builder.Property(x => x.PazarYeriNo).HasColumnName(Db.Table.PazarYeriSiparis.Column.PazarYeriNo);
			builder.Property(x => x.SevkiyatPaketDurumu).HasColumnName(Db.Table.PazarYeriSiparis.Column.SevkiyatPaketDurumu);
			builder.Property(x => x.SiparisId).HasColumnName(Db.Table.PazarYeriSiparis.Column.SiparisId);
			builder.Property(x => x.SiparisNo).HasColumnName(Db.Table.PazarYeriSiparis.Column.SiparisNo);
			builder.Property(x => x.SiparisTarih).HasColumnName(Db.Table.PazarYeriSiparis.Column.SiparisTarih);
			builder.Property(x => x.TahminiTeslimBaslangicTarih).HasColumnName(Db.Table.PazarYeriSiparis.Column.TahminiTeslimBaslangicTarih);
			builder.Property(x => x.TahminiTeslimBitisTarih).HasColumnName(Db.Table.PazarYeriSiparis.Column.TahminiTeslimBitisTarih);
			builder.Property(x => x.TcKimlikNo).HasColumnName(Db.Table.PazarYeriSiparis.Column.TcKimlikNo);
			builder.Property(x => x.TeslimatAdresTipi).HasColumnName(Db.Table.PazarYeriSiparis.Column.TeslimatAdresTipi);
			builder.Property(x => x.TeslimatTarihi).HasColumnName(Db.Table.PazarYeriSiparis.Column.TeslimatTarihi);
			builder.Property(x => x.ToplamIndirimTutar).HasColumnName(Db.Table.PazarYeriSiparis.Column.ToplamIndirimTutar);
			builder.Property(x => x.ToplamTutar).HasColumnName(Db.Table.PazarYeriSiparis.Column.ToplamTutar);
			builder.Property(x => x.VergiDairesi).HasColumnName(Db.Table.PazarYeriSiparis.Column.VergiDairesi);
			builder.Property(x => x.VergiNumarasi).HasColumnName(Db.Table.PazarYeriSiparis.Column.VergiNumarasi);
			builder.Property(x => x.InsertDatetime).HasColumnName(Db.Table.PazarYeriSiparis.Column.InsertDatetime).HasDefaultValueSql("SYSDATE");
			builder.Property(x => x.BrutTutar).HasColumnName(Db.Table.PazarYeriSiparis.Column.BrutTutar);
			builder.Property(x => x.DepoAktarildiEH).HasColumnName(Db.Table.PazarYeriSiparis.Column.DepoAktarildiEh);
			builder.Property(x => x.HasSent).HasColumnName(Db.Table.PazarYeriSiparis.Column.HasSent);
			builder.Property(x => x.Hata).HasColumnName(Db.Table.PazarYeriSiparis.Column.Hata);
			builder.Property(x => x.FaturaAdresId).HasColumnName(Db.Table.PazarYeriSiparis.Column.FaturaAdresId);
			builder.Property(x => x.KargoAdresId).HasColumnName(Db.Table.PazarYeriSiparis.Column.KargoAdresId);
			builder.Property(x => x.KoliAdeti).HasColumnName(Db.Table.PazarYeriSiparis.Column.KoliAdeti);
			builder.Property(x => x.Desi).HasColumnName(Db.Table.PazarYeriSiparis.Column.Desi);
			builder.Property(x => x.KargoTakipNo).HasColumnName(Db.Table.PazarYeriSiparis.Column.KargoTakipNo);
			builder.Property(x => x.KargoSaglayiciAdi).HasColumnName(Db.Table.PazarYeriSiparis.Column.KargoSaglayiciAdi);
			builder.Property(x => x.KargoGondericiNumarasi).HasColumnName(Db.Table.PazarYeriSiparis.Column.KargoGondericiNumarasi);
			builder.Property(x => x.KargoTakipUrl).HasColumnName(Db.Table.PazarYeriSiparis.Column.KargoTakipUrl);
			builder.Property(x => x.MusteriEmail).HasColumnName(Db.Table.PazarYeriSiparis.Column.MusteriEmail);
			builder.Property(x => x.MusteriAdi).HasColumnName(Db.Table.PazarYeriSiparis.Column.MusteriAdi);
			builder.Property(x => x.MusteriSoyadi).HasColumnName(Db.Table.PazarYeriSiparis.Column.MusteriSoyadi);
			builder.Property(x => x.MusteriId).HasColumnName(Db.Table.PazarYeriSiparis.Column.MusteriId);
			builder.Property(x => x.MaksTutar).HasColumnName(Db.Table.PazarYeriSiparis.Column.MaksTutar);
			builder.Property(x => x.MinTutar).HasColumnName(Db.Table.PazarYeriSiparis.Column.MinTutar);

			builder
			.HasMany(x => x.PazarYeriSiparisDetails)
			.WithOne(x => x.PazarYeriSiparis)
			.HasForeignKey(x => x.Id);
		}
	}
}