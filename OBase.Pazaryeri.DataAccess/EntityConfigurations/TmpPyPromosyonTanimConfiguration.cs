using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class TmpPyPromosyonTanimConfiguration : IEntityTypeConfiguration<TmpPyPromosyonTanim>
    {
        public void Configure(EntityTypeBuilder<TmpPyPromosyonTanim> builder)
        {
            builder.ToTable(Db.Table.TmpPyPromosyonTanim.Name);

            builder.HasNoKey(); 

            builder.Property(x => x.Id)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.Id);

            builder.Property(x => x.PazaryeriNo)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.PazaryeriNo)
                .HasMaxLength(10);

            builder.Property(x => x.PromosyonNo)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.PromosyonNo);

            builder.Property(x => x.Reason)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.Reason)
                .HasMaxLength(30);

            builder.Property(x => x.Active)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.Active)
                .HasMaxLength(10);

            builder.Property(x => x.StartTime)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.StartTime);

            builder.Property(x => x.EndTime)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.EndTime);

            builder.Property(x => x.PurchasedQuantity)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.PurchasedQuantity);

            builder.Property(x => x.OrderLimit)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.OrderLimit);

            builder.Property(x => x.TumBirimlerEh)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.TumBirimlerEh)
                .HasMaxLength(1);

            builder.Property(x => x.Type)
                .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.Type)
                .HasMaxLength(30);

            builder.Property(x => x.DisplayName)
               .HasColumnName(Db.Table.TmpPyPromosyonTanim.Column.DisplayName)
               .HasMaxLength(50);
        }
    }

}
