#region General
using Microsoft.Extensions.Options;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

#region Project
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Business.Utility;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using static OBase.Pazaryeri.Domain.Constants.Constants;
#endregion

namespace OBase.Pazaryeri.Business.Client.Concrete
{
    public class HepsiexpressClient : IHepsiExpressClient
    {
        #region Private
        private readonly IOptions<AppSettings> _appSettings;
        private IHepsiExpressClient _client;
        private ApiDefinitions _apiDefinition;
        private string encoded = "";
        #endregion

        #region Const
        public HepsiexpressClient(IOptions<AppSettings> appSettings)
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

        #region Price / Stock
        public async Task<Response<HEUpdateProductsRespDto>> UpdateListingProductStock([Path] string merchantid, [Body] Listings dto)
        {
            return await _client.UpdateListingProductStock(merchantid, dto);
        }
        public async Task<Response<HEGetRequestResponseDto>> GetRequestsControlById([Path] string merchantid, [Path] string requestid)
        {
            return await _client.GetRequestsControlById(merchantid, requestid);
        }
        public async Task<Response<HEListingDetailsDto>> GetListings([Path] string merchantid)
        {
            return await _client.GetListings(merchantid);
        }
        #endregion

        #region Discount
        public async Task<Response<HEListingDiscountResponseDto>> DeleteDiscount([Path] string id)
        {
            return await _client.DeleteDiscount(id);
        }
        public async Task<Response<HEGetListngDiscountResponseDto.DiscountsResult>> GetByMerchantIdAndStatusDiscount([Path] string merchantid, [Path] string status)
        {
            return await _client.GetByMerchantIdAndStatusDiscount(merchantid, status);
        }
        public async Task<Response<HEGetListngDiscountResponseDto.DiscountsResult>> GetDiscount([Path] string discountid)
        {
            return await _client.GetDiscount(discountid);
        }
        public async Task<Response<HEListingDiscountResponseDto>> InsertDiscount([Path] string merchantid, [Body] HEListingDiscountRequestDto dto)
        {
            return await _client.InsertDiscount(merchantid, dto);
        }
        public async Task<Response<HEListingDiscountResponseDto>> UpdateDiscount([Path] string discountid, [Body] HEListingDiscountRequestDto dto)
        {
            return await _client.UpdateDiscount(discountid, dto);
        }
        #endregion
    }
}