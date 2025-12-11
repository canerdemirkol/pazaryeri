using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriJobResultConfiguration : IEntityTypeConfiguration<PazarYeriJobResult>
    {
        public void Configure(EntityTypeBuilder<PazarYeriJobResult> builder)
        {
            builder.ToTable("PAZAR_YERI_JOB_RESULT");

            builder.HasKey(x => x.RefId);

            builder.Property(x => x.JobType)
                .HasColumnName("JOB_TYPE")
                .IsRequired();

            builder.Property(x => x.RefId)
                .HasColumnName("REF_ID");

			builder.Property(x => x.ObaseLogId)
                .HasColumnName("OBASE_LOG_ID");

            builder.Property(x => x.ThreadSize)
                .HasColumnName("THREAD_SIZE");

            builder.Property(x => x.NumberOfThreads)
                .HasColumnName("NUMBER_OF_THREADS");

            builder.Property(x => x.HasSent)
                .HasColumnName("HAS_SENT")
                .IsRequired();

            builder.Property(x => x.HasErrors)
                .HasColumnName("HAS_ERRORS")
                .IsRequired();

            builder.Property(x => x.InsertDatetime)
                .HasColumnName("INSERT_DATETIME").HasColumnType("DATE")
                .IsRequired();

            //builder.Property(x => x.PazarYeriNo)
            //   .HasColumnName("PAZAR_YERI_NO")
            //   .HasMaxLength(10)
            //   .HasColumnType("nvarchar2");

        }
    }
}