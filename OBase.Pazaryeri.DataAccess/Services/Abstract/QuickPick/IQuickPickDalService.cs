using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.Trendyol;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.QuickPick
{
    public interface IQuickPickDalService
	{
		string GetDecodedMerchantNo(string saleChannelId);
		Task<string> GetProductCodeByBarcodeAndSaleChannelIdAsync(string barcode, string saleChannelId);
		Task<string> GetInProductBarcodeByBarcodeAsync(string barcode);
		Task<IEnumerable<string>> GetProductBarcodesByBarcodeAndSaleChannelIdAsync(string productCode, string saleChannelId);
		Task<IEnumerable<ProductBarcodeDto>> GetInProductBarcodeByProductCodeAsync(string productCode);
		Task<IEnumerable<ProductBarcodeDto>> GetProductBarcodesWithPriorityNumberAsync(string productCode);
		Task<ProductDetailDto> ProductAllInfoMerchantViewAsync(string productCode, string saleChannelId, string storeId);
		Task<string> GetMarketPlaceStoreIdByObaseStoreIdAsync(string obaseStoreId, string merchantNo);
		string GetProductPhoto(string pystoreId, string obaseProductNo);
		Task<TrendyolCallCustomerDto> GetOrderNumberAndMarketPlaceStoreIdByQpIdAsync(long qpOrderId);

	}
}
