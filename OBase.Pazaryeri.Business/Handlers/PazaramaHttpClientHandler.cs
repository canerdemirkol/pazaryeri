using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
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

namespace OBase.Pazaryeri.Business.Handlers
{
    public class PazaramaHttpClientHandler : DelegatingHandler
    {
        private readonly IMemoryCache _cache;
        private readonly ApiDefinitions _apiDefinition;
        private readonly int[] retryDelays = { 1000, 2000, 3000, 5000, 10000 }; // 1s, 2s, 3s, 5s, 10s

        public PazaramaHttpClientHandler(IMemoryCache cache, IOptions<AppSettings> _appSettings)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _apiDefinition = _appSettings?.Value?.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Pazarama);
                             
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await EnsureValidTokenAsync(request);

            HttpResponseMessage response = null;
            int attempt = 0;
            bool tokenRefreshed = false;

            while (attempt < retryDelays.Length)
            {
                response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.OK)
                    return response;

                if (response.StatusCode == HttpStatusCode.Unauthorized && !tokenRefreshed)
                {
                    await RefreshTokenAsync();
                    tokenRefreshed = true; // Bir kez token yenilensin
                    continue;
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < retryDelays.Length)
                {
                    await Task.Delay(retryDelays[attempt], cancellationToken);
                    attempt++;
                    continue;
                }

                return response;
            }

            throw new HttpRequestException("429 Too Many Requests - Retry limitine ulaşıldı.");
        }


        private async Task EnsureValidTokenAsync(HttpRequestMessage request)
        {
            if (!_cache.TryGetValue("PazaramaToken", out string bearerToken) || string.IsNullOrEmpty(bearerToken))
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

            var pazaramaClient = restClient.For<IPazaramaClient>();

            string accept = "application/json";
            string username = _apiDefinition.ApiUser.Username;
            string password = _apiDefinition.ApiUser.Password;
            string encodedAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{password}"));

            string authorization = $"Basic {encodedAuth}";

            var tokenData = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "scope", "mahallemerchant.fullaccess" }
            };

            var tokenModel = await pazaramaClient.GetToken(authorization, accept, tokenData);        
            if (tokenModel.ResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                var model = JsonConvert.DeserializeObject<PazaramaResponse<GetPazaramaTokenResponseDto.Data>>(tokenModel.StringContent);
                var bearerToken = model?.Data?.AccessToken;

                if (!string.IsNullOrEmpty(bearerToken))
                {
                    _cache.Set("PazaramaToken", bearerToken, TimeSpan.FromMinutes(59));
                    return bearerToken;
                }
            }

            throw new Exception("Token alınamadı, API'den geçerli bir yanıt gelmedi.");
        }

    }


}
