using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Dtos.AkilliETicaret
{
    /// <summary>
    /// Batch Product Data DTO
    /// </summary>
    public class AkilliETicaretBatchProductResponseDto
    {
        /// <summary>
        /// Toplam istek sayısı
        /// </summary>
        public int TotalRequested { get; set; }

        /// <summary>
        /// Başarılı işlem sayısı
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Hatalı işlem sayısı
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Ürün bazlı detaylar
        /// </summary>
        public List<AkilliETicaretBatchProductDetailDto> Details { get; set; }
    }
    /// <summary>
    /// Batch Product Detail DTO
    /// </summary>
    public class AkilliETicaretBatchProductDetailDto
    {
        /// <summary>
        /// Stok kodu
        /// </summary>
        public string StockCode { get; set; }

        /// <summary>
        /// İşlem başarılı mı?
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// İşlem mesajı
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// İşlem sonucu data (opsiyonel - sadece başarılı işlemlerde)
        /// </summary>
        public AkilliETicaretBatchProductResultDto Data { get; set; }
    }

    /// <summary>
    /// Batch Product Result DTO
    /// </summary>
    public class AkilliETicaretBatchProductResultDto
    {
        /// <summary>
        /// Ürün ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Uyarılar (opsiyonel)
        /// </summary>
        public List<string> Warnings { get; set; }

        /// <summary>
        /// Depo işlem raporu
        /// </summary>
        public AkilliETicaretWarehouseReportDto WarehouseReport { get; set; }
    }
    /// <summary>
    /// Warehouse Report DTO
    /// </summary>
    public class AkilliETicaretWarehouseReportDto
    {
        /// <summary>
        /// Başarılı depolar
        /// </summary>
        public List<string> SuccessWarehouses { get; set; }

        /// <summary>
        /// Hatalı depolar
        /// </summary>
        public List<AkilliETicaretWarehouseDto> FailedWarehouses { get; set; }
    }

    /// <summary>
    /// Failed Warehouse DTO
    /// </summary>
    public class AkilliETicaretWarehouseDto
    {
        /// <summary>
        /// Depo kodu
        /// </summary>
        public string WarehouseCode { get; set; }

        /// <summary>
        /// Hata detayı
        /// </summary>
        public string Detail { get; set; }
    }
}
