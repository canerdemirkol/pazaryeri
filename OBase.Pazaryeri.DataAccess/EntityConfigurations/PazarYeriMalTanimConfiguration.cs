using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriMalTanimConfiguration : IEntityTypeConfiguration<PazarYeriMalTanim>
    {
        public void Configure(EntityTypeBuilder<PazarYeriMalTanim> builder)
        {
            builder.ToTable(Db.Table.PazarYeriMalTanim.Name);

            builder.HasKey(x =>new { x.PazarYeriNo,x.MalNo});

            builder.Property(x => x.AnaMalNo).HasColumnName(Db.Table.PazarYeriMalTanim.Column.AnaMalNo);
            builder.Property(x => x.MalNo).HasColumnName(Db.Table.PazarYeriMalTanim.Column.MalNo);
            builder.Property(x => x.PazarYeriMalAdi).HasColumnName(Db.Table.PazarYeriMalTanim.Column.PazarYeriMalAdi);
            builder.Property(x => x.PazarYeriMalNo).HasColumnName(Db.Table.PazarYeriMalTanim.Column.PazarYeriMalNo);
            builder.Property(x => x.PazarYeriNo).HasColumnName(Db.Table.PazarYeriMalTanim.Column.PazarYeriNo);
            builder.Property(x => x.PyUrunSatisBirim).HasColumnName(Db.Table.PazarYeriMalTanim.Column.PyUrunSatisBirim);
            builder.Property(x => x.PyUrunSatisDeger).HasColumnName(Db.Table.PazarYeriMalTanim.Column.PyUrunSatisDeger).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            builder.Property(x => x.SepeteEklenebilirMiktar).HasColumnName(Db.Table.PazarYeriMalTanim.Column.SepeteEklenebilirMiktar);
            builder.Property(x => x.ImageUrl).HasColumnName(Db.Table.PazarYeriMalTanim.Column.ImageUrl);
        }
    }
}