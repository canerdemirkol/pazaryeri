using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Order
{
    public interface IPazarYeriAktarimDalService:IBaseDalService
    {
        Task<IEnumerable<PazarYeriAktarim>> GetProductObaseNoAndBarcodeAsync(string merchantNo, string storeId);
		Task<IEnumerable<PazarYeriAktarim>> GetPyTransferredProductsAsync(string merchantNo, string storeId, List<string> pazaryeriBarcodeList=null);
        Task<IEnumerable<PazarYeriAktarim>> GetProductsByProductNoAsync(List<string> ObaseProducts, string merchantNo);
        Task<IEnumerable<PazarYeriAktarim>> GetPyTransferredProductListAsync(string merchantNo, string storeId, List<string> orderSkuList = null);
    }
}