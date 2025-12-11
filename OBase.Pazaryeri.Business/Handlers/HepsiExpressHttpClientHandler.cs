using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Handlers
{
    public class HepsiExpressHttpClientHandler : DelegatingHandler
    {
        private readonly IMemoryCache _cache;
        private readonly ApiDefinitions _apiDefinition;

        public HepsiExpressHttpClientHandler(IMemoryCache cache, IOptions<AppSettings> _appSettings)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _apiDefinition = _appSettings?.Value?.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.HepsiExpress);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }

}
