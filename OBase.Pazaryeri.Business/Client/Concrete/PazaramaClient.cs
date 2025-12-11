#region General
using RestEase;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
#endregion

#region Project
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Business.Client.Abstract;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Domain.Dtos.Pazarama;
using OBase.Pazaryeri.Domain.Dtos.Pazarama.PushPrice;

#endregion

namespace OBase.Pazaryeri.Business.Client.Concrete
{

    public class PazaramaClient : IPazaramaClient
    {
        #region Private
        private readonly IPazaramaClient _client;
        private readonly IPazaramaClient bearerClient;
        private readonly ApiDefinitions _apiDefinition;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.Pazarama);

		#endregion

        #region Const
        public PazaramaClient(IOptions<AppSettings> _appSettings)
        {
            _apiDefinition = _appSettings.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.Pazarama);         
        }


        public void Dispose()
        {
            if (_client is not null)
                _client.Dispose();

            if (bearerClient is not null)
                bearerClient.Dispose();
        }


        #endregion

        #region General
        public async Task<Response<PazaramaResponse<GetPazaramaTokenResponseDto.Data>>> GetToken(string authorization, string accept, Dictionary<string, string> data)
        {
            return await _client.GetToken(authorization,accept,data);
        }
        #endregion

        #region Price / Stock
        public async Task<Response<PazaramaResponse<string>>> ProductPriceUpdate([Body] PostPazaramaProductStockAndPriceUpdateRequestDto.Root dto)
        {
            return await bearerClient.ProductPriceUpdate(dto);
        }

        public async Task<Response<PazaramaResponse<string>>> ProductStockUpdate([Body] PostPazaramaProductStockAndPriceUpdateRequestDto.Root dto)
        {
            return await bearerClient.ProductStockUpdate(dto);
        }
        #endregion
    }
}