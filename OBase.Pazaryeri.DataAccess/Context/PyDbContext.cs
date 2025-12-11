using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Context
{
    public class PyDbContext : DbContext, IDbContext
    {
        public PyDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }

        DbSet<TEntity> IDbContext.Set<TEntity>()
        {
            return base.Set<TEntity>();
        }
        public DbSet<PazarYeriJobResult> PazarYeriJobResults { get; set; }
        public DbSet<PazarYeriJobResultDetails> PazarYeriJobResultDetails { get; set; }
        public DbSet<PazarYeriSiparisIade> PazarYeriSiparisIadeler { get; set; }
        public DbSet<PazarYeriSiparisIadeDetay> PazarYeriSiparisIadeDetaylar { get; set; }
        public DbSet<ReturnClaimProductInfo> ReturnClaimProductInfos { get; set; }
        public DbSet<SeqIdView> SeqIdView { get; set; }
        public DbSet<BirimTanim> BirimTanim { get; set; }
        public DbSet<PazarYeriBirimTanim> PazarYeriBirimTanim { get; set; }
        public DbSet<PazarYeriSiparis> PazarYeriSiparis { get; set; }
        public DbSet<PazarYeriSiparisDetay> PazarYeriSiparisDetay { get; set; }
        public DbSet<PazarYeriSiparisEkBilgi> PazarYeriSiparisEkBilgi { get; set; }
        public DbSet<PazarYeriSiparisUrun> PazarYeriSiparisUrun { get; set; }
        public DbSet<PazarYeriMalTanim> PazarYeriMalTanim { get; set; }
        public DbSet<PazarYeriKargoAdres> PazarYeriKargoAdres { get; set; }
        public DbSet<PazarYeriFaturaAdres> PazarYeriFaturaAdres { get; set; }
        public DbSet<PazarYeriAktarim> PazarYeriAktarim { get; set; }
        public DbSet<PazarYeriLog> PazarYeriLog { get; set; }
        public DbSet<MalBarkod> MalBarkod { get; set; }
        public DbSet<DecodedPazarYeriNoView> DecodedPazarYeriNoView { get; set; }
        public DbSet<VPazaryeriUrunler> VPazaryeriUrunler { get; set; }
        public DbSet<PazarYeriProductImageView> PazarYeriProductImageView { get; set; }
        public DbSet<PazaryeriAlternatifGonderim> GetirCarsiMalTanim { get; set; }
        public DbSet<EmailHareket> EmailHareket { get; set; }
        public DbSet<SiparisGunSonuRaporuView> SiparisGunSonuRaporu { get; set; }
        public DbSet<PyPromosyonEntegrasyon> PyPromosyonEntegrasyonlar { get; set; }
        public DbSet<PyPromosyonTanim> PyPromosyonTanimlar { get; set; }
        public DbSet<TmpPyPromosyonTanim> TmpPyPromosyonTanimlar { get; set; }
        public DbSet<TmpPyPromosyonDetay> TmpPyPromosyonDetaylar { get; set; }
        public DbSet<TmpPyPromosyonBirim> TmpPyPromosyonBirimler { get; set; }

    }
}