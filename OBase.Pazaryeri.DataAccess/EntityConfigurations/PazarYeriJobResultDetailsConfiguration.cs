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
    public class PazarYeriJobResultDetailsConfiguration : IEntityTypeConfiguration<PazarYeriJobResultDetails>
    {
        public void Configure(EntityTypeBuilder<PazarYeriJobResultDetails> builder)
        {
            builder.ToTable("PAZAR_YERI_JOB_RESULT_DETAILS");

            builder.Property(x => x.DetailId)
                .HasColumnName("DETAIL_ID")
                .IsRequired() ;

            builder.HasKey(x => new
            {
                x.RefId,
                x.PazarYeriNo,
                x.PazarYeriBirimNo,
                x.PazarYeriMalNo
            });

            builder.Property(x => x.RefId)
             .HasColumnName("REF_ID");


			builder.Property(x => x.PazarYeriNo)
                .HasColumnName("PAZAR_YERI_NO")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.MalNo)
                .HasColumnName("MAL_NO")
                .HasMaxLength(16)
                .IsRequired();

            builder.Property(x => x.PazarYeriMalNo)
                .HasColumnName("PAZAR_YERI_MAL_NO")
                 .IsRequired();

            builder.Property(x => x.PazarYeriBirimNo)
                .HasColumnName("PAZAR_YERI_BIRIM_NO")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Barkod)
                .HasColumnName("BARKOD")
                .HasMaxLength(22)
                .HasColumnType("nvarchar2");

            builder.Property(x => x.SatisFiyat)
                .HasColumnName("SATIS_FIYAT");

            builder.Property(x => x.StokMiktar)
               .HasColumnName("STOK_MIKTAR");

            builder.Property(x => x.SepeteEklenebilirMiktar)
               .HasColumnName("SEPETE_EKLENEBILIR_MIKTAR");

            builder.Property(x => x.IndirimliSatisFiyat)
              .HasColumnName("INDIRIMLI_SATIS_FIYAT");

            builder.Property(x => x.IndirimBaslangicTarih)
              .HasColumnName("INDIRIM_BASLANGIC_TARIH");

            builder.Property(x => x.IndirimBitisTarih)
              .HasColumnName("INDIRIM_BITIS_TARIH");


            builder.Property(x => x.ThreadNo)
              .HasColumnName("THREAD_NO")
              .IsRequired();

            builder.Property(x => x.HasSent)
              .HasColumnName("HAS_SENT")
              .HasMaxLength(1)
              .HasColumnType("nvarchar2")
              .IsRequired();

            builder.Property(x => x.HasErrors)
             .HasColumnName("HAS_ERRORS")
             .HasMaxLength(1)
             .HasColumnType("nvarchar2")
             .IsRequired();


            builder.Property(x => x.Guid)
             .HasColumnName("GUID")
             .HasMaxLength(100)
             .HasColumnType("nvarchar2");

            builder.Property(x => x.HasVerified)
             .HasColumnName("HAS_VERIFIED")
             .HasMaxLength(1)
             .HasColumnType("nvarchar2")
             .IsRequired();

            builder.Property(x => x.AktifPasifEh)
              .HasColumnName("AKTIF_PASIF_EH")
              .HasMaxLength(1)
              .HasColumnType("nvarchar2");


            builder.Property(x => x.Tarih)
              .HasColumnName("TARIH");

            builder.HasOne(x => x.PazarYeriJobResult)
                .WithMany(x => x.PazarYeriJobResultDetails)
                .HasForeignKey(x => x.RefId);

		}
	}
}
