using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.Idefix;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using Polly;
using Polly.Retry;
using RestEase;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Handlers
{
    public class YemekSepetiHttpClientHandler : DelegatingHandler
    {
        private readonly IMemoryCache _cache;
        private readonly ApiDefinitions _apiDefinition;

        private readonly string _logFolderName = nameof(PazarYerleriHttpClient.YemekSepetiHttpClient);

        /// <summary>
        ///
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="_appSettings"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public YemekSepetiHttpClientHandler(IMemoryCache cache, IOptions<AppSettings> _appSettings)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _apiDefinition = _appSettings?.Value?.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Yemeksepeti);

        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            string requestBody = request.Content != null
                ? await request.Content.ReadAsStringAsync()
                : string.Empty;

            string responseBody = string.Empty;
            string apiKeyHeaderToken = string.Empty;
            string reqGuid = Guid.NewGuid().ToString();

            if (request.Headers.TryGetValues("Authorization", out var values))
            {
                apiKeyHeaderToken = values.FirstOrDefault() ?? string.Empty;
            }
            Logger.Debug(
                "Yemeksepeti HttpClient [{guid}] Request => Url:{Url} | Method:{Method} | RequestBody:{RequestBody}",
                fileName: _logFolderName,
                reqGuid,
                request.RequestUri,
                request.Method,
                requestBody
            );

            var response = await base.SendAsync(request, cancellationToken);

            try { responseBody = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty; } catch { }

            Logger.Debug(
                "Yemeksepeti HttpClient [{guid}] Response => StatusCode:{statusCode} | Url:{Url} | Method:{Method} | ResponseBody:{ResponseBody}",
                fileName: _logFolderName,
                reqGuid,
                response.StatusCode.ToString(),
                request.RequestUri,
                request.Method,
                responseBody
            );

            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                Logger.Error(
                        "Yemeksepeti HttpClient [{guid}] Error => StatusCode:{statusCode} | Url:{Url} | Method:{Method} | Token:{Token} | RequestBody:{requestBody} | ResponseBody:{responseBody}",
                        fileName: _logFolderName,
                        reqGuid,
                        response.StatusCode.ToString(),
                        request.RequestUri,
                        request.Method,
                        apiKeyHeaderToken,
                        requestBody,
                        responseBody
                    );
            }
            return response;
        }


    }


}
