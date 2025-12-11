using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class MalBarkodConfiguration : IEntityTypeConfiguration<MalBarkod>
	{
		public void Configure(EntityTypeBuilder<MalBarkod> builder)
		{
			builder.ToTable(Db.Table.MalBarkod.Name);
			builder.Property(x => x.Barkod).HasColumnName(Db.Table.MalBarkod.Column.Barkod);
			builder.Property(x => x.MalNo).HasColumnName(Db.Table.MalBarkod.Column.MalNo);
			builder.Property(x => x.Oncelikli).HasColumnName(Db.Table.MalBarkod.Column.Oncelikli);
			builder.HasKey(x => x.Barkod);
		}
	}
}