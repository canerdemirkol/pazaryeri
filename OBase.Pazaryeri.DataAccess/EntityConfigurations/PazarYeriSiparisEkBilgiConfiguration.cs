using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriSiparisEkBilgiConfiguration : IEntityTypeConfiguration<PazarYeriSiparisEkBilgi>
    {
        public void Configure(EntityTypeBuilder<PazarYeriSiparisEkBilgi> builder)
        {
            builder.ToTable(Db.Table.PazarYeriSiparisEkBilgi.Name);

            builder.HasKey(x => x.ObaseSiparisId);
            {
                builder.Property(x => x.GuncelFaturaTutar).HasColumnName(Db.Table.PazarYeriSiparisEkBilgi.Column.GuncelFaturaTutar).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
                builder.Property(x => x.ObaseSiparisId).HasColumnName(Db.Table.PazarYeriSiparisEkBilgi.Column.ObaseSiparisId);
                builder.Property(x => x.PosetSayisi).HasColumnName(Db.Table.PazarYeriSiparisEkBilgi.Column.PosetSayisi);
                builder.Property(x => x.PosetTutari).HasColumnName(Db.Table.PazarYeriSiparisEkBilgi.Column.PosetTutari).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
                builder.Property(x => x.PySiparisNo).HasColumnName(Db.Table.PazarYeriSiparisEkBilgi.Column.PySiparisNo);
                builder.Property(x => x.GonderimUcreti).HasColumnName(Db.Table.PazarYeriSiparisEkBilgi.Column.GonderimUcreti).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            }
        }
    }
}