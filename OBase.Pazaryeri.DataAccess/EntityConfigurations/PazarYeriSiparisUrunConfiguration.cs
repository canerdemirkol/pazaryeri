using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriSiparisUrunConfiguration : IEntityTypeConfiguration<PazarYeriSiparisUrun>
    {
        public void Configure(EntityTypeBuilder<PazarYeriSiparisUrun> builder)
        {
            builder.ToTable(Db.Table.PazarYeriSiparisUrun.Name);

            builder.HasKey(x =>new{x.ObaseSiparisId,x.ObaseMalNo,x.PazarYeriMalNo,x.PySiparisNo});

            builder.Property(x => x.AltUrunMiktar).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.AltUrunMiktar).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            builder.Property(x => x.AltUrunObaseMalNo).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.AltUrunObaseMalNo);
            builder.Property(x => x.AltUrunPazarYeriMalNo).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.AltUrunPazarYeriMalNo);
            builder.Property(x => x.GuncelMiktar).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.GuncelMiktar).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            builder.Property(x => x.ImageUrl).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.ImageUrl);
            builder.Property(x => x.IsAlternativeEH).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.IsalternativeEh);
            builder.Property(x => x.IsCancelledEH).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.IscancelledEh);
            builder.Property(x => x.IsCollectedEH).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.IscollectedEh);
            builder.Property(x => x.Miktar).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.Miktar).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            builder.Property(x => x.ObaseMalNo).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.ObaseMalNo);
            builder.Property(x => x.ObaseSiparisId).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.ObaseSiparisId);
            builder.Property(x => x.PazarYeriBirimId).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.PazarYeriBirimId);
            builder.Property(x => x.PazarYeriMalNo).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.PazarYeriMalNo);
            builder.Property(x => x.PySiparisNo).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.PySiparisNo);
            builder.Property(x => x.MinMiktar).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.MinMiktar);
            builder.Property(x => x.MaxMiktar).HasColumnName(Db.Table.PazarYeriSiparisUrun.Column.MaxMiktar);
        }
    }
}