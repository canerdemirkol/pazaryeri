using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
	public class EmailHareketConfiguration : IEntityTypeConfiguration<EmailHareket>
	{
		public void Configure(EntityTypeBuilder<EmailHareket> builder)
		{
			builder.ToTable(Db.Table.EmailHareket.Name);
			builder.Property(x => x.Type).HasColumnName(Db.Table.EmailHareket.Column.Type);
			builder.Property(x => x.From).HasColumnName(Db.Table.EmailHareket.Column.From);
			builder.Property(x => x.To).HasColumnName(Db.Table.EmailHareket.Column.To);
			builder.Property(x => x.Cc).HasColumnName(Db.Table.EmailHareket.Column.Cc);
			builder.Property(x => x.Subject).HasColumnName(Db.Table.EmailHareket.Column.Subject);
			builder.Property(x => x.Body).HasColumnName(Db.Table.EmailHareket.Column.Body);
			builder.HasKey(x=> new
			{
				x.Type,
				x.From,
				x.To,
				x.Cc,
				x.Subject,
				x.Body
			});
		}
	}
}
