namespace OBase.Pazaryeri.Domain.Dtos.Sale
{
    /// <summary>
    /// DTO for P_KASA_FIS_ODEME_DETAY procedure parameters
    /// </summary>
    public class CashReceiptPaymentDetailDto
    {
        /// <summary>
        /// sequence (satis no)
        /// </summary>
        public long SatisNo { get; set; }

        /// <summary>
        /// ödeme kodu (öncesinde eşleştirme gerekecek)
        /// </summary>
        public string OdemeTip { get; set; }

        /// <summary>
        /// tutar
        /// </summary>
        public decimal OdemeTutar { get; set; }

        /// <summary>
        /// default -> null
        /// </summary>
        public string HesapNo { get; set; }

        /// <summary>
        /// taksit sayısı
        /// </summary>
        public int? TaksitSayisi { get; set; }
    }
}
