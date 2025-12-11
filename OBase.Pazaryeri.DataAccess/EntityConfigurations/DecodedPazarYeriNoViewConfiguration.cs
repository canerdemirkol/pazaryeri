using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
	public class DecodedPazarYeriNoViewConfiguration : IEntityTypeConfiguration<DecodedPazarYeriNoView>
	{
		public void Configure(EntityTypeBuilder<DecodedPazarYeriNoView> builder)
		{
			builder.HasNoKey();
		}
	}
}
