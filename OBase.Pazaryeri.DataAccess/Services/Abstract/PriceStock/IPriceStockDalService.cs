using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.PriceStock
{
    public interface IPriceStockDalService : IBaseDalService
    {
        Task<List<long>> GetAvailableRefIdsByMerchantNoAsync(CommonEnums.JobType jobType, string merchantNo);
        Task<List<long>> GetAvailableRefIdsAsync(CommonEnums.JobType jobType, bool workWithOld = false, List<string> merchantNos = null);
        Task<List<string>> GetStoreNoListAsync(long refId, string merchantNo);
        Task<IEnumerable<MerchantVerify>> GetAvailableVerifiablesAsync(string MerchantNo);
        Task<PazarYeriJobResultDetails> GetPazarYeriJobResultDetailAsync(long refId, long detailId);
        Task<List<PazarYeriJobResultDetails>> GetJobResultDetailsByIdMerchAsync(long refId, string MerhantId, string MerhantNo,bool satisFiyatControl=false);
        Task<bool> InsertPazarYeriJobResultAsync(PazarYeriJobResult model);
        Task<bool> InsertPazarYeriLogAsync(PazarYeriLog model);
        Task<bool> UpdatePazarYeriJobResultDetailAsync(PazarYeriJobResultDetails model);
        Task<int> UpdateServiceLog(long RefId, string ParDurum, string ParaHataMesaji);
        Task<PazarYeriJobResult> GetPazarYeriJobResultByRefIdAsync(long refId);
        Task<bool> UpdatePazarYeriJobResultAsync(PazarYeriJobResult model);
        Task<IEnumerable<PazarYeriJobResultDetails>> GetPazarYeriJobResultDetailsByRefIdAsync(long refId, int threadNo,string guid);
        Task<List<PazarYeriJobResultDetails>> GetPazarYeriJobResultDetailsAsync(List<long> refIds, List<long> detailIds);
        Task UpdatePazarYeriJobResultDetailRangeAsync(List<PazarYeriJobResultDetails> model);
        Task<List<PazarYeriJobResultDetails>> GetPazarYeriJobResultDetailsAsync(long refId, int threadNo, string guid, string merchantId);
        Task<PazarYeriJobResultDetails> GetPazarYeriJobResultDetailAsync(long refId, long detailId, int threadNo, string guid, string merchantId);
        Task<bool> AddRangePazarYeriLogAsync(List<PazarYeriLog> models);
        Task UpdatePazarYeriJobResultDetailRangeVerifiedAsync(List<PazarYeriJobResultDetails> model);
    }
}
