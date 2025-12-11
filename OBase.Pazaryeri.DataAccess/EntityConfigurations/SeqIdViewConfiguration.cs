using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
	public class SeqIdViewConfiguration : IEntityTypeConfiguration<SeqIdView>
	{
		public void Configure(EntityTypeBuilder<SeqIdView> builder)
		{
			builder.HasNoKey();
		}
	}
}
