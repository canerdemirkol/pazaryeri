using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.Getir;
using OBase.Pazaryeri.Domain.Dtos.Pazarama;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using RestEase.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Handlers
{
    public class GetirCarsiHttpClientHandler : DelegatingHandler
    {
        private readonly IMemoryCache _cache;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string _logFolderName = nameof(PazarYerleriHttpClient.GetirCarsiHttpClient);


        public GetirCarsiHttpClientHandler(IMemoryCache cache, IOptions<AppSettings> _appSettings)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _apiDefinition = _appSettings?.Value?.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.GetirCarsi);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await EnsureValidTokenAsync(request);

            string requestBody = request.Content != null
                 ? await request.Content.ReadAsStringAsync()
                 : string.Empty;

            string apiKeyHeaderToken = string.Empty;
            string responseBody = string.Empty;
            string reqGuid = Guid.NewGuid().ToString();

            if (request.Headers.TryGetValues("Authorization", out var values))
            {
                apiKeyHeaderToken = values.FirstOrDefault() ?? string.Empty;
            }

            Logger.Debug(
               "GetirCarsi HttpClient [{guid}] Request => Url:{Url} | Method:{Method} | RequestBody:{RequestBody}",
               fileName: _logFolderName,
               reqGuid,
               request.RequestUri,
               request.Method,
               requestBody
            );


            var response = await base.SendAsync(request, cancellationToken);

            try { responseBody = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty; } catch { }

            if (response.IsSuccessStatusCode)
            {
                return response;
            }



            if (response.StatusCode == HttpStatusCode.Unauthorized) // Token Expired
            {

                Logger.Warning(
                    "GetirCarsi HttpClient [{guid}] Token expired, refreshing... | StatusCode:{statusCode}",
                    fileName: _logFolderName,
                    reqGuid,
                    response.StatusCode.ToString()
                );

                var newToken = await RefreshTokenAsync();
                request.Headers.Remove("Authorization");
                request.Headers.Add("Authorization", $"Bearer {newToken}");

                response = await base.SendAsync(request, cancellationToken);

                try { responseBody = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty; } catch { }

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                else
                {
                    Logger.Error(
                        "GetirCarsi HttpClient [{guid}] Retry Failed => StatusCode:{statusCode} | Url:{Url} | RequestBody:{requestBody} | ResponseBody:{responseBody}",
                        fileName: _logFolderName,
                        reqGuid,
                        response.StatusCode.ToString(),
                        request.RequestUri,
                        requestBody,
                        responseBody
                    );
                }
            }
            else
            {

                Logger.Error(
                           "GetirCarsi HttpClient [{guid}] Error => StatusCode:{statusCode} | Url:{Url} | Method:{Method} | Token:{Token} | RequestBody:{requestBody} | ResponseBody:{responseBody}",
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



        private async Task EnsureValidTokenAsync(HttpRequestMessage request)
        {
            if (!_cache.TryGetValue("GetirCarsiToken", out string bearerToken) || string.IsNullOrEmpty(bearerToken))
            {
                bearerToken = await RefreshTokenAsync();
            }

            if (!string.IsNullOrEmpty(bearerToken))
            {
                request.Headers.Remove("Authorization");
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");
            }
        }

        private async Task<string> RefreshTokenAsync()
        {
            if (_apiDefinition == null || _apiDefinition.ApiUser == null)
            {
                throw new InvalidOperationException("ApiDefinition veya ApiUser bilgileri eksik.");
            }
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            string baseUrl = _apiDefinition.Domain;
            var restClient = new RestEase.RestClient(baseUrl)
            {
                JsonSerializerSettings = jsonSerializerSettings
            };
            var getirCarsiClient = restClient.For<IGetirCarsiClient>();

            string accept = "application/json";
            string username = _apiDefinition.ApiUser.Username;
            string password = _apiDefinition.ApiUser.Password;

            string encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
            string authorization = $"Basic {encodedAuth}";


            var tokenModel = await getirCarsiClient.GetToken(authorization, accept);


            if (tokenModel.ResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var model = JsonConvert.DeserializeObject<AuthTokenResponseDto>(tokenModel.StringContent);
                var bearerToken = model?.data?.token;

                if (!string.IsNullOrEmpty(bearerToken))
                {
                    _cache.Set("GetirCarsiToken", bearerToken, TimeSpan.FromMinutes(59));
                    return bearerToken;
                }
            }

            throw new Exception("Token alınamadı, API'den geçerli bir yanıt gelmedi.");
        }


    }
}
