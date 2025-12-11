using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.QuickPick;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.Trendyol;
using OBase.Pazaryeri.Domain.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Domain.ConfigurationOptions;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.QuickPick
{
    public class QuickPickDalService : IQuickPickDalService
	{
		private readonly IGetDalService _getDalService;
		private readonly IRepository _repository;
        private readonly IOptions<AppSettings> _appSettings;

        public QuickPickDalService(IGetDalService getDalService, IRepository repository, IOptions<AppSettings> appSettings)
        {
            _getDalService = getDalService;
            _repository = repository;
            _appSettings = appSettings;
        }


        public async Task<string> GetInProductBarcodeByBarcodeAsync(string barcode)
		{
			var result = await _getDalService.GetAsync<MalBarkod>(x => x.Barkod == barcode);
			return result?.MalNo ?? "";
		}

		public async Task<IEnumerable<ProductBarcodeDto>> GetInProductBarcodeByProductCodeAsync(string productCode)
		{
			return await _getDalService.GetTable<MalBarkod>(x => x.MalNo == productCode).OrderByDescending(x => x.Oncelikli).Select(x => new ProductBarcodeDto
			{
				Barcode = x.Barkod,
				IsPriority = x.Oncelikli == 1,
			}).ToListAsync();
		}

		public async Task<string> GetMarketPlaceStoreIdByObaseStoreIdAsync(string obaseStoreId, string merchantNo)
		{
			var result = await _getDalService.GetAsync<PazarYeriBirimTanim>(x => x.BirimNo == obaseStoreId && x.PazarYeriNo == merchantNo);
			return result.PazarYeriBirimNo;
		}

		public string GetDecodedMerchantNo(string saleChannelId)
		{
            return _repository.ExecuteSqlCommand<DecodedPazarYeriNoView>(_appSettings.Value.RawDatabaseQueries.DecodedPazarYeriNoQuery).FirstOrDefault().DecodedPazarYeriNo;
		}

		public async Task<TrendyolCallCustomerDto> GetOrderNumberAndMarketPlaceStoreIdByQpIdAsync(long qpOrderId)
		{
			var result = await _getDalService.GetAsync<PazarYeriSiparis>(x => x.Id == qpOrderId, x => x.PazarYeriSiparisDetails);
			if (result is null)
				return null;

			return new TrendyolCallCustomerDto
			{
				OrderNumber = result.SiparisNo,
				PyStoreId = result.PazarYeriSiparisDetails.FirstOrDefault()?.PazarYeriBirimId
			};
		}

		public async Task<IEnumerable<string>> GetProductBarcodesByBarcodeAndSaleChannelIdAsync(string productCode, string saleChannelId)
		{
			return await _getDalService.GetTable<PazarYeriAktarim>(x => x.MalNo == productCode && x.PazarYeriNo == saleChannelId).Select(x => x.PazarYeriMalNo).ToListAsync();
		}

		public async Task<IEnumerable<ProductBarcodeDto>> GetProductBarcodesWithPriorityNumberAsync(string productCode)
		{
			return await _getDalService.GetTable<MalBarkod>(x => x.MalNo == productCode).OrderByDescending(x => x.Oncelikli).Select(x => new ProductBarcodeDto
			{
				Barcode = x.Barkod,
				IsPriority = x.Oncelikli == 1,
			}).ToListAsync();
		}

		public async Task<string> GetProductCodeByBarcodeAndSaleChannelIdAsync(string barcode, string saleChannelId)
		{
			var result = await _getDalService.GetAsync<PazarYeriAktarim>(x => x.PazarYeriMalNo == barcode && x.PazarYeriNo == saleChannelId);
			return result.MalNo;
		}

		public string GetProductPhoto(string pystoreId, string obaseProductNo)
		{
            var result = _repository.ExecuteSqlCommand<PazarYeriProductImageView>(_appSettings.Value.RawDatabaseQueries.ProductPhotosQuery, new List<OracleParameter> {

		new OracleParameter
		{
			OracleDbType = OracleDbType.Varchar2,
			Direction = ParameterDirection.Input,
			ParameterName = Db.RawQuery.ProductPhotosQuery.Parameters.PazarYeriBirimId,
			Value = pystoreId
		},
		new OracleParameter
		{
			OracleDbType = OracleDbType.Varchar2,
			Direction = ParameterDirection.Input,
			ParameterName = Db.RawQuery.ProductPhotosQuery.Parameters.ObaseMalNo,
			Value = obaseProductNo
		}}.ToArray()).FirstOrDefault();
			return result?.ProductImage?? "";
		}

		public async Task<ProductDetailDto> ProductAllInfoMerchantViewAsync(string productCode, string saleChannelId, string storeId)
		{
			var result = await _getDalService.GetAsync<VPazaryeriUrunler>(x => x.MalNo == productCode && x.PazarYeriNo == saleChannelId && x.BirimNo == storeId);

			if (result is null)
				return null;

			return new ProductDetailDto
			{
				ProductCode = result.MalNo,
				ProductModelName = result.PazarYeriMalAdi,
				ImageMedium = "",
				ImageSmall = "",
				Unit = result.MalSatisBirimKod,
				CategoryId = result.KategoriKod,
				CategoryName = result.KategoriAdi,
				Price = result.SatisFiyat,
				StockQuantity = result.StokMiktar,
				VAT = result.MalSatisKdvDeger,
				Frozen = false
			};
		}
	}
}
