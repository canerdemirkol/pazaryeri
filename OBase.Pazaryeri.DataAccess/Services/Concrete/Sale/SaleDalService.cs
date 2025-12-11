using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Sale;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.Sale;
using OBase.Pazaryeri.Domain.Entities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Sale
{
    public class SaleDalService : BaseDalService, ISaleDalService
    {
        private readonly IOptions<AppSettings> _appSettings;
        public SaleDalService(IRepository repository, IOptions<AppSettings> appSettings) : base(repository)
        {
            _appSettings = appSettings;
        }
        public async Task<long> GetSeqId()
        {
            return await Task.FromResult(
                _repository.ExecuteSqlCommand<SeqIdView>(
                    _appSettings.Value.RawDatabaseQueries.SaleInfoSeqQuery
                ).FirstOrDefault()
                .SeqId
            );
        }


        public async Task InsertCashReceiptAsync(CashReceiptDto cashReceipt)
        {
            var query = _appSettings.Value.RawDatabaseQueries.CashReceiptInsertQuery;

            var parameters = new List<OracleParameter>
            {
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_BAGLANTI_NO",
                    Value = cashReceipt.BaglantiNo ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_SATIS_NO",
                    Value = cashReceipt.SatisNo
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_BIRIM_NO",
                    Value = cashReceipt.BirimNo ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_KASA_NO",
                    Value = cashReceipt.KasaNo ?? "0"
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Date,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_TARIH",
                    Value = cashReceipt.Tarih
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FIS_NO",
                    Value = cashReceipt.FisNo ?? "0"
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FIS_TIP",
                    Value = cashReceipt.FisTip ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FIS_TUTAR",
                    Value = cashReceipt.FisTutar
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_KASIYER_NO",
                    Value = cashReceipt.KasiyerNo ?? "1"
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FIS_KDV_TUTAR",
                    Value = cashReceipt.FisKdvTutar
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_TOPLAM_INDIRIM_TUTAR",
                    Value = cashReceipt.ToplamIndirimTutar
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_MAL_INDIRIM_TUTAR",
                    Value = cashReceipt.MalIndirimTutar
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FIS_SAAT",
                    Value = cashReceipt.FisSaat ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FATURA_AD",
                    Value = cashReceipt.FaturaAd ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FATURA_SOYAD",
                    Value = cashReceipt.FaturaSoyad ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FATURA_TELEFON",
                    Value = cashReceipt.FaturaTelefon ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FIS_BRUT_TUTAR",
                    Value = cashReceipt.FisBrutTutar
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FATURA_ADRES1",
                    Value = cashReceipt.FaturaAdres1 ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FATURA_ADRES2",
                    Value = cashReceipt.FaturaAdres2 ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FATURA_SEMT",
                    Value = cashReceipt.FaturaSemt ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FATURA_IL",
                    Value = cashReceipt.FaturaIl ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_MUSTERI_ID",
                    Value = cashReceipt.MusteriId.HasValue ? (object)cashReceipt.MusteriId.Value : DBNull.Value
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_MUSTERI_NO",
                    Value = cashReceipt.MusteriNo ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_E_FATURA",
                    Value = cashReceipt.EFatura ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_POS_FLAG",
                    Value = cashReceipt.PosFlag
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_IRSALIYE_AD",
                    Value = cashReceipt.IrsaliyeAd ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_IRSALIYE_SOYAD",
                    Value = cashReceipt.IrsaliyeSoyad ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_IRSALIYE_ADRES1",
                    Value = cashReceipt.IrsaliyeAdres1 ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_IRSALIYE_SEMT",
                    Value = cashReceipt.IrsaliyeSemt ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_IRSALIYE_IL",
                    Value = cashReceipt.IrsaliyeIl ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_IRSALIYE_TELEFON",
                    Value = cashReceipt.IrsaliyeTelefon ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_KARGO",
                    Value = string.IsNullOrEmpty(cashReceipt.Kargo) ? DBNull.Value : cashReceipt.Kargo
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_VERGI_NUMARASI",
                    Value = cashReceipt.VergiNumarasi ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_KAZANILAN_PUAN",
                    Value = cashReceipt.KazanilanPuan.HasValue ? (object)cashReceipt.KazanilanPuan.Value : DBNull.Value
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_HARCANAN_PUAN",
                    Value = cashReceipt.HarcananPuan.HasValue ? (object)cashReceipt.HarcananPuan.Value : DBNull.Value
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_WEB_MUSTERI_ID",
                    Value = string.IsNullOrEmpty(cashReceipt.WebMusteriId) ? DBNull.Value : cashReceipt.WebMusteriId
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_VERGI_DAIRESI",
                    Value = cashReceipt.VergiDairesi ?? string.Empty
                }
            }.ToArray();

            await _repository.ExecuteSqlRawAsync(query, parameters);
        }

        public async Task InsertCashReceiptDetailAsync(CashReceiptDetailDto cashReceiptDetail)
        {
            var query = _appSettings.Value.RawDatabaseQueries.CashReceiptDetailInsertQuery;

            var parameters = new List<OracleParameter>
            {
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_SATIS_NO",
                    Value = cashReceiptDetail.SatisNo
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_PROMOSYON_NO",
                    Value = string.IsNullOrEmpty(cashReceiptDetail.PromosyonNo) ? DBNull.Value : cashReceiptDetail.PromosyonNo
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_MIKTAR",
                    Value = cashReceiptDetail.Miktar
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_TUTAR",
                    Value = cashReceiptDetail.Tutar
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FIYAT",
                    Value = cashReceiptDetail.Fiyat
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_KDV_ORAN",
                    Value = cashReceiptDetail.KdvOran
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_KDV_TUTAR",
                    Value = cashReceiptDetail.KdvTutar
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_FIS_INDIRIM_TUTAR",
                    Value = cashReceiptDetail.FisIndirimTutar.HasValue ? (object)cashReceiptDetail.FisIndirimTutar.Value : DBNull.Value
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_MAL_INDIRIM_TUTAR",
                    Value = cashReceiptDetail.MalIndirimTutar.HasValue ? (object)cashReceiptDetail.MalIndirimTutar.Value : DBNull.Value
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_MAL_NO",
                    Value = cashReceiptDetail.MalNo ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Date,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_TARIH",
                    Value = cashReceiptDetail.Tarih
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_KAZANILAN_PUAN",
                    Value = cashReceiptDetail.KazanilanPuan.HasValue ? (object)cashReceiptDetail.KazanilanPuan.Value : DBNull.Value
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_BARKOD",
                    Value = cashReceiptDetail.Barkod ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_PROMOSYON_TIP",
                    Value = string.IsNullOrEmpty(cashReceiptDetail.PromosyonTip) ? DBNull.Value : cashReceiptDetail.PromosyonTip
                }
            }.ToArray();

            await _repository.ExecuteSqlRawAsync(query, parameters);
        }

        public async Task InsertCashReceiptPaymentDetailAsync(CashReceiptPaymentDetailDto cashReceiptPaymentDetail)
        {
            var query = _appSettings.Value.RawDatabaseQueries.CashReceiptPaymentDetailInsertQuery;

            var parameters = new List<OracleParameter>
            {
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_SATIS_NO",
                    Value = cashReceiptPaymentDetail.SatisNo
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_ODEME_TIP",
                    Value = cashReceiptPaymentDetail.OdemeTip ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_ODEME_TUTAR",
                    Value = cashReceiptPaymentDetail.OdemeTutar
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_HESAP_NO",
                    Value = string.IsNullOrEmpty(cashReceiptPaymentDetail.HesapNo) ? DBNull.Value : cashReceiptPaymentDetail.HesapNo
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_TAKSIT_SAYISI",
                    Value = cashReceiptPaymentDetail.TaksitSayisi.HasValue ? (object)cashReceiptPaymentDetail.TaksitSayisi.Value : DBNull.Value
                }
            }.ToArray();

            await _repository.ExecuteSqlRawAsync(query, parameters);
        }

        public async Task InsertCashReceiptDiscountDetailAsync(CashReceiptDiscountDetailDto cashReceiptDiscountDetail)
        {
            var query = _appSettings.Value.RawDatabaseQueries.CashReceiptDiscountDetailInsertQuery;

            var parameters = new List<OracleParameter>
            {
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_SATIS_NO",
                    Value = cashReceiptDiscountDetail.SatisNo
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_INDIRIM_TIP",
                    Value = cashReceiptDiscountDetail.IndirimTip ?? string.Empty
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Decimal,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_TUTAR",
                    Value = cashReceiptDiscountDetail.Tutar
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "BARKOD",
                    Value = string.IsNullOrEmpty(cashReceiptDiscountDetail.Barkod) ? DBNull.Value : cashReceiptDiscountDetail.Barkod
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "MAL_NO",
                    Value = string.IsNullOrEmpty(cashReceiptDiscountDetail.MalNo) ? DBNull.Value : cashReceiptDiscountDetail.MalNo
                }
            }.ToArray();

            await _repository.ExecuteSqlRawAsync(query, parameters);
        }
        public async Task InsertEmailHareketAsync(string subject, string body)
        {
            var query = _appSettings.Value.RawDatabaseQueries.EmailHareketInsertQuery;

            var parameters = new List<OracleParameter> {
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Clob,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_BODY",
                    Value = body
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_KONU",
                    Value = subject
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_CC",
                    Value = _appSettings.Value.MailSettings.CC ?? ""
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_NEREDEN",
                    Value = _appSettings.Value.MailSettings.From ?? ""
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_NEREYE",
                    Value = _appSettings.Value.MailSettings.To ?? ""
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_TIP",
                    Value = "01"
                }}.ToArray();

            await _repository.ExecuteSqlRawAsync(query, parameters);

        }

      
    }
}
