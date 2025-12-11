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
    public class IdefixHttpClientHandler : DelegatingHandler
    {
        private readonly IMemoryCache _cache;
        private readonly ApiDefinitions _apiDefinition;
        private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

        private readonly string _logFolderName = nameof(PazarYerleriHttpClient.IdefixHttpClient);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="_appSettings"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IdefixHttpClientHandler(IMemoryCache cache, IOptions<AppSettings> _appSettings)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _apiDefinition = _appSettings?.Value?.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Idefix);

            // Retry pipeline tanımı
            _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    DelayGenerator = args =>
                    {
                        // Retry-After header'ını oku
                        if (args.Outcome.Result?.Headers.TryGetValues("Retry-After", out var values) == true)
                        {
                            var retryAfter = values.FirstOrDefault();
                            if (double.TryParse(retryAfter, out var seconds) && seconds == 2.0)
                            {
                                // Sabit 2 saniye bekleme
                                return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(seconds));
                            }
                        }

                        // Retry-After yoksa veya 2.0 değilse retry yapma
                        return new ValueTask<TimeSpan?>((TimeSpan?)null);
                    },
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .HandleResult(r => r.StatusCode == (HttpStatusCode)429 || (int)r.StatusCode >= 500)
                        .Handle<Exception>(ex => ex is HttpRequestException || ex is SocketException),
                    OnRetry = args =>
                    {

                        Logger.Warning(
                           messageTemplate: $"Idefix [Retry] Attempt {args.AttemptNumber} - Delay {args.RetryDelay.TotalSeconds:F1}s - " + $"Reason: {args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()}",
                           fileName: _logFolderName
                        );
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            // Request Body
            string requestBody = request.Content != null
                ? await request.Content.ReadAsStringAsync()
                : string.Empty;

            string apiKeyHeader = string.Empty;

            if (request.Headers.TryGetValues("x-api-key", out var values))
            {
                apiKeyHeader = values.FirstOrDefault() ?? string.Empty;
            }

           // Logger.Information(
           //     "Idefix HttpClient Request => Url: {Url} | Method: {Method} | Headers: {Headers} | RequestBody: {RequestBody}",
           //     fileName: _logFolderName,
           //     request.RequestUri,
           //     request.Method,
           //     apiKeyHeader,
           //     requestBody
           // );



            var response = await _pipeline.ExecuteAsync(
                async ct => await base.SendAsync(request, ct),
                cancellationToken
            );

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            try
            {

                string responseBody = string.Empty;
                try
                {
                    responseBody = await response.Content.ReadAsStringAsync();

                    var error = JsonConvert.DeserializeObject<IdefixErrorResponse>(responseBody);

                    Logger.Error(
                        "Idefix HttpClient => Url {Url} => Method {Method} => Headers: {Headers} => RequestBody: {RequestBody} => Error: {Error}",
                        fileName: _logFolderName,
                        request.RequestUri,
                        request.Method,
                        apiKeyHeader,
                        requestBody,
                        JsonConvert.SerializeObject(error)
                    );
                }
                catch (Exception ex)
                {
                    Logger.Error("Error deserializing HTTP error response: {Exception}", fileName: _logFolderName, ex);
                }
         

            }
            catch (Exception ex)
            {
                Logger.Error("Error logging HTTP response: {Exception}", fileName: _logFolderName, ex);
               
            }
            return response;
        }


    }


}
