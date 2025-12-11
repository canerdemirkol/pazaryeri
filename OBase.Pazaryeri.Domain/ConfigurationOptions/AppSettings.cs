using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Core.Utility;

namespace OBase.Pazaryeri.Domain.ConfigurationOptions
{
	public class AppSettings
	{
		public bool EnableMarketPlaceServices { get; set; }
		public string WareHouseUrl { get; set; }
		public string CargoProductCode { get; set; }
		public ConnectionString ConnectionStrings { get; set; }
		public ApiDefinitions[] ApiDefinitions { get; set; }
		public SachetProduct[] SachetProducts { get; set; }
		public ImageSize ImageSize { get; set; }
		public ScheduledTask[] ScheduledTasks { get; set; }
		public LogDefinitions LogDefinitions { get; set; }
		public ApiUser ApiUser { get; set; }
		public MailSettings MailSettings { get; set; }
		public AuthDefinitions AuthDefinitions { get; set; }

        public string[] QpOrderErrorMessages { get; set; }

        public RawQueries RawDatabaseQueries { get; set; }

        public ValidateOptionsResult Validate()
		{
			return ValidateOptionsResult.Success;
		}
	}

	public class AppSettingsValidation : IValidateOptions<AppSettings>
	{
		public ValidateOptionsResult Validate(string name, AppSettings options)
		{
			return options.Validate();
		}
	}

	public class ConnectionString
	{
		public string DefaultConnection { get; set; }
	}
	public class MailSettings
	{
		public bool MailEnabled { get; set; } = false;
        public string From { get; set; }
		public string To { get; set; }
		public string CC { get; set; }
	}
	public class ApiDefinitions
	{
        public ApiUser ApiUser { get; set; }
		public string SupplierId { get; set; }
		public string Merchantno { get; set; }
		public string AuthenticationType { get; set; }
		public string Domain { get; set; }
        public string PIMDomain { get; set; }
        public string OSMDomain { get; set; }
        public string MerchantId { get; set; }
		public string XAgentName { get; set; }
		public string XExecutorUser { get; set; }
		public bool OrderSendToQp { get; set; }
		public SachetProduct[] SachetProducts { get; set; }
		public bool? UsingApinizer { get; set; }
        public string ChainId { get; set; }

    }

	public class LogDefinitions
	{
		public string FileLog { get; set; }
		public string LogThreshhold { get; set; }
		public string LogMaxSize { get; set; }
		public string FileLogName { get; set; }
		public string FileLogDayRetention { get; set; }
		public string LogOutputTemplate { get; set; }
		public string MinLogLevel { get; set; }
	}
	public class SachetProduct
	{
		public string ProductCode { get; set; }
		public decimal Price { get; set; }
	}
	public class ImageSize
	{
		public string Width { get; set; }
		public string Length { get; set; }
		public string UrlSeperator { get; set; }
		public string ResizePathParameter { get; set; }

	}

	public class Properties
	{
		public string Key { get; set; }
		public int? DayCount { get; set; }
		public bool? SendToQP { get; set; }
	}
	public class ScheduledTask
	{
		public string JobId { get; set; }
		public bool Status { get; set; }
		public string CronExpression { get; set; }
		public Properties Properties { get; set; }
	}

	#region UserCredentials
	public class ApiUserSettings
	{
		public bool AuthEnabled { get; set; }
		public string AuthType { get; set; }
		public ApiUser ApiUser { get; set; }
	}
	public class ApiUser
	{
		private string _username;
		private string _password;
		private string _apiKey;
        private string _secretKey;
        public string Token { get; set; }
		public string ObaseApiToken { get; set; }
        public string Username
		{
			get { return _username; }
			set
			{
				if (value is null)
				{
					throw new Exception("CONFIG DOSYASINDA Api Username BULUNAMADI Key[Username].");
				}
				_username = Cipher.DecryptString(value);
			}
		}
		public string Password
		{
			get { return _password; }
			set
			{
				if (value is null)
				{
					throw new Exception("CONFIG DOSYASINDA Api Password BULUNAMADI Key[Password].");
				}
				_password = Cipher.DecryptString(value);
			}
		}
		public string ApiKey
		{
			get { return _apiKey; }
			set
			{
				if (value is not null)
				{
					_apiKey = Cipher.DecryptString(value);
				}
				else
				{
					_apiKey = "";
				}
			}
		}
        public string SecretKey
        {
            get { return _secretKey; }
            set
            {
                if (value is not null)
                {
                    _secretKey = Cipher.DecryptString(value);
                }
                else
                {
                    _secretKey = "";
                }
            }
        }
    }
    #endregion

    public class AuthDefinitions
    {
        public bool AuthEnabled { get; set; }
        public string AuthType { get; set; }
        private string _jwtSecret;
        public string JwtSecret
        {
            get { return _jwtSecret; }
            set
            {
                if (value is not null)
                {
					_jwtSecret = Cipher.DecryptString(value);
                }
                else
                {
                    _jwtSecret = "";
                }
            }
        }
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
		public int? JwtExpiresInDay { get; set; }
        public JwtUser[] JwtUsers { get; set; }
    }

    public class JwtUser
    {
        private string _username;
        private string _password;

        public string Username
        {
            get { return _username; }
            set
            {
                if (value is not null)
                {
                    _username = Cipher.DecryptString(value);
                }
                else
                {
                    _username = "";
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value is not null)
                {
                    _password = Cipher.DecryptString(value);
                }
                else
                {
                    _password = "";
                }
            }
        }

        public string[] Roles { get; set; }

        /// <summary>
        /// Bu kullanıcı için Tedarikçi/Müşteri tanımlayıcısı
        /// </summary>
        public string SupplierId { get; set; }

        /// <summary>
        /// Bu kullanıcının erişmesine izin verilen end-point listesi (örneğin, "SaleInfo", "YemekSepeti", "GetirCarsi")
        /// </summary>
        public string[] AllowedServices { get; set; }

        public int? JwtExpiresInDay { get; set; }
    }
    public class RawQueries
    {
        public string PazarYeriAktarimWithLatestPrice { get; set; }
        public string PazarYeriAktarimInBarcodesWithLatestPrice { get; set; }
        public string MalSatisFiyatSelect { get; set; }
        public string MalSatisFiyatSelectWithMalNos { get; set; }
        public string SeqQuery { get; set; }
        public string SaleInfoSeqQuery { get; set; }
        public string DecodedPazarYeriNoQuery { get; set; }
        public string GunSonuRaporQuery { get; set; }
        public string IsItemCancelledQuery { get; set; }
        public string IsTheOrderToBeCancelled { get; set; }
        public string ProductPhotosQuery { get; set; }
        public string PPazarYeriLogGuncelleQuery { get; set; }
		public string EmailHareketInsertQuery { get; set; }
        public string UpdatePazarYeriSiparisStatusQuery { get; set; }
        public string ProcPYPromotionEntegrasyon { get; set; }
        public string PazarYeriPromotionEntegrasyonListQuery { get; set; }
        public string PazarYeriPromotionEntegrasyonTmpPromosyonTanimQuery { get; set; }
        public string PazarYeriPromotionEntegrasyonTmpPromosyonTanimDetayQuery { get; set; }
        public string PazarYeriPromotionEntegrasyonTmpPromosyonBirimQuery { get; set; }
        public string PazarYeriPromotionEntegrasyonUpdatePromotionStatusQuery { get; set; }
        public string CashReceiptInsertQuery { get; set; }
        public string CashReceiptDetailInsertQuery { get; set; }
        public string CashReceiptPaymentDetailInsertQuery { get; set; }
        public string CashReceiptDiscountDetailInsertQuery { get; set; }


    }
}
