namespace OBase.Pazaryeri.Domain.Dtos.Sale
{
    /// <summary>
    /// DTO for P_KASA_FIS_INDIRIM_DETAY procedure parameters
    /// </summary>
    public class CashReceiptDiscountDetailDto
    {
        /// <summary>
        /// sequence (satis no)
        /// </summary>
        public long SatisNo { get; set; }

        /// <summary>
        /// indirim tip kodu (öncesinde eşleştirme gerekecek)
        /// </summary>
        public string IndirimTip { get; set; }

        /// <summary>
        /// indirim tutarı
        /// </summary>
        public decimal Tutar { get; set; }

        /// <summary>
        /// ürün barkodu (fişe ise null)
        /// </summary>
        public string Barkod { get; set; }

        /// <summary>
        /// ürün numarası (fişe ise null)
        /// </summary>
        public string MalNo { get; set; }
    }
}
