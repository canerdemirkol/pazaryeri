using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace OBase.Pazaryeri.Domain.Constants
{
    public static class Constants
    {
        public static class Db
        {
            public static class Table
            {
                public static class PazarYeriSiparisIade
                {
                    public const string Name = "PAZAR_YERI_SIPARIS_IADE";
                    public static class Column
                    {
                        public const string BirimAciklama = "BIRIM_ACIKLAMA";
                        public const string ClaimId = "CLAIM_ID";
                        public const string ClaimStatus = "CLAIM_STATUS";
                        public const string ClaimTarih = "CLAIM_TARIH";
                        public const string DepoAktarildiEH = "DEPO_AKTARILDI_EH";
                        public const string Id = "ID";
                        public const string MusteriAd = "MUSTERI_AD";
                        public const string MusteriSoyad = "MUSTERI_SOYAD";
                        public const string PazarYeriNo = "PAZAR_YERI_NO";
                        public const string PazarYeriBirimNo = "PAZAR_YERI_BIRIM_NO";
                        public const string ReturnedSellerEH = "RETURNED_SELLER_EH";
                        public const string SiparisNo = "SIPARIS_NO";
                        public const string SiparisPaketId = "SIPARIS_PAKET_ID";
                        public const string SiparisTarih = "SIPARIS_TARIH";
                        public const string DepoAktarimDenemeSayisi = "DEPO_AKTARIM_DENEME_SAYISI";
                    }
                }
                public static class PazaryeriAlternatifGonderim
                {
                    public const string Name = "PAZAR_YERI_ALTERNATIF_GONDERIM";
                    public static class Column
                    {
                        public const string PazarYeriBirimNo = "PAZAR_YERI_BIRIM_NO";
                        public const string PazarYeriMalNo = "BARKOD";
                        public const string CatalogProductId = "CATALOGPRODUCTID";
                        public const string MenuProductId = "MENUPRODUCTID";
                        public const string OptionId = "OPTIONID";
                        public const string OptionAmount = "OPTIONAMOUNT";
                        public const string OptionPrice = "OPTIONPRICE";
                        public const string ProductImage = "IMAGE";
                    }
                }
                public static class PazarYeriSiparisIadeDetay
                {
                    public const string Name = "PAZAR_YERI_SIPARIS_IADE_DETAY";
                    public static class Column
                    {
                        public const string Aciklama = "ACIKLAMA";
                        public const string ClaimDetayId = "CLAIM_DETAY_ID";
                        public const string ClaimId = "CLAIM_ID";
                        public const string ClaimItemStatus = "CLAIM_ITEM_STATUS";
                        public const string CozumlendiEH = "COZUMLENDI_EH";
                        public const string MusteriIadeSebepAd = "MUSTERI_IADE_SEBEP_AD";
                        public const string MusteriIadeSebepId = "MUSTERI_IADE_SEBEP_ID";
                        public const string MusteriIadeSebepKod = "MUSTERI_IADE_SEBEP_KOD";
                        public const string MusteriNot = "MUSTERI_NOT";
                        public const string OrderLineItemId = "ORDER_LINE_ITEM_ID";
                        public const string PyIadeSebepAd = "PY_IADE_SEBEP_AD";
                        public const string PyIadeSebepId = "PY_IADE_SEBEP_ID";
                        public const string PyIadeSebepKod = "PY_IADE_SEBEP_KOD";
                        public const string ClaimImageUrls = "IMAGE_URLS";
                        public const string Miktar = "MIKTAR";
                        public const string Sayisi = "SAYISI";
                    }
                }
                public static class MalBarkod
                {
                    public const string Name = "MAL_BARKOD";
                    public static class Column
                    {
                        public const string Barkod = "BARKOD";
                        public const string MalNo = "MAL_NO";
                        public const string Oncelikli = "ONCELIKLI";

                    }
                }
                public static class PazarYeriSiparis
                {
                    public const string Name = "PAZAR_YERI_SIPARIS";
                    public static class Column
                    {
                        public const string BrutTutar = "BRUT_TUTAR";
                        public const string DepoAktarildiEh = "DEPO_AKTARILDI_EH";
                        public const string Desi = "DESI";
                        public const string FaturaAdresId = "FATURA_ADRES_ID";
                        public const string HasSent = "HAS_SENT";
                        public const string Hata = "HATA";
                        public const string Id = "ID";
                        public const string InsertDatetime = "INSERT_DATETIME";
                        public const string InsertUser = "INSERT_USER";
                        public const string KargoAdresId = "KARGO_ADRES_ID";
                        public const string KargoGondericiNumarasi = "KARGO_GONDERICI_NUMARASI";
                        public const string KargoSaglayiciAdi = "KARGO_SAGLAYICI_ADI";
                        public const string KargoTakipNo = "KARGO_TAKIP_NO";
                        public const string KargoTakipUrl = "KARGO_TAKIP_URL";
                        public const string KoliAdeti = "KOLI_ADETI";
                        public const string MusteriAdi = "MUSTERI_ADI";
                        public const string MusteriEmail = "MUSTERI_EMAIL";
                        public const string MusteriId = "MUSTERI_ID";
                        public const string MusteriSoyadi = "MUSTERI_SOYADI";
                        public const string ObaseSiparisNo = "OBASE_SIPARIS_NO";
                        public const string PaketId = "PAKET_ID";
                        public const string ParaBirimiKodu = "PARA_BIRIMI_KODU";
                        public const string PazarYeriNo = "PAZAR_YERI_NO";
                        public const string PosetSayisi = "POSET_SAYISI";
                        public const string SevkiyatPaketDurumu = "SEVKIYAT_PAKET_DURUMU";
                        public const string SiparisId = "SIPARIS_ID";
                        public const string SiparisNo = "SIPARIS_NO";
                        public const string SiparisTarih = "SIPARIS_TARIH";
                        public const string TahminiTeslimBaslangicTarih = "TAHMINI_TESLIM_BASLANGIC_TARIH";
                        public const string TahminiTeslimBitisTarih = "TAHMINI_TESLIM_BITIS_TARIH";
                        public const string TcKimlikNo = "TC_KIMLIK_NO";
                        public const string TeslimatAdresTipi = "TESLIMAT_ADRES_TIPI";
                        public const string TeslimatTarihi = "TESLIMAT_TARIHI";
                        public const string ToplamIndirimTutar = "TOPLAM_INDIRIM_TUTAR";
                        public const string ToplamTutar = "TOPLAM_TUTAR";
                        public const string UpdateDatetime = "UPDATE_DATETIME";
                        public const string UpdateUser = "UPDATE_USER";
                        public const string VergiDairesi = "VERGI_DAIRESI";
                        public const string VergiNumarasi = "VERGI_NUMARASI";
                        public const string MaksTutar = "MAKS_TUTAR";
                        public const string MinTutar = "MIN_TUTAR";

                    }
                }
                public static class PazarYeriSiparisDetay
                {
                    public const string Name = "PAZAR_YERI_SIPARIS_DETAY";
                    public static class Column
                    {
                        public const string AlternatifUrunEH = "ALTERNATIF_URUN_EH";
                        public const string Barkod = "BARKOD";
                        public const string BrutTutar = "BRUT_TUTAR";
                        public const string HasSent = "HAS_SENT";
                        public const string Hata = "HATA";
                        public const string Id = "ID";
                        public const string IndirimTutar = "INDIRIM_TUTAR";
                        public const string InsertDatetime = "INSERT_DATETIME";
                        public const string InsertUser = "INSERT_USER";
                        public const string IsAlternativeEh = "ISALTERNATIVE_EH";
                        public const string IsCancelledEh = "ISCANCELLED_EH";
                        public const string IsCollectedEh = "ISCOLLECTED_EH";
                        public const string KdvOran = "KDV_ORAN";
                        public const string KdvTutar = "KDV_TUTAR";
                        public const string LineItemId = "LINE_ITEM_ID";
                        public const string Miktar = "MIKTAR";
                        public const string NetTutar = "NET_TUTAR";
                        public const string ObaseMalNo = "OBASE_MAL_NO";
                        public const string PaketItemId = "PAKET_ITEM_ID";
                        public const string ParaBirimiKodu = "PARA_BIRIMI_KODU";
                        public const string PazarYeriBirimId = "PAZAR_YERI_BIRIM_ID";
                        public const string PazarYeriMalAdi = "PAZAR_YERI_MAL_ADI";
                        public const string PazarYeriMalNo = "PAZAR_YERI_MAL_NO";
                        public const string PazarYeriUrunKodu = "PAZAR_YERI_URUN_KODU";
                        public const string ReasonId = "REASON_ID";
                        public const string SatisKampanyaId = "SATIS_KAMPANYA_ID";
                        public const string SiparisUrunDurumAdi = "SIPARIS_URUN_DURUM_ADI";
                        public const string UpdateDatetime = "UPDATE_DATETIME";
                        public const string UpdateUser = "UPDATE_USER";
                        public const string UrunBoyutu = "URUN_BOYUTU";
                        public const string UrunRengi = "URUN_RENGI";
                        public const string Weight = "WEIGHT";


                    }
                }
                public static class PazarYeriSiparisEkBilgi
                {
                    public const string Name = "PAZAR_YERI_SIPARIS_EK_BILGI";
                    public static class Column
                    {
                        public const string GuncelFaturaTutar = "GUNCEL_FATURA_TUTAR";
                        public const string ObaseSiparisId = "OBASE_SIPARIS_ID";
                        public const string PosetSayisi = "POSET_SAYISI";
                        public const string PosetTutari = "POSET_TUTARI";
                        public const string PySiparisNo = "PY_SIPARIS_NO";
                        public const string GonderimUcreti = "GONDERIM_UCRETI";
                    }
                }
                public static class PazarYeriSiparisUrun
                {
                    public const string Name = "PAZAR_YERI_SIPARIS_URUN";
                    public static class Column
                    {
                        public const string AltUrunMiktar = "ALT_URUN_MIKTAR";
                        public const string AltUrunObaseMalNo = "ALT_URUN_OBASE_MAL_NO";
                        public const string AltUrunPazarYeriMalNo = "ALT_URUN_PAZAR_YERI_MAL_NO";
                        public const string GuncelMiktar = "GUNCEL_MIKTAR";
                        public const string ImageUrl = "IMAGE_URL";
                        public const string IsalternativeEh = "ISALTERNATIVE_EH";
                        public const string IscancelledEh = "ISCANCELLED_EH";
                        public const string IscollectedEh = "ISCOLLECTED_EH";
                        public const string Miktar = "MIKTAR";
                        public const string ObaseMalNo = "OBASE_MAL_NO";
                        public const string ObaseSiparisId = "OBASE_SIPARIS_ID";
                        public const string PazarYeriBirimId = "PAZAR_YERI_BIRIM_ID";
                        public const string PazarYeriMalNo = "PAZAR_YERI_MAL_NO";
                        public const string PySiparisNo = "PY_SIPARIS_NO";
                        public const string MinMiktar = "MIN_MIKTAR";
                        public const string MaxMiktar = "MAX_MIKTAR";
                    }
                }
                public static class PazarYeriMalTanim
                {
                    public const string Name = "PAZAR_YERI_MAL_TANIM";
                    public static class Column
                    {
                        public const string AnaMalNo = "ANA_MAL_NO";
                        public const string MalNo = "MAL_NO";
                        public const string PazarYeriMalAdi = "PAZAR_YERI_MAL_ADI";
                        public const string PazarYeriMalNo = "PAZAR_YERI_MAL_NO";
                        public const string PazarYeriNo = "PAZAR_YERI_NO";
                        public const string PyUrunSatisBirim = "PY_URUN_SATIS_BIRIM";
                        public const string PyUrunSatisDeger = "PY_URUN_SATIS_DEGER";
                        public const string SepeteEklenebilirMiktar = "SEPETE_EKLENEBILIR_MIKTAR";
                        public const string ImageUrl = "IMAGE_URL";

                    }
                }
                public static class PazarYeriKargoAdres
                {
                    public const string Name = "PAZAR_YERI_KARGO_ADRES";
                    public static class Column
                    {
                        public const string Ad = "AD";
                        public const string Adres1 = "ADRES_1";
                        public const string Adres2 = "ADRES_2";
                        public const string AdSoyad = "AD_SOYAD";
                        public const string Id = "ID";
                        public const string KargoAdresId = "KARGO_ADRES_ID";
                        public const string PostaKod = "POSTA_KOD";
                        public const string Sehir = "SEHIR";
                        public const string SehirKod = "SEHIR_KOD";
                        public const string Semt = "SEMT";
                        public const string SemtId = "SEMT_ID";
                        public const string Soyad = "SOYAD";
                        public const string TamAdres = "TAM_ADRES";
                        public const string UlkeKod = "ULKE_KOD";


                    }
                }
                public static class PazarYeriFaturaAdres
                {
                    public const string Name = "PAZAR_YERI_FATURA_ADRES";
                    public static class Column
                    {
                        public const string Adi = "ADI";
                        public const string Adres1 = "ADRES_1";
                        public const string Adres2 = "ADRES_2";
                        public const string AdSoyad = "AD_SOYAD";
                        public const string FaturaAdresId = "FATURA_ADRES_ID";
                        public const string Firma = "FIRMA";
                        public const string Id = "ID";
                        public const string PostaKod = "POSTA_KOD";
                        public const string Sehir = "SEHIR";
                        public const string Semt = "SEMT";
                        public const string Soyadi = "SOYADI";
                        public const string TamAdres = "TAM_ADRES";
                        public const string UlkeKod = "ULKE_KOD";


                    }
                }
                public static class PazarYeriBirimTanim
                {
                    public const string Name = "PAZAR_YERI_BIRIM_TANIM";
                    public static class Column
                    {
                        public const string BirimNo = "BIRIM_NO";
                        public const string PazarYeriBirimNo = "PAZAR_YERI_BIRIM_NO";
                        public const string PazarYeriNo = "PAZAR_YERI_NO";
                        public const string AktifPasif = "AKTIF_PASIF";
                    }
                }
                public static class BirimTanim
                {
                    public const string Name = "BIRIM_TANIM";
                    public static class Column
                    {
                        public const string BirimAdi = "BIRIM_ADI";
                        public const string BirimNo = "BIRIM_NO";

                    }
                }
                public static class PazarYeriAktarim
                {
                    public const string Name = "PAZAR_YERI_AKTARIM";
                    public static class Column
                    {
                        public const string BirimNo = "BIRIM_NO";
                        public const string IndirimliSatisFiyat = "INDIRIMLI_SATIS_FIYAT";
                        public const string MalNo = "MAL_NO";
                        public const string PazarYeriBirimNo = "PAZAR_YERI_BIRIM_NO";
                        public const string PazarYeriMalNo = "PAZAR_YERI_MAL_NO";
                        public const string PazarYeriNo = "PAZAR_YERI_NO";
                        public const string SatisFiyat = "SATIS_FIYAT";
                        public const string QpFiyat = "QP_FIYAT";

                    }
                }
                public static class VPazaryeriUrunler
                {
                    public const string Name = "V_PAZARYERI_URUNLER";
                    public static class Column
                    {
                        public const string BirimNo = "BIRIM_NO";
                        public const string KategoriAdi = "KATEGORI_ADI";
                        public const string KategoriKod = "KATEGORI_KOD";
                        public const string MalAltgrupAdi = "MAL_ALTGRUP_ADI";
                        public const string MalAltgrupNo = "MAL_ALTGRUP_NO";
                        public const string MalAltsinifAdi = "MAL_ALTSINIF_ADI";
                        public const string MalAltsinifNo = "MAL_ALTSINIF_NO";
                        public const string MalGrupAdi = "MAL_GRUP_ADI";
                        public const string MalGrupNo = "MAL_GRUP_NO";
                        public const string MalNo = "MAL_NO";
                        public const string MalSatisBirimKod = "MAL_SATIS_BIRIM_KOD";
                        public const string MalSatisKdvDeger = "MAL_SATIS_KDV_DEGER";
                        public const string MalSinifAdi = "MAL_SINIF_ADI";
                        public const string MalSinifNo = "MAL_SINIF_NO";
                        public const string PazarYeriMalAdi = "PAZAR_YERI_MAL_ADI";
                        public const string PazarYeriNo = "PAZAR_YERI_NO";
                        public const string SatisFiyat = "SATIS_FIYAT";
                        public const string StokMiktar = "STOK_MIKTAR";


                    }
                }
                public static class EmailHareket
                {
                    public const string Name = "EMAIL_HAREKET";
                    public static class Column
                    {
                        public const string Type = "EMAIL_TIP";
                        public const string From = "EMAIL_NEREDEN";
                        public const string To = "EMAIL_NEREYE";
                        public const string Cc = "EMAIL_CC";
                        public const string Subject = "EMAIL_KONU";
                        public const string Body = "EMAIL_BODY";
                    }
                }

                public static class PyPromosyonEntegrasyon
                {
                    public const string Name = "PY_PROMOSYON_ENTEGRASYON";
                    public const string PK = "PK_PROMOSYON_ENTEGRASYON";

                    public static class Column
                    {
                        public const string Id = "ID";
                        public const string Tarih = "TARIH";
                        public const string PromosyonNo = "PROMOSYON_NO";
                        public const string GonderildiEh = "GONDERILDI_EH";
                        public const string ServisDurum = "SERVIS_DURUM";
                        public const string TryCount = "TRY_COUNT";
                        public const string HataMesaji = "HATA_MESAJI";
                        public const string InsertDatetime = "INSERT_DATETIME";
                    }
                }
                public static class PyPromosyonTanim
                {
                    public const string Name = "PY_PROMOSYON_TANIM";
                    public const string PK = "PK_PY_PROMOSYON_TANIM";                    

                    public static class Column
                    {
                        public const string PazarYeriNo = "PAZAR_YERI_NO";
                        public const string PromosyonNo = "PROMOSYON_NO";
                        public const string PyPromosyonNo = "PY_PROMOSYON_NO";
                        public const string PromosyonTipKod = "PROMOSYON_TIP_KOD";
                        public const string Durum = "DURUM";
                        public const string BaslangicTarih = "BASLANGIC_TARIH";
                        public const string BaslangicSaat = "BASLANGIC_SAAT";
                        public const string BitisTarih = "BITIS_TARIH";
                        public const string BitisSaat = "BITIS_SAAT";
                        public const string MinSiparisMiktar = "MIN_SIPARIS_MIKTAR";
                        public const string MaxSiparisMiktar = "MAX_SIPARIS_MIKTAR";
                        public const string TumBirimlerEh = "TUM_BIRIMLER_EH";
                        public const string InsertDatetime = "INSERT_DATETIME";
                    }
                }
                public static class TmpPyPromosyonTanim
                {
                    public const string Name = "TMP_PY_PROMOSYON_TANIM";

                    public static class Column
                    {
                        public const string Id = "ID";
                        public const string PazaryeriNo = "PAZAR_YERI_NO";
                        public const string PromosyonNo = "PROMOSYON_NO";
                        public const string Reason = "REASON";
                        public const string Active = "ACTIVE";
                        public const string StartTime = "START_TIME";
                        public const string EndTime = "END_TIME";
                        public const string PurchasedQuantity = "PURCHASED_QUANTITY";
                        public const string OrderLimit = "ORDER_LIMIT";
                        public const string TumBirimlerEh = "TUM_BIRIMLER_EH";
                        public const string Type = "TYPE";
                        public const string DisplayName = "DISPLAY_NAME";
                    }
                }
                public static class TmpPyPromosyonDetay
                {
                    public const string Name = "TMP_PY_PROMOSYON_DETAY";

                    public static class Column
                    {
                        public const string Id = "ID";
                        public const string PromosyonNo = "PROMOSYON_NO";
                        public const string Sku = "SKU";
                        public const string Active = "ACTIVE";
                        public const string DiscountSubtype = "DISCOUNT_SUBTYPE";
                        public const string DiscountValue = "DISCOUNT_VALUE";
                        public const string MaxQuantity = "MAX_QUANTITY";
                    }
                }
                public static class TmpPyPromosyonBirim
                {
                    public const string Name = "TMP_PY_PROMOSYON_BIRIM";

                    public static class Column
                    {
                        public const string Id = "ID";
                        public const string PromosyonNo = "PROMOSYON_NO";
                        public const string BirimNo = "BIRIM_NO";
                    }
                }

            }
        

            public static class RawQuery
            {

                public static class IsItemCancelledQuery
                {
                    public static class Parameters
                    {
                        public const string QPProductId = "P_QPProductId";
                        public const string OrderId = "P_OrderId";
                    }
                }
                public static class ProductPhotosQuery
                {
                    public static class Parameters
                    {
                        public const string PazarYeriBirimId = "P_PAZAR_YERI_BIRIM_ID";
                        public const string ObaseMalNo = "P_OBASE_MAL_NO";
                    }
                }
            }
        }
        public static class Character
        {
            public const string E = "E";
            public const string H = "H";
            public const string D = "D";
            public const string C = "C";
            public const string P = "P";
            public const string I = "I";
        }
        public static class TyGoConstants
        {
            public const string Accepted = "ACCEPTED";
            public const string Rejected = "REJECTED";
            public const string Created = "Created";
            public const string Cancelled = "Cancelled";
            public const string UnSupplied = "UnSupplied";
            public const string Invoiced = "Invoiced";
            public const string StoreDelivery = "STORE";
            public const string GoDelivery = "GO";
            public const int RejectClaimReason = 1751;
            public static class Parameters
            {
                public const string GetClaimsDayCount = "ClaimDayCount";
                public const string GetOrdersDayCount = "DayCount";
                public const string SendToQp = "SendToQP";
                public const string NumberofThreads = "NumberofThreads";
                public const string NumberofProducts = "NumberofProducts";
                public const string NumberofTries = "NumberofTries";
                public const string NumberofSQLRows = "NumberofSQLRows";

            }
        }

        public static class SharedPriceStockOnlyConstants
        {
            public static class Parameters
            {
                public const string NumberofThreads = "NumberofThreads";
                public const string NumberofProducts = "NumberofProducts";
                public const string NumberofTries = "NumberofTries";
                public const string NumberofSQLRows = "NumberofSQLRows";
                public const string ExecutionType = "ExecutionType";
                public const string Merchantno = "Merchantno";
                public const string Request = "Request";
                public const string DeliveryDuration = "DeliveryDuration";
                public const string DeliveryType = "DeliveryType";
            }
        }

        public static class YemekSepetiConstants
        {

            public const string ShopAccepted = "SHOP_APPROVE";
            public const string ShopRejected = "SHOP_REJECT";

            public const string Created = "Created";

            public const string CreateOrderEndpoint = "api/v1/yemeksepeti/create-order";
            public static class ProductType
            {
                public const string KG = "KG";
                public const string Unit = "UNIT";
            }
            public static class ProductStatus
            {
                public const string InCart = "IN_CART";
                public const string NotFound = "NOT_FOUND";
                public const string Replaced = "REPLACED";

            }
            public static class OrderType
            {
                public const string Delivery = "DELIVERY";
            }
            public static class OrderStatus
            {
                public const string Received = "RECEIVED";
                public const string Dispatched = "DISPATCHED";
                public const string ReadyForPickup = "READY_FOR_PICKUP";
                public const string Delivered = "DELIVERED";
                public const string Cancelled = "CANCELLED";
            }
            public static class OrderCancellationReason
            {
                public const string Closed = "CLOSED";
                public const string ItemUnavailable = "ITEM_UNAVAILABLE";
                public const string ToBusy = "TOO_BUSY";
            }

            public static class OrderTransportType
            {
                public const string OD = "LOGISTICS_DELIVERY";
                public const string VD = "VENDOR_DELIVERY";
            }
            public static class Parameters
            {
                public const string NumberofThreads = "NumberofThreads";
                public const string NumberofProducts = "NumberofProducts";
                public const string NumberofTries = "NumberofTries";
                public const string NumberofSQLRows = "NumberofSQLRows";
                public const string ExecutionType = "ExecutionType";
                public const string Merchantno = "Merchantno";
                public const string Request = "Request";
            }
        }

        public static class TyConstants
        {
            public static class Parameters
            {
                public const string NumberofThreads = "NumberofThreads";
                public const string NumberofProducts = "NumberofProducts";
                public const string NumberofTries = "NumberofTries";
                public const string NumberofSQLRows = "NumberofSQLRows";
                public const string ExecutionType = "ExecutionType";
            }
        }
        public static class SaleInfoConstants
        {
            public const string SaleInfoEndpoint = "api/v1/sales/create";
        }
        public static class GetirConstants
        {
            public const string Unapproved = "UNAPPROVED";
            public const string ShopAccepted = "SHOP_APPROVE";
            public const string ShopRejected = "SHOP_REJECT";

            public const string Created = "Created";
            public const string Cancelled = "CANCELLED";
            public const string GetirCreateOrderEndpoint = "api/v1/getir/create-order";
           

            public static class ProductType
            {
                public const string Gr = "gr";
                public const string Count = "count";
            }
            public static class Parameters
            {
                public const string GetClaimsDayCount = "ClaimDayCount";
                public const string GetOrdersDayCount = "DayCount";
                public const string SendToQp = "SendToQP";
                public const string NumberofThreads = "NumberofThreads";
                public const string NumberofProducts = "NumberofProducts";
                public const string NumberofTries = "NumberofTries";
                public const string NumberofSQLRows = "NumberofSQLRows";

            }
            public static class ReturnsStatus
            {
                public const string Waiting = "waiting";
                public const string DirectlyToShop = "directly-to-shop";
            }
            public static class DeliveryType
            {
                public const string GetirGetirsin = "GG";
                public const string IsletmeGetirsin = "RG";
            }
            public static class DeliveryTypeCode
            {
                public const string GetirGetirsin = "1";
                public const string IsletmeGetirsin = "2";
            }
        }

        public static class IdefixConstants
        {
            public static class ShipmentStatuses
            {
                public const string Approved = "Approved"; // Onaylandı
                public const string Packaging = "Packaging"; // Paketleniyor
                public const string Shipped = "Shipped"; // Kargoya Verildi
                public const string Delivered = "Delivered"; // Teslim Edildi
                public const string Cancelled = "Cancelled"; // İptal Edildi
                public const string Returned = "Returned"; // İade Edildi
            }

            public class OrderState
            {
                public const string Created = "created";
                public const string ShipmentReady = "shipment_ready";
                public const string ShipmentPicking = "picking";
                public const string ShipmentInvoiced = "invoiced";
                public const string ShipmentCancelled = "shipment_cancelled";
                public const string ShipmentUnsupplied = "shipment_unsupplied";
                public const string ShipmentSplit = "shipment_split";
                public const string ShipmentInCargo = "shipment_in_cargo";
                public const string ShipmentDelivered = "shipment_delivered";
                public const string ShipmentUndeliver = "shipment_undeliver";
                public const string ShipmentApproved = "shipment_approved";

            }


            public static class ProductType
            {
                public const string Gr = "gr";
                public const string Count = "count";
            }
            public static class Parameters
            {
                public const string GetClaimsDayCount = "ClaimDayCount";
                public const string GetOrdersDayCount = "DayCount";
                public const string SendToQp = "SendToQP";
                public const string NumberofThreads = "NumberofThreads";
                public const string NumberofProducts = "NumberofProducts";
                public const string NumberofTries = "NumberofTries";
                public const string NumberofSQLRows = "NumberofSQLRows";
                public const string DeliveryDuration = "DeliveryDuration";
                public const string DeliveryType = "DeliveryType";

            }
            public static class ReturnsStatus
            {
                public const string Waiting = "waiting";
                public const string DirectlyToShop = "directly-to-shop";
            }

            public static class OrderTransportType
            {
                public const string DLVRY = "delivery";
                public const string OD = "LOGISTICS_DELIVERY";
                public const string VD = "VENDOR_DELIVERY";
            }
        }
        public static class CommonConstants
        {

            public const string GeneralLogFile = "GeneralLogs";
            public const string SharedPriceStockOnlyStockJob = "SharedPriceStockOnlyStockJob";
            public const string CheckingPeriod = "CheckingPeriod";
            public const string ControlHour = "ControlHour";
            public const string PazarYerleri = "PazarYerleri";
            public const string ControlMinute = "ControlMinute";
            public const string QuickPickLogFile = "QuickPick";
            public const string KG = "KG";
            public const string ADET = "ADET";
            public const string Aktif = "A";
            public const string Pasif = "P";
            public const string XApiKey = "x-api-key";
            public const string Authorization = "Authorization";
            public static string[] LogFiles =
            {
                "SharedPriceStockOnlyStockJob",
                 "QuickPick",
                 "SaleInfo",
                 nameof(CommonEnums.PazarYerleri.HepsiExpress),
                 nameof(CommonEnums.PazarYerleri.TrendyolGo),
                 nameof(CommonEnums.PazarYerleri.Trendyol),
                 nameof(CommonEnums.PazarYerleri.GetirCarsi),
                 nameof(CommonEnums.PazarYerleri.YemekSepeti),
                 nameof(CommonEnums.PazarYerleri.Pazarama),
                 nameof(CommonEnums.PazarYerleri.Idefix),
                 nameof(CommonEnums.PazarYerleri.AkilliETicaret),
                 nameof(CommonEnums.PazarYerleriHttpClient.IdefixHttpClient),
                 nameof(CommonEnums.PazarYerleriHttpClient.YemekSepetiHttpClient),
                 nameof(CommonEnums.PazarYerleriPromosyon.YemekSepetiPromosyon),
                 nameof(CommonEnums.PazarYerleriHttpClient.GetirCarsiHttpClient),

        };
        }
        public class PazarYeriPaymentId
        {
            public const int HepsiExpress = 33;
            public const int TrendyolGO = 32;
            public const int Getir = 34;
            public const int YemekSepeti = 35;
            public const int Default = 32;
            public const int Idefix = 36;
            public const int AkilliETicaret = 38;
        }
        public class SaleChannelId
        {
            public const int TrendyolGO = 3;
            public const int HepsiExpress = 4;
            public const int Getir = 5;
            public const int YemekSepeti = 6;
            public const int Idefix = 7;
            public const int AkilliETicaret = 8;
            public const int Default = 32;
        }
        public static class PazarYeri
        {
            public const string HepsiExpress = "01";
            public const string TrendyolGo = "02";
            public const string Trendyol = "03";
            public const string GetirCarsi = "04";
            public const string Yemeksepeti = "05";
            public const string Pazarama = "06";
            public const string Idefix = "07";
            public const string AkilliETicaret = "08";
        }
        public static class QpDeliveryModels
        {
            public const string MarketPlace = "MARKETPLACE";
            public const string Store = "STORE";
        }
        public class HepsiExpressOrderStatus
        {
            public const string Open = "Open";
            public const string Cancelled = "Cancel";
        }

        public class Currency
        {
            public const string Try = "TRY";

        }
        public class AutType
        {
            public const string Bearer = "Bearer";
            public const string Basic = "Basic";

        }
    }
}