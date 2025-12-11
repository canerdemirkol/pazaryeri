#region General
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestEase;
#endregion

#region Project
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using System.Text;
using static OBase.Pazaryeri.Domain.Dtos.TrendyolGo.TGUpdateStockAndPriceReqDto;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Dtos.Trendyol;
#endregion

namespace OBase.Pazaryeri.Business.Client.Concrete
{
	public class TrendyolGoClient : ITrendyolGoClient
    {
        #region Private
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ILogger<TrendyolGoClient> _logger;
        private readonly ITrendyolGoClient _client;
        private readonly ApiDefinitions _apiDefinition;
        #endregion

        #region Const
        public TrendyolGoClient(ILogger<TrendyolGoClient> logger, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _logger = logger;
            _apiDefinition = _appSettings.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.TrendyolGo);
        }
        #endregion

        #region Orders
        public async Task<Response<TGInvoiceLimitResponseDto>> GetInvoiceAmount([Path] string supplierId, [Path] string orderId)
        {
            return await _client.GetInvoiceAmount(supplierId, orderId);
        }
        public async Task<Response<TGShipmentPackageResponseDto>> GetOrderPackagesByOrderNumber([Path] string supplierId, [Path] string orderNumber)
        {
            return await _client.GetOrderPackagesByOrderNumber(supplierId, orderNumber);
        }
        public async Task<Response<TGShipmentPackageResponseDto>> GetShipmentPackages([Path] string supplierId, [Path] string status, [Path] string storeId, [Path] string startDate, [Path] string endDate, [Path] string page)
        {
            return await _client.GetShipmentPackages(supplierId,status, storeId, startDate, endDate, page);
        }
        public async Task<Response<CommonResponseDto>> ManualDelivered([Path] string sellerId, [Path] string packageId)
        {
            return await _client.ManualDelivered(sellerId, packageId);
        }
        public async Task<Response<CommonResponseDto>> ManualShipped([Path] string sellerId, [Path] string packageId)
        {
            return await _client.ManualShipped(sellerId, packageId);
        }
        public async Task<Response<CommonResponseDto>> MarkAlternative([Path] string sellerId, [Path] string packageId, [Body] TGMarkAlternativeRequestDto markAlternativeDto)
        {
            return await _client.MarkAlternative(sellerId, packageId, markAlternativeDto);
        }
        public async Task<Response<CommonResponseDto>> Picked([Path] string supplierId, [Path] string packageId)
        {
            return await _client.Picked(supplierId, packageId);
        }
        public async Task<Response<CommonResponseDto>> PutUpdatePackage([Path] string sellerId, [Path] string packageId, [Path] string status)
        {
            return await _client.PutUpdatePackage(sellerId, packageId, status);
        }
        public async Task<Response<CommonResponseDto>> UpdatePackageAsInvoiced([Path] string supplierId, [Path] string packageId, [Body] TGUpdatePackageInvoicedRequestDto updatePackageAsInvoicedDto)
        {
            return await _client.UpdatePackageAsInvoiced(supplierId, packageId, updatePackageAsInvoicedDto);
        }
        public async Task<Response<CommonResponseDto>> UpdatePackageAsUnSupplied([Path] string sellerId, [Path] string packageId, [Body] TGUpdatePackageUnSuppliedRequestDto updatePackageAsUnSuppliedDto)
        {
            return await _client.UpdatePackageAsUnSupplied(sellerId, packageId, updatePackageAsUnSuppliedDto);
        }
        public async Task<Response<TrendyolCallCustomerResponseDto>> GetInstantCall([Path] string supplierId, [Path] string packageId, [Body] TGGetInstantCallRequestDto instantCallDto)
        {
            return await _client.GetInstantCall(supplierId, packageId, instantCallDto);
        }
        #endregion

        #region Price / Stock
        public async Task<Response<TGUpdateStockAndPriceRespDto>> StoreUpdatePriceAndInventory([Path] string sellerId, [Body] RequestItems dto)
        {
            return await _client.StoreUpdatePriceAndInventory(sellerId, dto);
        }
        public async Task<Response<TGGetBatchRequestResultDto.Root>> GetBatchRequestResult([Path] string sellerId, [Path] string batchRequestId)
        {
            return await _client.GetBatchRequestResult(sellerId, batchRequestId);
        }
		#endregion

		#region Iade
		public async Task<Response<CommonResponseDto>> AcceptClaimAsync([Path] string sellerId, [Path] string claimId, [Body] TGAcceptClaimRequestDto acceptClaimDto)
		{
			return await _client.AcceptClaimAsync(sellerId, claimId, acceptClaimDto);
		}
		public async Task<Response<CommonResponseDto>> RejectClaimAsync([Path] string sellerId, [Path] string claimId, [Body] TGRejectClaimRequestDto rejectClaimDto)
		{
			return await _client.RejectClaimAsync(sellerId, claimId, rejectClaimDto);
		}
		public async Task<Response<TGGetReturnedPackagesResponseDto>> GetReturnedPackagesAsync([Path] string sellerId, [Path] string status, [Path] string startDate, [Path] string endDate, [Path] string page)
		{
			return await _client.GetReturnedPackagesAsync(sellerId, status, startDate, endDate, page);
		}   
        #endregion
    }
}
