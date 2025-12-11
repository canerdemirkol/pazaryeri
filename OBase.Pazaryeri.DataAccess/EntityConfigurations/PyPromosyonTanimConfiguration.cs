using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PyPromosyonTanimConfiguration : IEntityTypeConfiguration<PyPromosyonTanim>
    {
        public void Configure(EntityTypeBuilder<PyPromosyonTanim> builder)
        {
            builder.ToTable(Db.Table.PyPromosyonTanim.Name);

            builder.HasKey(x => new { x.PazarYeriNo, x.PromosyonNo })
                   .HasName(Db.Table.PyPromosyonTanim.PK);

            builder.Property(x => x.PazarYeriNo)
                .HasMaxLength(10)
                .HasColumnName(Db.Table.PyPromosyonTanim.Column.PazarYeriNo)
                .IsRequired();

            builder.Property(x => x.PyPromosyonNo)
                .HasColumnName(Db.Table.PyPromosyonTanim.Column.PyPromosyonNo)
                .HasMaxLength(36);

            builder.Property(x => x.PromosyonTipKod)
                .HasMaxLength(2)
                .HasColumnName(Db.Table.PyPromosyonTanim.Column.PromosyonTipKod)
                .IsRequired();

            builder.Property(x => x.Durum)
                .HasColumnName(Db.Table.PyPromosyonTanim.Column.Durum)
                .HasMaxLength(1)
                .IsRequired()
                .HasDefaultValue("H");

            builder.Property(x => x.BaslangicSaat)
                .HasColumnName(Db.Table.PyPromosyonTanim.Column.BaslangicSaat)
                .HasMaxLength(5)
                .HasDefaultValue("00:00");

            builder.Property(x => x.BitisSaat)
                .HasColumnName(Db.Table.PyPromosyonTanim.Column.BitisSaat)
                .HasMaxLength(5)
                .HasDefaultValue("23:59");

            builder.Property(x => x.MinSiparisMiktar)
                .HasColumnName(Db.Table.PyPromosyonTanim.Column.MinSiparisMiktar)
                .HasDefaultValue(1);

            builder.Property(x => x.MaxSiparisMiktar)
               .HasColumnName(Db.Table.PyPromosyonTanim.Column.MaxSiparisMiktar);

            builder.Property(x => x.TumBirimlerEh)
                .HasColumnName(Db.Table.PyPromosyonTanim.Column.TumBirimlerEh)
                .HasMaxLength(1);

            builder.Property(x => x.InsertDatetime).HasColumnName(Db.Table.PyPromosyonTanim.Column.InsertDatetime).HasDefaultValueSql("SYSDATE");

            builder.HasIndex(x => new { x.PazarYeriNo, x.PyPromosyonNo })
                   .IsUnique()
                   .HasDatabaseName("UNDX_PY_PROMOSYON_TANIM_01");
        }
    }

}
