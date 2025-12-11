namespace OBase.Pazaryeri.Domain.Dtos.AkilliETicaret
{
    /// <summary>
    /// </summary>
    public class AkilliETicaretProductMasterUpsertDto
    {
        /// <summary>
        /// Stok kodu (zorunlu)
        /// </summary>
        public string StockCode { get; set; }

        /// <summary>
        /// Stok adı (opsiyonel)
        /// </summary>
        public string StockName { get; set; }

        /// <summary>
        /// Fiyat (opsiyonel)
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Üretici kodu - GTIN (opsiyonel)
        /// </summary>
        public string ManufacturerCode { get; set; }

        /// <summary>
        /// Üretici kodu 2 - MPN (opsiyonel)
        /// </summary>
        public string ManufacturerCode2 { get; set; }

        /// <summary>
        /// KDV oranı (opsiyonel)
        /// </summary>
        public decimal? Vat { get; set; }

        /// <summary>
        /// Birim (opsiyonel, örn: "AD", "KG")
        /// </summary>
        public string UnitStr { get; set; }

        /// <summary>
        /// Marka adı - isimden bulacak/oluşturacak (opsiyonel)
        /// </summary>
        public string BrandName { get; set; }

        /// <summary>
        /// Marka ID - direkt ID kullanacak, performanslı (opsiyonel)
        /// </summary>
        public int? BrandId { get; set; }

        /// <summary>
        /// Kategori adı - isimden bulacak/oluşturacak (opsiyonel)
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Kategori ID - direkt ID kullanacak, performanslı (opsiyonel)
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Depo stok bilgileri (opsiyonel)
        /// </summary>
        public List<AkilliETicaretInventoryDto> Inventory { get; set; }
    }
    /// <summary>
    /// Depo stok bilgisi
    /// </summary>
    public class AkilliETicaretInventoryDto
    {
        /// <summary>
        /// Depo kodu
        /// </summary>
        public string WarehouseCode { get; set; }

        /// <summary>
        /// Miktar
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Satış fiyatı
        /// </summary>
        public decimal SalePrice { get; set; }
    }
}
