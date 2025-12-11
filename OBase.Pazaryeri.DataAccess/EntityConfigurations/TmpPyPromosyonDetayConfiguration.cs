using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class TmpPyPromosyonDetayConfiguration : IEntityTypeConfiguration<TmpPyPromosyonDetay>
    {
        public void Configure(EntityTypeBuilder<TmpPyPromosyonDetay> builder)
        {
            builder.ToTable(Db.Table.TmpPyPromosyonDetay.Name);

            builder.HasNoKey(); 

            builder.Property(x => x.Id)
                .HasColumnName(Db.Table.TmpPyPromosyonDetay.Column.Id);

            builder.Property(x => x.PromosyonNo)
                .HasColumnName(Db.Table.TmpPyPromosyonDetay.Column.PromosyonNo);

            builder.Property(x => x.Sku)
                .HasColumnName(Db.Table.TmpPyPromosyonDetay.Column.Sku)
                .HasMaxLength(16);

            builder.Property(x => x.Active)
                .HasColumnName(Db.Table.TmpPyPromosyonDetay.Column.Active)
                .HasMaxLength(10);

            builder.Property(x => x.DiscountSubtype)
                .HasColumnName(Db.Table.TmpPyPromosyonDetay.Column.DiscountSubtype)
                .HasMaxLength(30);

            builder.Property(x => x.DiscountValue)
                .HasColumnName(Db.Table.TmpPyPromosyonDetay.Column.DiscountValue);

            builder.Property(x => x.MaxQuantity)
                .HasColumnName(Db.Table.TmpPyPromosyonDetay.Column.MaxQuantity);
        }
    }

}
