using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
	public class GetirCarsiMalTanimConfiguration : IEntityTypeConfiguration<PazaryeriAlternatifGonderim>
	{
		public void Configure(EntityTypeBuilder<PazaryeriAlternatifGonderim> builder)
		{
			builder.ToTable(Db.Table.PazaryeriAlternatifGonderim.Name);

			builder.Property(x => x.PazarYeriBirimNo).HasColumnName(Db.Table.PazaryeriAlternatifGonderim.Column.PazarYeriBirimNo);
			builder.Property(x => x.PazarYeriMalNo).HasColumnName(Db.Table.PazaryeriAlternatifGonderim.Column.PazarYeriMalNo);
			builder.Property(x => x.CatalogProductId).HasColumnName(Db.Table.PazaryeriAlternatifGonderim.Column.CatalogProductId);
			builder.Property(x => x.MenuProductId).HasColumnName(Db.Table.PazaryeriAlternatifGonderim.Column.MenuProductId);
			builder.Property(x => x.OptionId).HasColumnName(Db.Table.PazaryeriAlternatifGonderim.Column.OptionId);
			builder.Property(x => x.ProductImage).HasColumnName(Db.Table.PazaryeriAlternatifGonderim.Column.ProductImage);
			builder.Property(x => x.OptionAmount).HasColumnName(Db.Table.PazaryeriAlternatifGonderim.Column.OptionAmount).HasConversion(decimalValue => (double)decimalValue,
			doubleValue => (decimal)doubleValue);
			builder.Property(x => x.OptionPrice).HasColumnName(Db.Table.PazaryeriAlternatifGonderim.Column.OptionPrice).HasConversion(decimalValue => (double)decimalValue,
			doubleValue => (decimal)doubleValue);
			builder.HasKey(x => new
			{
				x.PazarYeriMalNo,
				x.PazarYeriBirimNo
			});
		}
	}
}
