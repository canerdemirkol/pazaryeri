using Microsoft.AspNetCore.Http;
using RestSharp;
using Serilog;
using System.Net;
using System.Text;
using System.Text.Json;

namespace OBase.Pazaryeri.Business.Utility
{
	public static class ExceptionHelper
	{
		public static string GetInnerExceptionMessage(this Exception ex)
		{
			if (ex.InnerException == null)
				return ex.Message;

			return ex.Message + " r\nInnerException:" + GetInnerExceptionMessage(ex.InnerException);
		}

		public static string GetErrorMessage(this Exception ex)
		{
			return ex == null ? "" : string.Format("{0}->{1}", ex.Message, GetErrorMessage(ex.InnerException));
		}

		public static async Task HandleExceptionAsync(this HttpContext context, object result, HttpStatusCode httpStatusCode, Exception e)
		{
			if (e != null)
			{
				Log.Error(e.Message, e);
			}

            // Response başlamışsa header'ları değiştiremeyiz
            if (context.Response.HasStarted)
                return;

            context.Response.ContentType = ContentType.Json;
			context.Response.StatusCode = (int)httpStatusCode;
			await context.Response.WriteAsync(JsonSerializer.Serialize(result));
		}

		public static string FormatHeaders(this IHeaderDictionary headers)
		{
			return string.Join(", ", headers.Where(x => !x.Key.Equals("Authorization")).Select(kvp => $"{{{kvp.Key}: {string.Join(", ", kvp.Value.ToString())}}}"));
		}

		public static async Task<string> ReadRequestBodyAsync(this HttpRequest request)
		{
			try
			{
				request.EnableBuffering();
				var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
				var bodyAsText = Encoding.UTF8.GetString(buffer);
				request.Body.Seek(0, SeekOrigin.Begin);
				return bodyAsText;

			}
			catch (Exception ex)
			{
				return ex.Message + ex.InnerException?.Message;
			}
		}
		public static void ThrowExMessages<T>(this Exception ex, string entity, T obj = null) where T : class
		{
			string errMSG = "DB HATASI:";
			errMSG += string.Format("Tablo:\"{0}\" Message:\"{1}\"", entity, ex.ToString());
			if (ex.InnerException != null)
			{
				errMSG += string.Format("\r\nInner Exception: {0}", ex.InnerException.Message);
			}
			if (obj != null)
			{
				errMSG += string.Format("\r\nFailed Data: {0}", JsonSerializer.Serialize(obj));
			}
			Exception exx = new Exception(ex.Message);
			exx.Data.Add("CustomMessage", errMSG);
			throw exx;
		}
		public static void ThrowExMessages(this Exception ex, string entity)
		{
			ex.ThrowExMessages<string>(entity);
		}
	}
}
