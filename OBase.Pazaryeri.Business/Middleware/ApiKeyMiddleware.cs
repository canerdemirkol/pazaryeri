using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Middleware
{
	public class ApiKeyMiddleware : ControllerBase
	{
		private readonly RequestDelegate _next;
		private readonly IOptions<AppSettings> _options;
		public ApiKeyMiddleware(RequestDelegate next, IOptions<AppSettings> options)
		{
			_next = next;
			_options = options;
		}
		public async Task InvokeAsync(HttpContext context)
		{
            if (context.Request.Path.ToString().ToLower().Contains("serverjobs"))
            {
                await _next(context);
            }

            if (context.Request.Path.Value.Contains(GetirConstants.GetirCreateOrderEndpoint))
			{
				if (!context.Request.Headers.TryGetValue(CommonConstants.XApiKey, out
						var extractedApiKey))
				{
					context.Response.StatusCode = 401;
					context.Response.ContentType = ContentType.Json;
					var result =  ServiceResponse<CommonResponseDto>.Error($"{CommonConstants.XApiKey} missing.", HttpStatusCode.Unauthorized);
					await context.Response.WriteAsync(JsonSerializer.Serialize(result));
					return;
				}
				var apiKey = _options.Value.ApiUser.ApiKey;
				if (!apiKey.Equals(extractedApiKey))
				{
					context.Response.StatusCode = 401;
					context.Response.ContentType = ContentType.Json;
					var result = ServiceResponse<CommonResponseDto>.Error($"{CommonConstants.XApiKey} is incorrect.", HttpStatusCode.Unauthorized);
					await context.Response.WriteAsync(JsonSerializer.Serialize(result));
					return;
				}
			}
			await _next(context);
		}
	}
}
