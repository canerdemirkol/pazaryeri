using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PyPromosyonEntegrasyonConfiguration : IEntityTypeConfiguration<PyPromosyonEntegrasyon>
    {
        public void Configure(EntityTypeBuilder<PyPromosyonEntegrasyon> builder)
        {
            builder.ToTable(Db.Table.PyPromosyonEntegrasyon.Name);

            builder.HasKey(x => new { x.Id, x.Tarih })
                   .HasName(Db.Table.PyPromosyonEntegrasyon.PK);

            builder.Property(x => x.Id)
                .HasColumnName(Db.Table.PyPromosyonEntegrasyon.Column.Id)
                .IsRequired()
                .ValueGeneratedNever(); // Sequence tetikleyiciyle geliyor, EF üretmeyecek

            builder.Property(x => x.Tarih)
                .HasColumnName(Db.Table.PyPromosyonEntegrasyon.Column.Tarih)
                .HasDefaultValueSql("TRUNC(SYSDATE)")
                .IsRequired();

            builder.Property(x => x.PromosyonNo)
                .HasColumnName(Db.Table.PyPromosyonEntegrasyon.Column.PromosyonNo)
                .IsRequired();

            builder.Property(x => x.GonderildiEh)
                .HasColumnName(Db.Table.PyPromosyonEntegrasyon.Column.GonderildiEh)
                .HasMaxLength(1)
                .HasDefaultValue("H")
                .IsRequired();

            builder.Property(x => x.ServisDurum)
                .HasColumnName(Db.Table.PyPromosyonEntegrasyon.Column.ServisDurum)
                .HasMaxLength(1)
                .HasDefaultValue("0")
                .IsRequired();

            builder.Property(x => x.TryCount)
                .HasColumnName(Db.Table.PyPromosyonEntegrasyon.Column.TryCount)
                .HasDefaultValue(0);

            builder.Property(x => x.HataMesaji)
                .HasColumnName(Db.Table.PyPromosyonEntegrasyon.Column.HataMesaji)
                .HasMaxLength(4000);

            builder.Property(x => x.InsertDatetime).HasColumnName(Db.Table.PyPromosyonEntegrasyon.Column.InsertDatetime).HasDefaultValueSql("SYSDATE");

            builder.HasIndex(x => new { x.Tarih, x.GonderildiEh, x.ServisDurum, x.TryCount })
                .HasDatabaseName("NDX_PY_PROMOSYON_ENTEGRASYON01");
        }
    }

}
