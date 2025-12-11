using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriAktarimConfiguration : IEntityTypeConfiguration<PazarYeriAktarim>
    {
        public void Configure(EntityTypeBuilder<PazarYeriAktarim> builder)
        {
            builder.ToTable(Db.Table.PazarYeriAktarim.Name);

            builder.HasNoKey();
         
            builder.Property(x => x.BirimNo).HasColumnName(Db.Table.PazarYeriAktarim.Column.BirimNo);
            builder.Property(x => x.IndirimliSatisFiyat).HasColumnName(Db.Table.PazarYeriAktarim.Column.IndirimliSatisFiyat).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            builder.Property(x => x.MalNo).HasColumnName(Db.Table.PazarYeriAktarim.Column.MalNo);
            builder.Property(x => x.PazarYeriBirimNo).HasColumnName(Db.Table.PazarYeriAktarim.Column.PazarYeriBirimNo);
            builder.Property(x => x.PazarYeriMalNo).HasColumnName(Db.Table.PazarYeriAktarim.Column.PazarYeriMalNo);
            builder.Property(x => x.PazarYeriNo).HasColumnName(Db.Table.PazarYeriAktarim.Column.PazarYeriNo);
            builder.Property(x => x.SatisFiyat).HasColumnName(Db.Table.PazarYeriAktarim.Column.SatisFiyat).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            builder.Property(x => x.QpFiyat).HasColumnName(Db.Table.PazarYeriAktarim.Column.QpFiyat).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);

        }
    }
}