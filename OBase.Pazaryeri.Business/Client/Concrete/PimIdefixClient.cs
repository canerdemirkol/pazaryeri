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
using Microsoft.Extensions.Logging;
using OBase.Pazaryeri.Domain.Dtos.Getir.PriceStock;
using OBase.Pazaryeri.Domain.Dtos.Idefix.PriceStock;
using OBase.Pazaryeri.Domain.Dtos.Idefix;

#endregion

namespace OBase.Pazaryeri.Business.Client.Concrete
{

    public class PimIdefixClient : IPimIdefixClient
    {
        #region Private
        private readonly IPimIdefixClient _client;
        private readonly ApiDefinitions _apiDefinition;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ILogger<PimIdefixClient> _logger;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.Idefix);

		#endregion

        #region Const
        public PimIdefixClient(ILogger<PimIdefixClient> logger, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _logger = logger;
            _apiDefinition = _appSettings.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.Idefix);

        }


        public void Dispose()
        {
            if (_client is not null)
                _client.Dispose();

        }
        
        #endregion

        #region Price / Stock     
        public async Task<Response<IdefixGenericResponse<ProductInventoryItemResponse>>> UpdatePriceAndQuantityWithVendor([Path] string vendorId, [Body] IdefixProductPriceAndStockUpdateWithVendorRequestDto dto)
        {
            return await _client.UpdatePriceAndQuantityWithVendor(vendorId,dto);
        }


        public async Task<Response<IdefixGenericResponse<ProductInventoryItemWithBatchRequesIdResponse>>> GetBatchRequestsControlById([Path] string vendorId, [Path] string batchId)
        {
            return await _client.GetBatchRequestsControlById(vendorId, batchId);
        }
        #endregion
    }
}