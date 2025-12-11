using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class OrderCancellationStatusConfiguration : IEntityTypeConfiguration<OrderCancellationStatusConfiguration>
    {
        public void Configure(EntityTypeBuilder<OrderCancellationStatusConfiguration> builder)
        {
            builder.HasNoKey();
        }
    }
}
