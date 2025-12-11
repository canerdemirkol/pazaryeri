using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriLogConfiguration : IEntityTypeConfiguration<PazarYeriLog>
    {
        public void Configure(EntityTypeBuilder<PazarYeriLog> builder)
        {
            builder.ToTable("PAZAR_YERI_LOG");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("ID")
                .IsRequired();

            builder.Property(x => x.RefId)
                .HasColumnName("REF_ID")
                .IsRequired();

            builder.Property(x => x.PazarYeriNo)
                .HasColumnName("PAZAR_YERI_NO")
                .HasMaxLength(50)
                .HasColumnType("nvarchar2");

            builder.Property(x => x.PazarYeriBirimNo)
                .HasColumnName("PAZAR_YERI_BIRIM_NO")
                .HasMaxLength(50)
                .HasColumnType("nvarchar2");

            builder.Property(x => x.ThreadNo)
                .HasColumnName("THREAD_NO");

            builder.Property(x => x.LogType)
                .HasColumnName("LOG_TYPE")
                .HasMaxLength(50)
                .HasColumnType("nvarchar2");

            builder.Property(x => x.Request)
                .HasColumnName("REQUEST")
                .HasColumnType("CLOB");

            builder.Property(x => x.Response)
                .HasColumnName("RESPONSE")
                .HasColumnType("CLOB");

            builder.Property(x => x.HasErrors)
                .HasColumnName("HAS_ERRORS")
                .HasColumnType("nvarchar2");

            builder.Property(x => x.ExecutionType)
              .HasColumnName("EXECUTION_TYPE")
              .HasMaxLength(10)
              .HasColumnType("nvarchar2");

            builder.Property(x => x.DetailId)
              .HasColumnName("DETAIL_ID");

            builder.Property(x => x.Guid)
             .HasColumnName("GUID");
        }
    }
}