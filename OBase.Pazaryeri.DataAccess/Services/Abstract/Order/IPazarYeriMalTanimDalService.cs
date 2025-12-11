using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Order
{
    public interface IPazarYeriMalTanimDalService :IBaseDalService
    {
        Task<IEnumerable<PazarYeriMalTanim>> GetProductSalesValueByUnitAsync(string unit,string merchantNo);
        Task<IEnumerable<PazarYeriMalTanim>> GetProductObaseNoAsync(string merchantNo);
        Task<string> GetProductMarketPlaceIdByObaseProductIdAsync(string productNo, string merchantNo);
        Task<IEnumerable<PazarYeriMalTanim>> GetPyProductsAsync(string merchantNo, List<string> malNos = null);
        Task<decimal> GetProductSalesValue(string MerchantNo, string MerchantSKU);
        Task<IEnumerable<PazarYeriMalTanim>> GetProductDetailsAsync(string MerchantNo, string MerchantSKU);
        Task<IEnumerable<PazarYeriMalTanim>> GetPyProductNosAsync(List<string> pyProductNos, string merchantNo);
        Task<bool> UpdateProductAsync(PazarYeriMalTanim model);
    }
}