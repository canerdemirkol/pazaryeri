using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriKargoAdresConfiguration : IEntityTypeConfiguration<PazarYeriKargoAdres>
    {
        public void Configure(EntityTypeBuilder<PazarYeriKargoAdres> builder)
        {
            builder.ToTable(Db.Table.PazarYeriKargoAdres.Name);

            builder.HasKey(x =>new { x.Id,x.KargoAdresId });

            builder.Property(x => x.Ad).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.Ad);
            builder.Property(x => x.Adres1).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.Adres1);
            builder.Property(x => x.Adres2).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.Adres2);
            builder.Property(x => x.AdSoyad).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.AdSoyad);
            builder.Property(x => x.Id).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.Id);
            builder.Property(x => x.KargoAdresId).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.KargoAdresId);
            builder.Property(x => x.PostaKod).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.PostaKod);
            builder.Property(x => x.Sehir).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.Sehir);
            builder.Property(x => x.SehirKod).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.SehirKod);
            builder.Property(x => x.Semt).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.Semt);
            builder.Property(x => x.SemtId).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.SemtId);
            builder.Property(x => x.Soyad).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.Soyad);
            builder.Property(x => x.TamAdres).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.TamAdres);
            builder.Property(x => x.UlkeKod).HasColumnName(Db.Table.PazarYeriKargoAdres.Column.UlkeKod);
        }
    }
}