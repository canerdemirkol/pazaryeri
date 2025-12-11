using System.Text.Json.Serialization;

namespace OBase.Pazaryeri.Domain.Enums
{
    public class CommonEnums
    {
		public enum PazarYerleri
		{
			HepsiExpress,
			TrendyolGo,
			Trendyol,
			GetirCarsi,
			YemekSepeti,
			Pazarama,
            Idefix,
            AkilliETicaret
        }
        public enum PazarYerleriHttpClient
        {
            YemekSepetiHttpClient,
            IdefixHttpClient,
            GetirCarsiHttpClient,
            AkilliETicaretHttpClient
        }
        public enum PazarYerleriPromosyon
        {
            YemekSepetiPromosyon,
            
        }
        public enum JobType
        {
            None = -1,
            CleanJob = 0,
            All = 1,
            OnlyStock = 2,
            VerifyStock = 3,
            VerifyPriceAndStock = 4,
            GetOrder = 5,
            UpdateOrder = 6,
            UpdateBoxInfo = 7,
            Discount = 8,
            CompleteTGOrder = 9,
        }

        public enum MerchantType
        {
            None = -1,
            HepsiExpress = 10,
            TrendyolGO = 20,
            Trendyol = 30,
            Getir = 40,
            YemekSepeti = 50,
            Pazarama = 60,
            Idefix = 70
        }

        public enum LogType
        {
            AllCompleted = 0,
            PartiallyCompleted = 1,
            AllFailed = 2
        }

        public enum APIVerifyStatus
        {
            None = -1,
            Processing = 0,
            Success = 1,
            Failed = 2
        }

        public enum StatusEnums
        {
            Created = 0,
            Picking = 1,
            Completed = 2,
            Cancelled = 3,
            Collected = 4,
            CancelledPart = 5,
            Invoiced = 6,
            UnSupplied = 8,
            UnPack = 9,
            UnDelivered = 10,
            Verified = 11,
            InTransit = 12,
            Delivered = 13,
            Prepared = 14
        }

        public enum UnSuppliedStatuEnums
        {
            // TY Go
            TedarikProblemi = 621,
            MagazaKapali = 622,
            MagazaSiparisHazirlayamiyor = 623,
            YuksekYogunluk = 624,
            KullaniciKaynakliAdresHatali = 626,
            KullaniciKaynakliAdresteDegil = 627,

            // HepsiExpress
            MusteriVazgecti = 26,
            SatisaUygunDegil = 82,
            UrunStoktaYok = 83,
            HizmetDisiBolge = 42,
            MusteriyeUlasamadi = 43,

            // İdefix
            StokTukendi=500,
            KusurluDefoluBozukUrun=501,
            HataliFiyat=502,
            EntegrasyonHatasi=504,
            TopluAlim=505,
            MucbirSebeb=506,
            TedarikEdilemediVeyaStoktaYok=507
        }

        public enum GetirReturnProductStatus
        {
            IN_PROGRESS = 50,
            SHOP_APPROVE = 100,
            SHOP_REJECT = 200,
            GETIR_APPROVE = 300,
            GETIR_REJECT = 400
        }

        public enum GetirRejectReasonIDs
        {
            NOT_BELONG_TO_STORE = 1,
            DIFFERENT_PRODUCT,
            DAMAGED_PRODUCT,
            MISSING_PART,
            NOT_WRONG,
            NOT_DEFECTIVE,
            NOT_RECEIVED_RETURN,
            USED_PRODUCT
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum YemekSepetiJobType
        {
            ASYNC, 
            SYNC
        }
    }
}
