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
	public class PazarYeriProductImageViewConfiguration : IEntityTypeConfiguration<PazarYeriProductImageView>
	{
		public void Configure(EntityTypeBuilder<PazarYeriProductImageView> builder)
		{
			builder.HasNoKey();
		}
	}
}
