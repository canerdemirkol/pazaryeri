namespace OBase.Pazaryeri.Domain.Dtos.Sale
{
    /// <summary>
    /// DTO for P_KASA_FIS_DETAY procedure parameters
    /// </summary>
    public class CashReceiptDetailDto
    {
        /// <summary>
        /// sequence (satis no)
        /// </summary>
        public long SatisNo { get; set; }

        /// <summary>
        /// default -> null
        /// </summary>
        public string PromosyonNo { get; set; }

        /// <summary>
        /// miktar (gramaj olur 12,3)
        /// </summary>
        public decimal Miktar { get; set; }

        /// <summary>
        /// birim satış fiyatı * miktar (kdv dahil)
        /// </summary>
        public decimal Tutar { get; set; }

        /// <summary>
        /// birim satış fiyatı (kdv dahil)
        /// </summary>
        public decimal Fiyat { get; set; }

        /// <summary>
        /// kdv oranı (numeric)
        /// </summary>
        public decimal KdvOran { get; set; }

        /// <summary>
        /// kdv tutar
        /// </summary>
        public decimal KdvTutar { get; set; }

        /// <summary>
        /// fişe yapılan indirimin ürüne dağıtılmış hali
        /// </summary>
        public decimal? FisIndirimTutar { get; set; }

        /// <summary>
        /// ürüne yapılan indirim tutarı
        /// </summary>
        public decimal? MalIndirimTutar { get; set; }

        /// <summary>
        /// ürün numarası
        /// </summary>
        public string MalNo { get; set; }

        /// <summary>
        /// satış tarihi (satıştaki tarih yazılabilir)
        /// </summary>
        public DateTime Tarih { get; set; }

        /// <summary>
        /// default -> null
        /// </summary>
        public decimal? KazanilanPuan { get; set; }

        /// <summary>
        /// ürün barkodu
        /// </summary>
        public string Barkod { get; set; }

        /// <summary>
        /// default -> null
        /// </summary>
        public string PromosyonTip { get; set; }
    }
}
