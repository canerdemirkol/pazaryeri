#region General Using
using RestEase;
using System.Text;
using Microsoft.Extensions.Options;
#endregion

#region Project Using
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Dtos;
#endregion

namespace OBase.Pazaryeri.Business.Client.Concrete
{
    public class HepsiExpressOrderClient : IHepsiExpressOrderClient
    {
        #region Private
        private readonly IOptions<AppSettings> _appSettings;
        private ApiDefinitions _apiDefinition;
        private IHepsiExpressOrderClient _client;
        private string encoded = "";
        #endregion

        #region Const
        public HepsiExpressOrderClient(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;

            _apiDefinition = _appSettings.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.HepsiExpress);           
        }

        public void Dispose()
        {
            if (_client is not null)
                _client.Dispose();
        }

        #endregion
        #region Orders
        public async Task<Response<HEUpdateProductRequestDto.Root>> CancelOrderByLineItemId([Path] string merchantid, string lineitemid, [Body] HEReasonDto reasonModel)
        {
            return await _client.CancelOrderByLineItemId(merchantid, lineitemid, reasonModel);
        }

        public async Task<Response<HEPutUpdatePackageResponseDto.Root>> ChangeHexOrder([Path] string merchantid, [Path] string lineitemid, [Body] HEChangeOrderDetailsDto dto)
        {
            return await _client.ChangeHexOrder(merchantid, lineitemid, dto);
        }

        public async Task<Response<HEUpdateProductRequestDto.Root>> CompleteOrder([Path] string merchantid, [Body] HECompleteOrderRequestDto model)
        {
            return await _client.CompleteOrder(merchantid, model);
        }

        public async Task<Response<HEPutUpdatePackageResponseDto.Root>> CreateHexOrder([Path] string merchantid, [Body] HECreateOrderDto dto)
        {
            return await _client.CreateHexOrder(merchantid, dto);
        }

        public async Task<Response<HEPutUpdatePackageResponseDto.Root>> GetOrderDetails([Path] string merchantid, [Path] string orderNumber)
        {
            return await _client.GetOrderDetails(merchantid, orderNumber);
        }

        public async Task<Response<HEUpdateProductRequestDto.Root>> PutAsPicked([Path] string merchantid, [Path] string OrderNumber)
        {
            return await _client.PutAsPicked(merchantid, OrderNumber);
        }

        public async Task<Response<HEUpdateProductRequestDto.Root>> PutOrderAsCanceled([Path] string merchantid, [Path] string OrderNumber, [Body] HEReasonDto model)
        {
            return await _client.PutOrderAsCanceled(merchantid, OrderNumber, model);
        }

        public async Task<Response<HEUpdateProductRequestDto.Root>> UnPackOrder([Path] string merchantid, [Path] string packageNumber)
        {
            return await _client.UnPackOrder(merchantid, packageNumber);
        }
        public async Task<Response<CommonResponseDto>> InTransitOrder([Path] string merchantid, [Path] string packagenumber, [Body] HEInTransitRequestDto.Root model)
        {
            return await _client.InTransitOrder(merchantid, packagenumber, model);
        }
        public async Task<Response<CommonResponseDto>> UnDeliverOrder([Path] string merchantid, [Path] string packagenumber, [Body] HEUnDeliverRequestDto.Root model)
        {
            return await _client.UnDeliverOrder(merchantid, packagenumber, model);
        }
        public async Task<Response<CommonResponseDto>> DeliverOrder([Path] string merchantid, [Path] string packagenumber, [Body] HEDeliverRequestDto.Root model)
        {
            return await _client.DeliverOrder(merchantid, packagenumber, model);
        }
        #endregion
    }
}