using OBase.Pazaryeri.Domain.Dtos.Sale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Domain.Extensions
{
    public static class SaleInfoDtoExtensions
    {
        /// <summary>
        /// SaleInfoDto'yu P_KASA_FIS_TANIM procedure'ü için CashReceiptDto'ya dönüştürür
        /// </summary>
        /// <param name="saleInfo">Satış bilgisi</param>
        /// <param name="satisNo">Veritabanından alınan sequence numarası</param>
        /// <returns>CashReceiptDto</returns>
        public static CashReceiptDto ToCashReceiptDto(this SaleInfoDto saleInfo, long satisNo)
        {
            if (saleInfo == null)
                throw new ArgumentNullException(nameof(saleInfo));

            // Calculate brut tutar (toplam indirim + net tutar)
            var fisBrutTutar = saleInfo.TotalPrice + saleInfo.TotalReceiptDiscount + saleInfo.TotalProductDiscount;

            // (NN for sales, NI for returns)
            var fisTip = saleInfo.Status?.ToUpper() == "RETURN" ? "NI" : "NN";

            // Format time as HH:mm
            var fisSaat = saleInfo.SaleDateUtc.ToString("HH:mm");

            return new CashReceiptDto
            {
                // Connection and sale info
                BaglantiNo = saleInfo.OrderId,
                SatisNo = satisNo,
                BirimNo = saleInfo.StoreCode,
                Tarih = saleInfo.SaleDateUtc.Date,
                FisSaat = fisSaat,
                FisTip = fisTip,

                // Default values
                KasaNo = "0",
                FisNo = "0",
                KasiyerNo = "1",
                PosFlag = 2,

                // Financial info
                FisTutar = saleInfo.TotalPrice,
                FisKdvTutar = saleInfo.TotalTaxPrice,
                ToplamIndirimTutar = saleInfo.TotalReceiptDiscount,
                MalIndirimTutar = saleInfo.TotalProductDiscount,
                FisBrutTutar = fisBrutTutar,

                // Customer info
                FaturaAd = saleInfo.Customer?.Name ?? string.Empty,
                FaturaSoyad = saleInfo.Customer?.LastName ?? string.Empty,
                FaturaTelefon = saleInfo.Customer?.PhoneNumber ?? string.Empty,
                MusteriNo = saleInfo.Customer?.CardNo ?? string.Empty,
                WebMusteriId = saleInfo.Customer?.CustomerId ?? string.Empty,

                // Billing address
                FaturaAdres1 = saleInfo.BillingAddress?.Address ?? string.Empty,
                FaturaAdres2 = saleInfo.BillingAddress?.Address2 ?? string.Empty,
                FaturaSemt = saleInfo.BillingAddress?.District ?? string.Empty,
                FaturaIl = saleInfo.BillingAddress?.City ?? string.Empty,
                VergiNumarasi = saleInfo.BillingAddress?.TaxNo ?? string.Empty,
                VergiDairesi = saleInfo.BillingAddress?.TaxOffice ?? string.Empty,

                // Shipping address (Irsaliye)
                IrsaliyeAd = saleInfo.Customer?.Name ?? string.Empty,
                IrsaliyeSoyad = saleInfo.Customer?.LastName ?? string.Empty,
                IrsaliyeAdres1 = saleInfo.ShippingAddress?.Address ?? string.Empty,
                IrsaliyeSemt = saleInfo.ShippingAddress?.District ?? string.Empty,
                IrsaliyeIl = saleInfo.ShippingAddress?.City ?? string.Empty,
                IrsaliyeTelefon = saleInfo.ShippingAddress?.PhoneNumber ?? string.Empty,

                // E-Fatura (default to H - hayır)
                EFatura = "H",

                // Nullable fields
                MusteriId = null,
                Kargo = null,
                KazanilanPuan = null,
                HarcananPuan = null
            };
        }

        /// <summary>
        /// ItemDto'yu P_KASA_FIS_DETAY procedure'ü için CashReceiptDetailDto'ya dönüştürür
        /// </summary>
        /// <param name="item">Satış kalemi bilgisi</param>
        /// <param name="satisNo">Veritabanından alınan sequence numarası</param>
        /// <param name="tarih">Satış tarihi</param>
        /// <returns>CashReceiptDetailDto</returns>
        public static CashReceiptDetailDto ToCashReceiptDetailDto(this ItemDto item, long satisNo, DateTime tarih)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return new CashReceiptDetailDto
            {
                SatisNo = satisNo,
                PromosyonNo = null,
                Miktar = item.Quantity,
                Tutar = item.Total,
                Fiyat = item.Price,
                KdvOran = item.VatRate,
                KdvTutar = item.TotalVat,
                FisIndirimTutar = item.ReceiptDiscount ?? 0,
                MalIndirimTutar = item.ProductDiscount ?? 0,
                MalNo = item.ProductCode,
                Tarih = tarih.Date,
                KazanilanPuan = null,
                Barkod = item.Barcode,
                PromosyonTip = null
            };
        }

        /// <summary>
        /// PaymentDto'yu P_KASA_FIS_ODEME_DETAY procedure'ü için CashReceiptPaymentDetailDto'ya dönüştürür
        /// </summary>
        /// <param name="payment">Ödeme bilgisi</param>
        /// <param name="satisNo">Veritabanından alınan sequence numarası</param>
        /// <returns>CashReceiptPaymentDetailDto</returns>
        public static CashReceiptPaymentDetailDto ToCashReceiptPaymentDetailDto(this PaymentDto payment, long satisNo)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            return new CashReceiptPaymentDetailDto
            {
                SatisNo = satisNo,
                OdemeTip = payment.Code ?? string.Empty,
                OdemeTutar = payment.Total ?? 0,
                HesapNo = null,
                TaksitSayisi = payment.InstallmentCount
            };
        }

        /// <summary>
        /// DiscountDto'yu P_KASA_FIS_INDIRIM_DETAY procedure'ü için CashReceiptDiscountDetailDto'ya dönüştürür
        /// </summary>
        /// <param name="discount">İndirim bilgisi</param>
        /// <param name="satisNo">Veritabanından alınan sequence numarası</param>
        /// <returns>CashReceiptDiscountDetailDto</returns>
        public static CashReceiptDiscountDetailDto ToCashReceiptDiscountDetailDto(this DiscountDto discount, long satisNo)
        {
            if (discount == null)
                throw new ArgumentNullException(nameof(discount));

            return new CashReceiptDiscountDetailDto
            {
                SatisNo = satisNo,
                IndirimTip = discount.Code ?? string.Empty,
                Tutar = discount.Total ?? 0,
                Barkod = discount.Barcode ?? null,
                MalNo = discount.ProductCode ?? null
            };
        }
    }
}
