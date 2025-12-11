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
using OBase.Pazaryeri.Domain.Dtos.Idefix.Unsupplied;
using System.DirectoryServices.Protocols;
using OBase.Pazaryeri.Domain.Dtos.Idefix.Shipment;
using RestEase.Implementation;
using OBase.Pazaryeri.Domain.Dtos;

#endregion

namespace OBase.Pazaryeri.Business.Client.Concrete
{

    public class OsmIdefixClient : IOsmIdefixClient
    {
        #region Private
        private readonly IOsmIdefixClient _client;
        private readonly ApiDefinitions _apiDefinition;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ILogger<PimIdefixClient> _logger;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.Idefix);

        #endregion

        #region Const
        public OsmIdefixClient(ILogger<PimIdefixClient> logger, IOptions<AppSettings> appSettings)
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

        #region Order  

        public async Task<Response<IdefixGenericResponse<IdefixOrderDto>>> GetShipmentList([Path] string vendorId, [Query] string page, [Query] string startDate = "", [Query] string endDate = "", [Query] string state = "", [Query] int limit = 200)
        {
            return await _client.GetShipmentList(vendorId: vendorId, page: page, startDate: startDate, endDate: endDate, state: state, limit: limit);
        }
        public async Task<Response<CommonResponseDto>> UpdateShipmentStatusAsync([Path] string vendorId, [Path] string shipmentId, [Body] UpdateShipmentStatusRequest body)
        {
            return await _client.UpdateShipmentStatusAsync(vendorId, shipmentId, body);
        }
        #endregion

        #region Claim
        public async Task<Response<UnsuppliedResponse>> MarkShipmentAsUnsuppliedAsync([Path] string vendorId, [Path] string shipmentId, [Body] UnsuppliedRequest request)
        {
            return await _client.MarkShipmentAsUnsuppliedAsync(vendorId, shipmentId, request);
        }
        #endregion
    }
}