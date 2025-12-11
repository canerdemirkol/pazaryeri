using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.AkilliETicaret;
using RestEase;
using System.Drawing.Printing;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Client.Concrete
{
    public class AkilliETicaretClient : IAkilliETicaretClient
    {
        private readonly IAkilliETicaretClient _akilliETicaretClient;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string _token;
        public AkilliETicaretClient(IOptions<AppSettings> appSettings)
        {
            _apiDefinition = appSettings.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.AkilliETicaret);
        }
        public async Task<Response<AkilliETicaretResponse<AkilliETicaretBatchProductResponseDto>>> BatchProductAsync(List<AkilliETicaretProductMasterUpsertDto> products)
        {
            return await _akilliETicaretClient.BatchProductAsync(products);
        }

        public async Task<Response<AkilliETicaretResponse<AkilliETicaretLoginResponseDto>>> LoginAsync(AkilliETicaretLoginRequestDto requestDto)
        {
            return await _akilliETicaretClient.LoginAsync(requestDto);
        }

        public async Task<Response<AkilliETicaretResponse<AkilliETicaretLoginResponseDto>>> RefreshLoginAsync(AkilliETicaretRefreshLoginRequestDto requestDto)
        {
            return await _akilliETicaretClient.RefreshLoginAsync(requestDto);
        }
    }
}