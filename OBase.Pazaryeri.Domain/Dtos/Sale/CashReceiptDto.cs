namespace OBase.Pazaryeri.Domain.Dtos.Sale
{
    /// <summary>
    /// DTO for P_KASA_FIS_TANIM procedure parameters
    /// </summary>
    public class CashReceiptDto
    {
        /// <summary>
        /// eticaret satış no
        /// </summary>
        public string BaglantiNo { get; set; }

        /// <summary>
        /// sequence
        /// </summary>
        public long SatisNo { get; set; }

        /// <summary>
        /// birim
        /// </summary>
        public string BirimNo { get; set; }

        /// <summary>
        /// default -> 0
        /// </summary>
        public string KasaNo { get; set; } = "0";

        /// <summary>
        /// satışın yapıldığı tarih (dateonly)
        /// </summary>
        public DateTime Tarih { get; set; }

        /// <summary>
        /// default -> 0
        /// </summary>
        public string FisNo { get; set; } = "0";

        /// <summary>
        /// SATIŞLARDA NN, IADE NI
        /// </summary>
        public string FisTip { get; set; }

        /// <summary>
        /// indirim düşülmüş hali (kdv dahil)
        /// </summary>
        public decimal FisTutar { get; set; }

        /// <summary>
        /// default -> 1
        /// </summary>
        public string KasiyerNo { get; set; } = "1";

        /// <summary>
        /// toplam kdv tutar
        /// </summary>
        public decimal FisKdvTutar { get; set; }

        /// <summary>
        /// fiş indirim toplamı
        /// </summary>
        public decimal ToplamIndirimTutar { get; set; }

        /// <summary>
        /// ürün indirim toplamı
        /// </summary>
        public decimal MalIndirimTutar { get; set; }

        /// <summary>
        /// satışın yapıldığı saat:dk (time)
        /// </summary>
        public string FisSaat { get; set; }

        /// <summary>
        /// müşteri ad - zorunlu alan değil fakat earsiv süreci için gerekli
        /// </summary>
        public string FaturaAd { get; set; }

        /// <summary>
        /// müşteri soyad
        /// </summary>
        public string FaturaSoyad { get; set; }

        /// <summary>
        /// müşteri telefon (format yok)
        /// </summary>
        public string FaturaTelefon { get; set; }

        /// <summary>
        /// indirim tutar + net tutar
        /// </summary>
        public decimal FisBrutTutar { get; set; }

        /// <summary>
        /// fatura adres
        /// </summary>
        public string FaturaAdres1 { get; set; }

        /// <summary>
        /// fatura adres 2
        /// </summary>
        public string FaturaAdres2 { get; set; }

        /// <summary>
        /// ilçe
        /// </summary>
        public string FaturaSemt { get; set; }

        /// <summary>
        /// il
        /// </summary>
        public string FaturaIl { get; set; }

        /// <summary>
        /// default -> null
        /// </summary>
        public long? MusteriId { get; set; }

        /// <summary>
        /// müşteri kart no
        /// </summary>
        public string MusteriNo { get; set; }

        /// <summary>
        /// earsiv veya efatura kullanıyor mu (E evet, H hayır)
        /// </summary>
        public string EFatura { get; set; }

        /// <summary>
        /// default -> 2
        /// </summary>
        public int PosFlag { get; set; } = 2;

        /// <summary>
        /// teslimat müşteri ad
        /// </summary>
        public string IrsaliyeAd { get; set; }

        /// <summary>
        /// teslimat müşteri soyad
        /// </summary>
        public string IrsaliyeSoyad { get; set; }

        /// <summary>
        /// teslimat adres 1
        /// </summary>
        public string IrsaliyeAdres1 { get; set; }

        /// <summary>
        /// teslimat ilçe
        /// </summary>
        public string IrsaliyeSemt { get; set; }

        /// <summary>
        /// teslimat il
        /// </summary>
        public string IrsaliyeIl { get; set; }

        /// <summary>
        /// teslimat tel
        /// </summary>
        public string IrsaliyeTelefon { get; set; }

        /// <summary>
        /// default -> null
        /// </summary>
        public string Kargo { get; set; }

        /// <summary>
        /// vergi no veya tckn
        /// </summary>
        public string VergiNumarasi { get; set; }

        /// <summary>
        /// default -> null
        /// </summary>
        public decimal? KazanilanPuan { get; set; }

        /// <summary>
        /// default -> null
        /// </summary>
        public decimal? HarcananPuan { get; set; }

        /// <summary>
        /// default -> null
        /// </summary>
        public string WebMusteriId { get; set; }

        /// <summary>
        /// vergi dairesi
        /// </summary>
        public string VergiDairesi { get; set; }
    }
}
