using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class TmpPyPromosyonBirimConfiguration : IEntityTypeConfiguration<TmpPyPromosyonBirim>
    {
        public void Configure(EntityTypeBuilder<TmpPyPromosyonBirim> builder)
        {
            builder.ToTable(Db.Table.TmpPyPromosyonBirim.Name);

            builder.HasNoKey(); 

            builder.Property(x => x.Id)
                .HasColumnName(Db.Table.TmpPyPromosyonBirim.Column.Id);

            builder.Property(x => x.PromosyonNo)
                .HasColumnName(Db.Table.TmpPyPromosyonBirim.Column.PromosyonNo);

            builder.Property(x => x.BirimNo)
                .HasColumnName(Db.Table.TmpPyPromosyonBirim.Column.BirimNo)
                .HasMaxLength(16);
        }
    }

}
