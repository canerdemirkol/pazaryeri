using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class BirimTanimConfiguration : IEntityTypeConfiguration<BirimTanim>
    {
        public void Configure(EntityTypeBuilder<BirimTanim> builder)
        {
            builder.ToTable(Db.Table.BirimTanim.Name);

            builder.HasKey(x => x.BirimNo);
			builder.Property(x => x.BirimAdi).HasColumnName(Db.Table.BirimTanim.Column.BirimAdi);
			builder.Property(x => x.BirimNo).HasColumnName(Db.Table.BirimTanim.Column.BirimNo);
			builder
            .HasMany(x => x.PazarYeriBirims)
            .WithOne(x => x.Birim)
            .HasForeignKey(x => x.BirimNo);
        }
    }
}