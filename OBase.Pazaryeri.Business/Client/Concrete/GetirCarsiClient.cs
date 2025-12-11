#region General
using RestEase;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
#endregion

#region Project
using OBase.Pazaryeri.Domain.Dtos.Getir;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.Getir.PriceStock;
using OBase.Pazaryeri.Business.Client.Abstract;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Dtos.Getir.Return;
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Domain.Dtos.Getir.Product;
using OBase.Pazaryeri.Domain.Dtos.Getir.Login;
#endregion

namespace OBase.Pazaryeri.Business.Client.Concrete
{
    public class GetirCarsiClient : IGetirCarsiClient
    {
        #region Private
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IGetirCarsiClient _client;
        private readonly IGetirCarsiClient bearerClient;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.GetirCarsi);
        #endregion

        #region Ctor
        public GetirCarsiClient(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _apiDefinition = _appSettings.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.GetirCarsi);
        }


        public void Dispose()
        {
            if(_client is not null)
                _client.Dispose();

            if (bearerClient is not null)
                bearerClient.Dispose();
        }

        #endregion

        #region General
        public async Task<Response<AuthTokenResponseDto>> GetToken(string authorization, string accept)
        {
            return await _client.GetToken(authorization, accept);
        }
        public async Task<Response<GetirResponse>> ResetPassword([Body] ResetPasswordDto resetPasswordDto)
        {
            CheckGetirBearerClient();
            return await bearerClient.ResetPassword(resetPasswordDto);
        }
        #endregion

        #region Orders
        public async Task<Response<GetirResponse>> Verify([Path] string orderId, [Path] string shopId)
        {
            CheckGetirBearerClient();
            return await bearerClient.Verify(orderId, shopId);
        }
        public async Task<Response<GetirResponse>> Prepare([Path] string orderId, [Path] string shopId, [Body] PrepareDto preapareDto)
        {
            CheckGetirBearerClient();
            return await bearerClient.Prepare(orderId, shopId, preapareDto);
        }
        public async Task<Response<GetirResponse>> Handover([Path] string orderId, [Path] string shopId)
        {
            CheckGetirBearerClient();
            return await bearerClient.Handover(orderId, shopId);
        }
        public async Task<Response<GetirResponse>> Deliver([Path] string orderId, [Path] string shopId)
        {
            CheckGetirBearerClient();
            return await bearerClient.Deliver(orderId, shopId);
        }
        public async Task<Response<GetirResponse>> Cancel([Path] string orderId, [Path] string shopId, [Body] CancelDto cancelDto)
        {
            CheckGetirBearerClient();
            return await bearerClient.Cancel(orderId, shopId, cancelDto);
        }
        public async Task<Response<GenericGetirResponse<CancelOptionResponseDto>>> CancelOptions([Path] string orderId)
        {
            CheckGetirBearerClient();
            return await bearerClient.CancelOptions(orderId);
        }
        #endregion

        #region Price / Stock
        public async Task<Response<GetirGetUpdateProductsResultWithBatchRequesIdtRespDto.Root>> GetBatchRequestsControlById([Path] string batchRequestId)
        {
            CheckGetirBearerClient();
            return await bearerClient.GetBatchRequestsControlById(batchRequestId);
        }
        public async Task<Response<GetirPriceAndQuantityOfProductRespDto.Root>> UpdatePriceAndQuantity([Body] GetirPriceAndQuantityOfProductReqDto.Root dto)
        {
            CheckGetirBearerClient();
            return await bearerClient.UpdatePriceAndQuantity(dto);
        }
        public async Task<Response<GetirPriceAndQuantityOfProductRespDto.Root>> UpdatePriceAndQuantityWithVendor([Body] GetirPriceAndQuantityOfProductWithVendorReqDto.Root dto)
        {
            CheckGetirBearerClient();
            return await bearerClient.UpdatePriceAndQuantityWithVendor(dto);
        }
        #endregion

        #region Return
        public async Task<Response<GenericGetirResponse<GetirReturnRespDto>>> GetReturn([Path] string returnId)
        {
            CheckGetirBearerClient();
            return await bearerClient.GetReturn(returnId);
        }
        public async Task<Response<GenericGetirResponse<GetirReturnsRespDto>>> GetReturnedPackagesAsync([Path] string shopId, [Path] string type, [Path] int page, [Path] int size, [Body] ReturnReqBody returnReqBody)
        {
            CheckGetirBearerClient();
            return await bearerClient.GetReturnedPackagesAsync(shopId, type, page, size, returnReqBody);
        }
        public async Task<Response<GenericGetirResponse<GetirReturnsRespDto>>> PostReturn([Body] GetirPostReturnReqBody returnReqBody)
        {
            CheckGetirBearerClient();
            return await bearerClient.PostReturn(returnReqBody);
        }

        public async Task<Response<GenericGetirResponse<GetirProductDataPaged>>> Products([Path] string shopId, [Path] int page = 1, [Path] int size = 1)
        {
            CheckGetirBearerClient();
            return await bearerClient.Products(shopId, page, size);
        }

        public async Task<Response<GetirResponse>> PatchReceiveReturn([Path] string shopId, [Path] string returnId)
        {
            CheckGetirBearerClient();
            return await bearerClient.PatchReceiveReturn(shopId, returnId);
        }
        #endregion
        private void CheckGetirBearerClient()
        {
            if (bearerClient is null)
                throw new Exception("Getir Çarşı'dan token alınamadı.");
        }
       
    }
}