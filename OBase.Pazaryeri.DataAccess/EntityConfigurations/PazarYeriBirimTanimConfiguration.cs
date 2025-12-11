using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriBirimTanimConfiguration : IEntityTypeConfiguration<PazarYeriBirimTanim>
    {
        public void Configure(EntityTypeBuilder<PazarYeriBirimTanim> builder)
        {
            builder.ToTable(Db.Table.PazarYeriBirimTanim.Name);

            builder.HasKey(x =>  new{x.PazarYeriNo,x.BirimNo});

            builder.Property(x => x.BirimNo).HasColumnName(Db.Table.PazarYeriBirimTanim.Column.BirimNo);
            builder.Property(x => x.PazarYeriBirimNo).HasColumnName(Db.Table.PazarYeriBirimTanim.Column.PazarYeriBirimNo);
            builder.Property(x => x.PazarYeriNo).HasColumnName(Db.Table.PazarYeriBirimTanim.Column.PazarYeriNo);
            builder.Property(x => x.AktifPasif).HasColumnName(Db.Table.PazarYeriBirimTanim.Column.AktifPasif);
            builder.Ignore(x => x.BirimAdi);
        }
    }
}