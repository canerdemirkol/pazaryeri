using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.EntityConfigurations
{
    public class PazarYeriSiparisDetayConfiguration : IEntityTypeConfiguration<PazarYeriSiparisDetay>
    {
        public void Configure(EntityTypeBuilder<PazarYeriSiparisDetay> builder)
        {
            builder.ToTable(Db.Table.PazarYeriSiparisDetay.Name);

            builder.HasKey(x => new { x.Id,x.LineItemId });

            builder.Property(x => x.AlternatifUrunEH).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.AlternatifUrunEH);
            builder.Property(x => x.Barkod).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.Barkod);
            builder.Property(x => x.BrutTutar).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.BrutTutar).HasConversion(decimalValue => (double)decimalValue,
			doubleValue => (decimal)doubleValue);
            builder.Property(x => x.HasSent).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.HasSent);
            builder.Property(x => x.Hata).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.Hata);
            builder.Property(x => x.Id).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.Id);
            builder.Property(x => x.IndirimTutar).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.IndirimTutar).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            builder.Property(x => x.IsAlternativeEH).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.IsAlternativeEh);
            builder.Property(x => x.IsCancelledEH).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.IsCancelledEh);
            builder.Property(x => x.IsCollectedEH).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.IsCollectedEh);
            builder.Property(x => x.KdvOran).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.KdvOran);
            builder.Property(x => x.KdvTutar).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.KdvTutar).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            builder.Property(x => x.LineItemId).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.LineItemId);
            builder.Property(x => x.Miktar).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.Miktar).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            builder.Property(x => x.NetTutar).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.NetTutar).HasConversion(decimalValue => (double)decimalValue, doubleValue => (decimal)doubleValue);
            builder.Property(x => x.ObaseMalNo).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.ObaseMalNo);
            builder.Property(x => x.PaketItemId).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.PaketItemId);
            builder.Property(x => x.ParaBirimiKodu).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.ParaBirimiKodu);
            builder.Property(x => x.PazarYeriBirimId).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.PazarYeriBirimId);
            builder.Property(x => x.PazarYeriMalAdi).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.PazarYeriMalAdi);
            builder.Property(x => x.PazarYeriMalNo).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.PazarYeriMalNo);
            builder.Property(x => x.PazarYeriUrunKodu).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.PazarYeriUrunKodu);
            builder.Property(x => x.ReasonId).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.ReasonId);
            builder.Property(x => x.SatisKampanyaId).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.SatisKampanyaId);
            builder.Property(x => x.SiparisUrunDurumAdi).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.SiparisUrunDurumAdi);
            builder.Property(x => x.UrunBoyutu).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.UrunBoyutu);
            builder.Property(x => x.UrunRengi).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.UrunRengi);
            builder.Property(x => x.Weight).HasColumnName(Db.Table.PazarYeriSiparisDetay.Column.Weight);

        }
    }
}