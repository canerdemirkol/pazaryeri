using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriFaturaAdresConfguration : IEntityTypeConfiguration<PazarYeriFaturaAdres>
    {
        public void Configure(EntityTypeBuilder<PazarYeriFaturaAdres> builder)
        {
            builder.ToTable(Db.Table.PazarYeriFaturaAdres.Name);

            builder.HasKey(x => new { x.Id, x.FaturaAdresId });

            builder.Property(x => x.Adi).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.Adi);
            builder.Property(x => x.Adres1).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.Adres1);
            builder.Property(x => x.Adres2).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.Adres2);
            builder.Property(x => x.AdSoyad).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.AdSoyad);
            builder.Property(x => x.Id).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.Id);
            builder.Property(x => x.FaturaAdresId).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.FaturaAdresId);
            builder.Property(x => x.Firma).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.Firma);
            builder.Property(x => x.PostaKod).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.PostaKod);
            builder.Property(x => x.Sehir).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.Sehir);
            builder.Property(x => x.Semt).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.Semt);
            builder.Property(x => x.Soyadi).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.Soyadi);
            builder.Property(x => x.TamAdres).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.TamAdres);
            builder.Property(x => x.UlkeKod).HasColumnName(Db.Table.PazarYeriFaturaAdres.Column.UlkeKod);

        }
    }
}