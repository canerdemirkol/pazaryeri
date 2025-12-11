using OBase.Pazaryeri.Domain.Enums;
using System.Net;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Domain.Helper
{
	public static class CommonHelper
	{
		public static int TranslateQPReasonIdToTrendyolReasonId(string qpReasonId)
		{
			return qpReasonId switch
			{
				"621" => (int)CommonEnums.UnSuppliedStatuEnums.TedarikProblemi,
				_ => (int)CommonEnums.UnSuppliedStatuEnums.TedarikProblemi
			};
		}
		public static string ResizeTyGoImageUrl(this string[] imageUrlsArray, string width, string lenght, string urlSeperator, string resizePathParameter)
		{
			StringBuilder imageUrlSB = new("");
			foreach (string url in imageUrlsArray)
			{
				var urlArray = url.Split(urlSeperator);
				if (urlArray.Length == 2)
				{
					string baseUrl = urlArray[0] + urlSeperator;
					imageUrlSB.Append($"{baseUrl + resizePathParameter + width + lenght + urlArray[1]},");
				}
				else
				{
					imageUrlSB.Append($"{url},");
				}
			}
			return imageUrlSB.ToString();
		}
		public static string ResizeTyGoImageUrl(this string imageUrl, string width, string lenght, string urlSeperator, string resizePathParameter)
		{
			StringBuilder imageUrlSB = new StringBuilder("");
			var urlArray = imageUrl.Split(urlSeperator);
			if (urlArray.Length == 2)
			{
				string baseUrl = urlArray[0] + urlSeperator;
				imageUrlSB.Append($"{baseUrl + resizePathParameter + width + lenght + urlArray[1]}");
			}
			else
			{
				imageUrlSB.Append($"{imageUrl}");
			}

			return imageUrlSB.ToString();
		}
		public static DateTime UnixTimeStampToDateTime(long dateTime0, string merchantNo = "")
		{
			long dateTime = dateTime0;
			if (dateTime <= 1000000000000L) dateTime *= 1000;

			return merchantNo switch
			{
				PazarYeri.HepsiExpress => new DateTime(dateTime).ToLocalTime(),
                PazarYeri.TrendyolGo=> (new DateTime(1970, 1, 1)).AddMilliseconds(Convert.ToDouble(dateTime)).ToLocalTime(),
				_=> new DateTime(dateTime).ToLocalTime()
			};
		}
		public static int StringToInt32(string strPhrase, int defValue = 0)
		{
			if (!int.TryParse(strPhrase, out int intValue)) intValue = defValue;
			return intValue;
		}

		public static long StringToLong(string strPhrase, long defValue = 0)
		{
			if (!long.TryParse(strPhrase, out long longValue)) longValue = defValue;
			return longValue;
		}
		public static bool CalcSuccessFromHTTPStatus(HttpStatusCode httpStatusCode)
		{
			return httpStatusCode switch
			{
				HttpStatusCode.OK => true,
				HttpStatusCode.Accepted => true,
				HttpStatusCode.NoContent => true,
				HttpStatusCode.Created => true,
				HttpStatusCode.Conflict => false,
				HttpStatusCode.NotFound => false,
				HttpStatusCode.BadRequest => false,
				_ => false
			};
		}
		public static string GetMerchantNo(this CommonEnums.PazarYerleri pazarYeri)
		{
			return Enum.GetName(typeof(PazarYerleri), pazarYeri) switch
			{
				nameof(PazarYeri.GetirCarsi) => PazarYeri.GetirCarsi,
				nameof(PazarYeri.Pazarama) => PazarYeri.Pazarama,
				nameof(PazarYeri.HepsiExpress) => PazarYeri.HepsiExpress,
				nameof(PazarYeri.Yemeksepeti) => PazarYeri.Yemeksepeti,
				nameof(PazarYeri.Trendyol) => PazarYeri.Trendyol,
				nameof(PazarYeri.TrendyolGo) => PazarYeri.TrendyolGo,
				_ => string.Empty
			};
		}
	}
}