using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Promotion;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Promotion
{
    public class PromotionDalService : BaseDalService, IPromotionDalService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptions<AppSettings> _appSettings;
        public PromotionDalService(IRepository repository, IServiceScopeFactory serviceScopeFactory, IOptions<AppSettings> appSettings) : base(repository)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _appSettings = appSettings;
        }
        public async Task<bool> ExecutePromotionProcedureAsync(long promotionNo, long id)
        {
            try
            {
                var query = _appSettings.Value.RawDatabaseQueries.ProcPYPromotionEntegrasyon;
                var parameters = new List<OracleParameter> {
                    new OracleParameter
                    {
                        OracleDbType = OracleDbType.Int64,
                        Direction = ParameterDirection.Input,
                        ParameterName = "PAR_PROMOSYON_NO",
                        Value = promotionNo
                    },
                    new OracleParameter
                    {
                        OracleDbType = OracleDbType.Int64,
                        Direction = ParameterDirection.Input,
                        ParameterName = "PAR_ID",
                        Value = id
                    }}.ToArray();
                await _repository.ExecuteStoredProcedureAsync(query, parameters);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PyPromosyonEntegrasyon>> GetPendingPromotionsAsync()
        {
            var query = _appSettings.Value.RawDatabaseQueries.PazarYeriPromotionEntegrasyonListQuery;
            return await Task.FromResult(_repository.ExecuteSqlCommand<PyPromosyonEntegrasyon>(query).ToList());
        }

        public async Task<(TmpPyPromosyonTanim? Tanim, List<TmpPyPromosyonDetay>? Detaylar, List<TmpPyPromosyonBirim>? Birimler)> GetTmpPromotionDataAsync(long promotionId, long promotionNo, string merchantNo)
        {
            try
            {

                var tmpPyPromosyonTanimQuery = _appSettings.Value.RawDatabaseQueries.PazarYeriPromotionEntegrasyonTmpPromosyonTanimQuery;

                var tmpPyPromosyonTanimDetayQuery = _appSettings.Value.RawDatabaseQueries.PazarYeriPromotionEntegrasyonTmpPromosyonTanimDetayQuery;

                var tmpPyPromosyonBirimQuery = _appSettings.Value.RawDatabaseQueries.PazarYeriPromotionEntegrasyonTmpPromosyonBirimQuery;



                var queryParameters = new List<OracleParameter> {
                                    new OracleParameter
                                    {
                                        OracleDbType = OracleDbType.Int64,
                                        Direction = ParameterDirection.Input,
                                        ParameterName = "PRM_NO",
                                        Value = promotionNo
                                    },
                                    new OracleParameter
                                    {
                                        OracleDbType = OracleDbType.Int64,
                                        Direction = ParameterDirection.Input,
                                        ParameterName = "PAR_ID",
                                        Value = promotionId
                                    }}.ToArray();


                var tmpPyPromosyonTanim = await Task.FromResult(_repository.ExecuteSqlCommand<TmpPyPromosyonTanim>(tmpPyPromosyonTanimQuery, queryParameters).ToList().FirstOrDefault());

                if (tmpPyPromosyonTanim == null)
                    return (null,
                            new List<TmpPyPromosyonDetay>(),
                            null);

                var tmpPyPromosyonTanimDetaylar = await Task.FromResult(_repository.ExecuteSqlCommand<TmpPyPromosyonDetay>(tmpPyPromosyonTanimDetayQuery, queryParameters).ToList());


                List<TmpPyPromosyonBirim>? tmpPyPromosyonBirimler = null;

                if (tmpPyPromosyonTanim.TumBirimlerEh != "E")
                {

                    tmpPyPromosyonBirimler = await Task.FromResult(_repository.ExecuteSqlCommand<TmpPyPromosyonBirim>(tmpPyPromosyonBirimQuery, queryParameters).ToList());
                }

                return (tmpPyPromosyonTanim, tmpPyPromosyonTanimDetaylar, tmpPyPromosyonBirimler);



            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> MarkPromotionAsSentAsync(long promotionNo, string pazaryeriPromoNo)
        {
            try
            {
                //var sql = @"BEGIN UPDATE OBASE.PY_PROMOSYON_ENTEGRASYON  SET GONDERILDI_EH = 'E'  WHERE PROMOSYON_NO = :PRM;  UPDATE OBASE.PY_PROMOSYON_TANIM SET PY_PROMOSYON_NO = :PROMO WHERE PROMOSYON_NO = :PRM;  END;";

                var tmpPyPromosyonBirimQuery = _appSettings.Value.RawDatabaseQueries.PazarYeriPromotionEntegrasyonUpdatePromotionStatusQuery;
                var parameters = new List<OracleParameter> {
                    new OracleParameter
                    {
                        OracleDbType = OracleDbType.Int64,
                        Direction = ParameterDirection.Input,
                        ParameterName = "PRM",
                        Value = promotionNo
                    },
                    new OracleParameter
                    {
                        OracleDbType = OracleDbType.Varchar2,
                        Direction = ParameterDirection.Input,
                        ParameterName = "PROMO",
                        Value = pazaryeriPromoNo
                    }}.ToArray();

                await _repository.ExecuteSqlRawAsync(tmpPyPromosyonBirimQuery, parameters);

                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task UpdatePromotionErrorAsync(long promotionNo, string errorMessage)
        {
            try
            {
                var record = await _repository.GetTable<PyPromosyonEntegrasyon>().Where(x => x.PromosyonNo == promotionNo).FirstOrDefaultAsync();
                if (record == null) return;

                record.TryCount += 1;
                record.HataMesaji = errorMessage;

                await _repository.UpdateAsync(record);
            }
            catch (Exception)
            {
                
            }
        }
    }
}
