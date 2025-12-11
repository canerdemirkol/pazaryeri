using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.PriceStock;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Domain.ConfigurationOptions;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.PriceStock
{
    public class PriceStockDalService : BaseDalService, IPriceStockDalService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptions<AppSettings> _appSettings;
        public PriceStockDalService(IRepository repository, IServiceScopeFactory serviceScopeFactory, IOptions<AppSettings> appSettings) : base(repository)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _appSettings = appSettings;
        }

        #region PazarYeriJobResult
        public async Task<PazarYeriJobResult> GetPazarYeriJobResultByRefIdAsync(long refId)
        {
            return await _repository.GetTable<PazarYeriJobResult>()
                          .FirstOrDefaultAsync(detail => detail.RefId == refId);
        }

        public async Task<bool> InsertPazarYeriJobResultAsync(PazarYeriJobResult model)
        {
            try
            {
                await _repository.AddAsync(model);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> UpdatePazarYeriJobResultAsync(PazarYeriJobResult model)
        {
            try
            {
                await _repository.UpdateAsync(model);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<List<long>> GetAvailableRefIdsAsync(CommonEnums.JobType jobType, bool workWithOld = false, List<string> merchantNos = null)
        {
            List<long> refIds = new();
            if (workWithOld)
            {
                var yesterday = DateTime.Now.AddDays(-1).Date;
                refIds = await _repository.GetTable<PazarYeriJobResult>()
                    .Where(x =>
                    x.PazarYeriJobResultDetails.Any(y => y.HasSent == Character.H && merchantNos.Contains(y.PazarYeriNo))
                && x.InsertDatetime >= yesterday
                && x.JobType == Enum.GetName(typeof(CommonEnums.JobType), jobType)).Select(x => x.RefId).ToListAsync();
            }
            else
            {
                refIds = await _repository.GetTable<PazarYeriJobResult>().Where(x => x.HasSent == Character.H && x.JobType == Enum.GetName(typeof(CommonEnums.JobType), jobType)).Select(x => x.RefId).ToListAsync();
            }
            return refIds.OrderBy(x => x).ToList();
        }
        public async Task<List<long>> GetAvailableRefIdsByMerchantNoAsync(CommonEnums.JobType jobType, string merchantNo)
        {
            List<long> refIds = new();
            //NOT : All type aktarilacak urun yoksa OnlyStock urunleri etkilenmesin diye asaidaki condition kapatildi.
            //&& x.PazarYeriJobResultDetails.Any(y => y.PazarYeriNo == merchantNo)
            refIds = await _repository.GetTable<PazarYeriJobResult>().Where(x => x.HasSent == Character.H && x.JobType == Enum.GetName(typeof(CommonEnums.JobType), jobType) ).Select(x => x.RefId).ToListAsync(); 
            return refIds.OrderBy(x => x).ToList();
        }
        #endregion

        #region PazarYeriJobResultDetails
        public async Task<IEnumerable<MerchantVerify>> GetAvailableVerifiablesAsync(string MerchantNo)
        {
            var query = _repository.GetTable<PazarYeriJobResultDetails>().Where(x => x.PazarYeriNo == MerchantNo && x.HasVerified == Character.H && x.HasErrors == Character.H && x.Guid != null)
                .OrderByDescending(x => x.Tarih).ThenBy(f => f.Guid)
                .Take(1000)
                .Select(x => new MerchantVerify
                {
                    DETAIL_ID = x.DetailId,
                    REF_ID = x.RefId,
                    PAZAR_YERI_BIRIM_NO = x.PazarYeriBirimNo,
                    PAZAR_YERI_MAL_NO = x.PazarYeriMalNo,
                    THREAD_NO = x.ThreadNo,
                    MAL_NO = x.MalNo,
                    GUID = x.Guid,
                    BARKOD = x.Barkod
                });
            return await query.ToListAsync();
        }
        public async Task<List<string>> GetStoreNoListAsync(long refId, string merchantNo)
        {
            var details = await _repository.ToListAsync<PazarYeriJobResultDetails>(x => x.RefId == refId && x.PazarYeriNo == merchantNo);
            return details.GroupBy(x => x.PazarYeriBirimNo).Select(x => x.Key).ToList();
        }
        public async Task<PazarYeriJobResultDetails> GetPazarYeriJobResultDetailAsync(long refId, long detailId)
        {
            return await _repository.GetTable<PazarYeriJobResultDetails>()
                          .FirstOrDefaultAsync(detail => detail.RefId == refId && detail.DetailId == detailId);
        }
        public async Task<List<PazarYeriJobResultDetails>> GetPazarYeriJobResultDetailsAsync(List<long> refIds, List<long> detailIds)
        {
            return await _repository.ToListAsync<PazarYeriJobResultDetails>(detail => refIds.Contains(detail.RefId) && detailIds.Contains(detail.DetailId));
        }
        public async Task<List<PazarYeriJobResultDetails>> GetJobResultDetailsByIdMerchAsync(
            long refId,
            string merchantId,
            string merchantNo,
            bool satisFiyatControl = false)
        {
            var query = _repository.GetTable<PazarYeriJobResultDetails>()
                .Where(x => x.PazarYeriNo == merchantNo
                            && (x.HasSent == Character.H || x.HasSent == Character.I)
                            && x.RefId == refId
                            && x.PazarYeriBirimNo == merchantId);

            if (satisFiyatControl)
                query = query.Where(x => x.SatisFiyat > 0);

            return await query
                .OrderBy(x => x.DetailId)
                .ToListAsync();
        }

        public async Task<bool> UpdatePazarYeriJobResultDetailAsync(PazarYeriJobResultDetails model)
        {
            try
            {
                await _repository.UpdateAsync(model);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task UpdatePazarYeriJobResultDetailRangeAsync(List<PazarYeriJobResultDetails> model)
        {
            var errorIds = model.Where(x => x.HasErrors == "E").Select(x => x.DetailId).ToList();
            var successIds = model.Where(x => x.HasErrors == "H").Select(x => x.DetailId).ToList();
            int threadNo = model.First().ThreadNo;
            string guid = model.First().Guid;
            var date = model.First().Tarih;

            // Define the chunk size
            const int chunkSize = 1000;

            using var context = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IDbContext>();

            try
            {
                // Process error IDs in chunks
                foreach (var errorChunk in errorIds.Chunk(chunkSize))
                {
                    var errorIdParams = string.Join(",", errorChunk);
                    await context.Database.ExecuteSqlRawAsync(
                        $"UPDATE PAZAR_YERI_JOB_RESULT_DETAILS " +
                        $"SET HAS_SENT = 'E', HAS_ERRORS = 'E', THREAD_NO = {threadNo}, GUID = '{guid}' " +
                        $"WHERE DETAIL_ID IN ({errorIdParams}) AND TARIH = TO_DATE('{date.ToString("yyyy-MM-dd")}','YYYY-MM-DD')"
                    ).ConfigureAwait(false);
                }

                // Process success IDs in chunks
                foreach (var successChunk in successIds.Chunk(chunkSize))
                {
                    var successIdParams = string.Join(",", successChunk);
                    await context.Database.ExecuteSqlRawAsync(
                        $"UPDATE PAZAR_YERI_JOB_RESULT_DETAILS " +
                        $"SET HAS_SENT = 'E', THREAD_NO = {threadNo}, GUID = '{guid}' " +
                        $"WHERE DETAIL_ID IN ({successIdParams}) AND TARIH = TO_DATE('{date.ToString("yyyy-MM-dd")}','YYYY-MM-DD')"
                    ).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        public async Task UpdatePazarYeriJobResultDetailRangeVerifiedAsync(List<PazarYeriJobResultDetails> model)
        {
            var successIds = model.Where(x => x.HasVerified == "E").Select(x => x.DetailId).ToList();
            var errorIds = model.Where(x => x.HasVerified != "E").Select(x => x.DetailId).ToList();
            string guid = model.First().Guid;
            var date = model.First().Tarih;

            using IDbContext context = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IDbContext>();

            try
            {

                if (errorIds.Count > 0)
                {
                    var errorIdParams = string.Join(",", errorIds);
                    await context.Database.ExecuteSqlRawAsync(
                        $"UPDATE PAZAR_YERI_JOB_RESULT_DETAILS " +
                        $"SET HAS_VERIFIED = 'H', HAS_ERRORS = 'E', GUID = '{guid}' " +
                        $"WHERE DETAIL_ID IN ({errorIdParams}) AND TARIH = TO_DATE('{date.ToString("yyyy-MM-dd")}','YYYY-MM-DD')"
                    ).ConfigureAwait(false);
                }


                if (successIds.Count > 0)
                {
                    var successIdParams = string.Join(",", successIds);
                    await context.Database.ExecuteSqlRawAsync(
                        $"UPDATE PAZAR_YERI_JOB_RESULT_DETAILS " +
                        $"SET HAS_VERIFIED = 'E' , GUID = '{guid}' " +
                        $"WHERE DETAIL_ID IN ({successIdParams}) AND TARIH = TO_DATE('{date.ToString("yyyy-MM-dd")}','YYYY-MM-DD')"
                    ).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                throw;
            }

        }


        public async Task<IEnumerable<PazarYeriJobResultDetails>> GetPazarYeriJobResultDetailsByRefIdAsync(long refId, int threadNo, string guid)
        {
            return await _repository.GetTable<PazarYeriJobResultDetails>()
                          .Where(detail => detail.RefId == refId && detail.ThreadNo == threadNo && detail.Guid == guid)
                          .ToListAsync();
        }
        public async Task<List<PazarYeriJobResultDetails>> GetPazarYeriJobResultDetailsAsync(long refId, int threadNo, string guid, string merchantId)
        {
            return await _repository.ToListAsync<PazarYeriJobResultDetails>(detail => detail.RefId == refId && detail.ThreadNo == threadNo && detail.Guid == guid && detail.PazarYeriBirimNo == merchantId);
        }
        public async Task<PazarYeriJobResultDetails> GetPazarYeriJobResultDetailAsync(long refId, long detailId, int threadNo, string guid, string merchantId)
        {
            var response = await _repository.ToListAsync<PazarYeriJobResultDetails>(detail => detail.RefId == refId && detail.DetailId == detailId && detail.ThreadNo == threadNo && detail.Guid == guid && detail.PazarYeriBirimNo == merchantId);
            return response.FirstOrDefault();
        }
        #endregion

        #region PazarYeriLog
        public async Task<bool> AddRangePazarYeriLogAsync(List<PazarYeriLog> models)
        {
            try
            {
                await _repository.AddRangeAsync(models);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> InsertPazarYeriLogAsync(PazarYeriLog model)
        {
            try
            {
                await _repository.AddAsync(model);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> UpdateServiceLog(long refId, string parDurum, string paraHataMesaji)
        {
            return await _repository.ExecuteStoredProcedureAsync(_appSettings.Value.RawDatabaseQueries.PPazarYeriLogGuncelleQuery, new List<OracleParameter> {
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_REF_ID",
                    Value = refId
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_DURUM",
                    Value = parDurum
                },
                        new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "PAR_HATA_MESAJI",
                    Value = paraHataMesaji
                }}.ToArray());
        }
    }
    #endregion
}