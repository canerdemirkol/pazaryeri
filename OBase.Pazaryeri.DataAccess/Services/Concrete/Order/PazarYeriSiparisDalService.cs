using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Entities;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Order
{
    public class PazarYeriSiparisDalService : BaseDalService, IPazarYeriSiparisDalService
    {
        private readonly IOptions<AppSettings> _appSettings;
        public PazarYeriSiparisDalService(IRepository repository, IOptions<AppSettings> appSettings) : base(repository)
        {
            _appSettings = appSettings;
        }
        public async Task<PazarYeriSiparis> GetOrderByIdAsync(long orderId)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>()
                .FirstOrDefaultAsync(x => x.Id == orderId);
            return result;
        }
        public async Task<PazarYeriSiparis> GetOrderByIdWithDetailsAsync(long orderId)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>().Include(x => x.PazarYeriSiparisDetails)
                .FirstOrDefaultAsync(x => x.Id == orderId);
            return result;
        }
        public async Task<PazarYeriSiparis> GetOrderByPackageIdAsync(string orderPackageId)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>()
                .FirstOrDefaultAsync(x => x.PaketId == orderPackageId);
            return result;
        }
        public async Task<PazarYeriSiparis> GetOrderByOrderIdAsync(string orderId)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>()
                .FirstOrDefaultAsync(x => x.SiparisId == orderId);
            return result;
        }
        public async Task<IEnumerable<PazarYeriSiparis>> GetOrderInformationsByIdAsync(long orderId)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>().Include(s => s.PazarYeriSiparisDetails)
                .Select(s => new PazarYeriSiparis()
                {
                    PaketId = s.PaketId,
                    PazarYeriNo = s.PazarYeriNo,
                    SiparisNo = s.SiparisNo
                })
                .Where(x => x.Id == orderId).ToListAsync();
            return result;
        }
        public async Task<PazarYeriSiparis> GetOrderWithOrderIdAsync(string orderId, string merchantNo)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>().FirstOrDefaultAsync(x => x.SiparisId == orderId && x.PazarYeriNo == merchantNo);

            return result;
        }
        public async Task<PazarYeriSiparis> GetOldestOrderWithOrderIdAsync(string orderId, string merchantNo)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>()
                                        .Where(x => x.SiparisId == orderId && x.PazarYeriNo == merchantNo)
                                        .OrderBy(x => x.Id).FirstOrDefaultAsync();
            return result;
        }
        public async Task AddOrderAsync(PazarYeriSiparis order)
        {
            await _repository.AddAsync(order);
        }
        public async Task UpdateOrderAsync(PazarYeriSiparis model)
        {
            await _repository.UpdateAsync(model);
        }
        public async Task<bool> OrderExistAsync(string orderPackageId, string merchantNo)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>()
                .AnyAsync(x => x.PaketId == orderPackageId && x.PazarYeriNo == merchantNo);
            return result;
        }
        public async Task<bool> OrderExistByIdAsync(string orderId, string merchantNo)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>()
                .AnyAsync(x => x.SiparisId == orderId && x.PazarYeriNo == merchantNo);
            return result;
        }
        public async Task<long> GetOrderIdByIdAsync(string orderPackageId, string merchantNo)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>()
                .Where(x => x.PaketId == orderPackageId && x.PazarYeriNo == merchantNo).Select(s => s.Id).FirstOrDefaultAsync();
            return result;
        }
        public async Task<long> GetOrderWareHouseTransferredCountAsync(string orderNo, string merchantNo)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>()
                .Where(x => x.SiparisNo == orderNo && x.PazarYeriNo == merchantNo && x.DepoAktarildiEH == Character.E).Select(s => s.Id).CountAsync();
            return result;
        }
        public async Task<long> GetOrdersCountWithSameOrderIdAsync(string orderNo, string merchantNo)
        {
            var result = await _repository.GetTable<PazarYeriSiparis>()
                .Where(x => x.SiparisNo == orderNo && x.PazarYeriNo == merchantNo).Select(s => s.Id).CountAsync();
            return result;
        }
        public async Task<PazarYeriSiparis> GetCreatedOrderByOrderNumberAsync(string orderNo, string merchantNo, string sevkiyatPaketDurumu = "Created")
        {
            return await _repository.GetTable<PazarYeriSiparis>()
                .Where(x => x.SiparisNo == orderNo && x.PazarYeriNo == merchantNo && x.SevkiyatPaketDurumu == sevkiyatPaketDurumu).FirstOrDefaultAsync();

        }
        public async Task<long> GetSeqId()
        {
            return await Task.FromResult(_repository.ExecuteSqlCommand<SeqIdView>(_appSettings.Value.RawDatabaseQueries.SeqQuery).FirstOrDefault().SeqId);
        }

        public async Task InsertEmailHareketAsync(string subject, string body)
        {
            var query = _appSettings.Value.RawDatabaseQueries.EmailHareketInsertQuery;

            var parameters = new List<OracleParameter> {
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Clob,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_BODY",
                    Value = body
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_KONU",
                    Value = subject
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_CC",
                    Value = _appSettings.Value.MailSettings.CC ?? ""
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_NEREDEN",
                    Value = _appSettings.Value.MailSettings.From ?? ""
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_NEREYE",
                    Value = _appSettings.Value.MailSettings.To ?? ""
                },
                new OracleParameter
                {
                    OracleDbType = OracleDbType.Varchar2,
                    Direction = ParameterDirection.Input,
                    ParameterName = "EMAIL_TIP",
                    Value = "01"
                }}.ToArray();

            await _repository.ExecuteSqlRawAsync(query, parameters);

        }

        public async Task UpdateOrderStatusAsync(string orderId, string status, string merchantNo)
        {
            var query = _appSettings.Value.RawDatabaseQueries.UpdatePazarYeriSiparisStatusQuery;

            var parameters = new List<OracleParameter> {
            new OracleParameter
            {
                OracleDbType = OracleDbType.Varchar2,
                Direction = ParameterDirection.Input,
                ParameterName = "SIPARIS_ID",
                Value = orderId
            },
            new OracleParameter
            {
                OracleDbType = OracleDbType.Varchar2,
                Direction = ParameterDirection.Input,
                ParameterName = "SEVKIYAT_PAKET_DURUMU",
                Value = status
            },
            new OracleParameter
            {
                OracleDbType = OracleDbType.Varchar2,
                Direction = ParameterDirection.Input,
                ParameterName = "PAZAR_YERI_NO",
                Value = merchantNo
            }
            }.ToArray();

            await _repository.ExecuteSqlRawAsync(query, parameters);
        }
    }
}