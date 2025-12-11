using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriSiparisIadeConfiguration : IEntityTypeConfiguration<PazarYeriSiparisIade>
	{
		public void Configure(EntityTypeBuilder<PazarYeriSiparisIade> builder)
		{
			builder.ToTable(Db.Table.PazarYeriSiparisIade.Name);

			builder.Property(x => x.BirimAciklama).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.BirimAciklama);
			builder.Property(x => x.ClaimId).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.ClaimId);
			builder.Property(x => x.ClaimStatus).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.ClaimStatus);
			builder.Property(x => x.ClaimTarih).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.ClaimTarih);
			builder.Property(x => x.DepoAktarildiEH).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.DepoAktarildiEH);
			builder.Property(x => x.Id).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.Id);
			builder.Property(x => x.MusteriAd).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.MusteriAd);
			builder.Property(x => x.MusteriSoyad).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.MusteriSoyad);
			builder.Property(x => x.PazarYeriNo).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.PazarYeriNo);
            builder.Property(x => x.PazarYeriBirimNo).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.PazarYeriBirimNo);
            builder.Property(x => x.ReturnedSellerEH).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.ReturnedSellerEH);
			builder.Property(x => x.SiparisNo).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.SiparisNo);
			builder.Property(x => x.SiparisPaketId).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.SiparisPaketId);
			builder.Property(x => x.SiparisTarih).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.SiparisTarih);
			builder.Property(x => x.DepoAktarimDenemeSayisi).HasColumnName(Db.Table.PazarYeriSiparisIade.Column.DepoAktarimDenemeSayisi);
			builder.HasKey(x => x.ClaimId);
            builder
            .HasMany(x => x.PazarYeriSiparisIadeDetay)
            .WithOne(x => x.PazarYeriSiparisIade)
            .HasForeignKey(x => x.ClaimId);
        }
	}
}