using Microsoft.AspNetCore.Http;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Utility;
using OBase.Pazaryeri.Domain.Dtos;
using Serilog;
using System.Diagnostics;
using System.Net;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Middleware
{
	public class ExceptionAndLogMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly string logFile = CommonConstants.GeneralLogFile;

		public ExceptionAndLogMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				if (context.Request.Path.ToString().ToLower().Contains("serverjobs"))
				{
					await _next(context);
				}
				else
				{
					Stopwatch stopWatch = new();
					var request = context.Request;
					string requestHeader = string.Empty;
					string requestBodyContent = string.Empty;
					string queryString = string.Empty;
					string method = string.Empty;
					string path = string.Empty;

					requestBodyContent = await request.ReadRequestBodyAsync();
					requestHeader = request.Headers.FormatHeaders();
					method = request.Method;
					path = request.Path;
					queryString = request.QueryString.ToString();

					stopWatch.Start();
					await _next(context);
					stopWatch.Stop();


                    Log.ForContext("HttpRequest", true, true)
						.Information($"HTTP request information:\n" +
					$"\tElapsedTime: {stopWatch.Elapsed.TotalSeconds}\n" +
					$"\tMethod: {method}\n" +
					$"\tPath: {path}\n" +
					$"\tRequestHeader: {requestHeader}\n" +
					$"\tQueryString: {queryString}\n" +
					$"\tRequestBody: {requestBodyContent}\n");

					if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
                    {
						var result = ServiceResponse<CommonResponseDto>.Error("Kullanici adi ve/veya sifre bilgileri hatali.", HttpStatusCode.Unauthorized);
						await context.UnauthorizedStatusCodeAddContent(result, HttpStatusCode.Unauthorized);
					}
				}
			}
			catch (Exception ex)
			{
				HttpStatusCode statusCode;
				if (ex.Message.Contains("timeout"))
				{
					statusCode = HttpStatusCode.InternalServerError;
				}
				else
				{
					statusCode = HttpStatusCode.BadRequest;
				}
				var exMessage = ex.GetInnerExceptionMessage();
				Logger.Error("Error while processing request {exception}",logFile, ex);
                Log.ForContext("HttpRequest", true, true).Error("Error while processing request {exception}", ex);
				var result = ServiceResponse<CommonResponseDto>.Error(exMessage, statusCode);
				await context.HandleExceptionAsync(result, statusCode, ex);
			}
		}
	}
}
