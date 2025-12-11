using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.AkilliETicaret;
using OBase.Pazaryeri.Domain.Dtos.Pazarama;
using Polly;
using RestEase;
using RestEase.Implementation;
using System;
using System.Net;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Handlers
{
    public class AkilliETicaretHttpClientHandler : DelegatingHandler
    {
        private readonly IMemoryCache _cache;
        private readonly ApiDefinitions _apiDefinition;
        private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
        private readonly string _logFolderName = nameof(PazarYerleriHttpClient.AkilliETicaretHttpClient);

        public AkilliETicaretHttpClientHandler(IMemoryCache cache, IOptions<AppSettings> _appSettings)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _apiDefinition = _appSettings?.Value?.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.AkilliETicaret);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await EnsureValidTokenAsync(request);

            string requestBody = request.Content != null
                 ? await request.Content.ReadAsStringAsync()
                 : string.Empty;

            string responseBody = string.Empty;
            string apiKeyHeaderToken = string.Empty;
            string apiKeyHeaderGuid = string.Empty;
            string reqGuid = Guid.NewGuid().ToString();

            if (request.Headers.TryGetValues("Authorization", out var authValues))
            {
                apiKeyHeaderToken = authValues.FirstOrDefault() ?? string.Empty;
            }

            if (request.Headers.TryGetValues("GUID", out var guidValues))
            {
                apiKeyHeaderGuid = guidValues.FirstOrDefault() ?? string.Empty;
            }

            // İlk istek
            var response = await base.SendAsync(request, cancellationToken);

            try { responseBody = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty; } catch { }

            // Eğer 401 Unauthorized gelirse token yenileme dene
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Önce refresh token ile dene
                bool refreshSuccess = await TryRefreshTokenAsync(request);

                if (!refreshSuccess)
                {
                    // Refresh başarısız olursa yeni token al
                    await ForceGetNewTokenAsync(request);
                }

                // Token yenilendi, isteği tekrar gönder
                response = await base.SendAsync(request, cancellationToken);

                try { responseBody = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty; } catch { }
            }

            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                Logger.Error(
                    "AkilliETicaret HttpClient [{guid}] Error => StatusCode:{statusCode} | Url:{Url} | Method:{Method} |Header GUID :{apiKeyHeaderGuid}| Token:{Token} | RequestBody:{requestBody} | ResponseBody:{responseBody}",
                    fileName: _logFolderName,
                    reqGuid,
                    response.StatusCode.ToString(),
                    request.RequestUri,
                    request.Method,
                    apiKeyHeaderGuid,
                    apiKeyHeaderToken,
                    requestBody,
                    responseBody
                );
            }

            return response;
        }

        private async Task EnsureValidTokenAsync(HttpRequestMessage request)
        {
            if (!_cache.TryGetValue("AkilliETicaretToken", out string bearerToken) || string.IsNullOrEmpty(bearerToken))
            {
                bearerToken = await GetTokenAsync();
            }

            if (!string.IsNullOrEmpty(bearerToken))
            {
                request.Headers.Remove("Authorization");
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");
            }
        }

        private async Task<bool> TryRefreshTokenAsync(HttpRequestMessage request)
        {
            try
            {
                var newToken = await RefreshTokenAsync();

                if (!string.IsNullOrEmpty(newToken))
                {
                    request.Headers.Remove("Authorization");
                    request.Headers.Add("Authorization", $"Bearer {newToken}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    "AkilliETicaret HttpClient [TryRefreshTokenAsync] Error => {error}",
                    fileName: _logFolderName,
                    ex.Message
                );
                return false;
            }
        }

        private async Task ForceGetNewTokenAsync(HttpRequestMessage request)
        {
            // Cache'i temizle
            _cache.Remove("AkilliETicaretToken");
            _cache.Remove("AkilliETicaretRefreshToken");

            // Yeni token al
            var newToken = await GetTokenAsync();

            if (!string.IsNullOrEmpty(newToken))
            {
                request.Headers.Remove("Authorization");
                request.Headers.Add("Authorization", $"Bearer {newToken}");
            }
        }

        private async Task<string> GetTokenAsync()
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

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            httpClient.DefaultRequestHeaders.Add("GUID", _apiDefinition.ApiUser.SecretKey);

            var restClient = new RestEase.RestClient(httpClient)
            {
                JsonSerializerSettings = jsonSerializerSettings
            };

            var akilliETicaretClient = restClient.For<IAkilliETicaretClient>();
            string username = _apiDefinition.ApiUser.Username;
            string password = _apiDefinition.ApiUser.Password;

            var requestDto = new Domain.Dtos.AkilliETicaret.AkilliETicaretLoginRequestDto()
            {
                Password = password,
                UserName = username
            };

            var tokenModel = await akilliETicaretClient.LoginAsync(requestDto);

            if (tokenModel.ResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var model = JsonConvert.DeserializeObject<AkilliETicaretResponse<AkilliETicaretLoginResponseDto>>(tokenModel.StringContent);

                if (!model.Status)
                {
                    Logger.Error(
                        "AkilliETicaret HttpClient [GetTokenAsync] Error => |Header GUID :{apiKeyHeaderGuid}  | RequestBody:{requestBody} | ResponseBody:{responseBody}",
                        fileName: _logFolderName,
                        _apiDefinition.ApiUser.SecretKey,
                        requestDto,
                        tokenModel.StringContent
                    );
                    throw new Exception($"Token alınamadı, API'den geçerli bir yanıt gelmedi. API Error: {tokenModel.StringContent}");
                }

                var bearerToken = model?.Data?.Token;
                var refreshToken = model?.Data?.RefreshToken;

                if (!string.IsNullOrEmpty(bearerToken))
                {
                    var expirationUtc = model?.Data?.Expiration;
                    TimeSpan duration;

                    if (expirationUtc.HasValue)
                    {
                        var nowUtc = DateTime.UtcNow;
                        duration = expirationUtc.Value - nowUtc;

                        if (duration <= TimeSpan.Zero)
                            duration = TimeSpan.FromMinutes(1);
                    }
                    else
                    {
                        duration = TimeSpan.FromMinutes(59);
                    }

                    _cache.Set("AkilliETicaretToken", bearerToken, duration);
                    _cache.Set("AkilliETicaretRefreshToken", refreshToken);
                    return bearerToken;
                }
            }

            throw new Exception("Token alınamadı, API'den geçerli bir yanıt gelmedi.");
        }

        private async Task<string> RefreshTokenAsync()
        {
            if (_apiDefinition == null || _apiDefinition.ApiUser == null)
            {
                throw new InvalidOperationException("ApiDefinition veya ApiUser bilgileri eksik.");
            }

            if (!_cache.TryGetValue("AkilliETicaretRefreshToken", out string refreshToken) || string.IsNullOrEmpty(refreshToken))
            {
                throw new Exception("RefreshToken bulunamadı.");
            }

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            string baseUrl = _apiDefinition.Domain;

            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            httpClient.DefaultRequestHeaders.Add("GUID", _apiDefinition.ApiUser.SecretKey);

            var restClient = new RestEase.RestClient(httpClient)
            {
                JsonSerializerSettings = jsonSerializerSettings
            };

            var akilliETicaretClient = restClient.For<IAkilliETicaretClient>();

            var requestDto = new Domain.Dtos.AkilliETicaret.AkilliETicaretRefreshLoginRequestDto()
            {
                RefreshToken = refreshToken
            };

            var tokenModel = await akilliETicaretClient.RefreshLoginAsync(requestDto);

            if (tokenModel.ResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var model = JsonConvert.DeserializeObject<AkilliETicaretResponse<AkilliETicaretLoginResponseDto>>(tokenModel.StringContent);

                if (!model.Status)
                {
                    Logger.Error(
                        "AkilliETicaret HttpClient [RefreshLoginAsync] Error => |Header GUID :{apiKeyHeaderGuid}  | RequestBody:{requestBody} | ResponseBody:{responseBody}",
                        fileName: _logFolderName,
                        _apiDefinition.ApiUser.SecretKey,
                        requestDto,
                        tokenModel.StringContent
                    );
                    throw new Exception($"Token alınamadı, API'den geçerli bir yanıt gelmedi. API Error: {tokenModel.StringContent}");
                }

                var bearerToken = model?.Data?.Token;
                var newRefreshToken = model?.Data?.RefreshToken;

                if (!string.IsNullOrEmpty(bearerToken))
                {
                    var expirationUtc = model?.Data?.Expiration;
                    TimeSpan duration;

                    if (expirationUtc.HasValue)
                    {
                        var nowUtc = DateTime.UtcNow;
                        duration = expirationUtc.Value - nowUtc;

                        if (duration <= TimeSpan.Zero)
                            duration = TimeSpan.FromMinutes(1);
                    }
                    else
                    {
                        duration = TimeSpan.FromMinutes(59);
                    }

                    _cache.Set("AkilliETicaretToken", bearerToken, duration);
                    _cache.Set("AkilliETicaretRefreshToken", newRefreshToken);
                    return bearerToken;
                }
            }

            throw new Exception("Refresh token ile yeni token alınamadı.");
        }
    }
}
