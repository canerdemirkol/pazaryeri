using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriSiparisIadeDetayConfiguration : IEntityTypeConfiguration<PazarYeriSiparisIadeDetay>
	{
		public void Configure(EntityTypeBuilder<PazarYeriSiparisIadeDetay> builder)
		{
			builder.ToTable(Db.Table.PazarYeriSiparisIadeDetay.Name);

			builder.Property(x => x.Aciklama).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.Aciklama);
			builder.Property(x => x.ClaimDetayId).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.ClaimDetayId);
			builder.Property(x => x.ClaimId).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.ClaimId);
			builder.Property(x => x.ClaimItemStatus).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.ClaimItemStatus);
			builder.Property(x => x.CozumlendiEH).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.CozumlendiEH);
			builder.Property(x => x.MusteriIadeSebepAd).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.MusteriIadeSebepAd);
			builder.Property(x => x.MusteriIadeSebepId).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.MusteriIadeSebepId);
			builder.Property(x => x.MusteriIadeSebepKod).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.MusteriIadeSebepKod);
			builder.Property(x => x.MusteriNot).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.MusteriNot);
			builder.Property(x => x.OrderLineItemId).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.OrderLineItemId);
			builder.Property(x => x.PyIadeSebepAd).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.PyIadeSebepAd);
			builder.Property(x => x.PyIadeSebepId).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.PyIadeSebepId);
			builder.Property(x => x.PyIadeSebepKod).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.PyIadeSebepKod);
			builder.Property(x => x.ClaimImageUrls).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.ClaimImageUrls);
            builder.Property(x => x.Miktar).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.Miktar);
            builder.Property(x => x.Sayisi).HasColumnName(Db.Table.PazarYeriSiparisIadeDetay.Column.Sayisi);

            builder.HasKey(x => new { x.ClaimId, x.ClaimDetayId });
		}
	}
}