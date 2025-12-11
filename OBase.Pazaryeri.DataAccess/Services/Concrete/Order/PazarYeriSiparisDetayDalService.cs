using Microsoft.EntityFrameworkCore;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Generic;
using OBase.Pazaryeri.Domain.Entities;
using Oracle.ManagedDataAccess.Client;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using System.Data;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Domain.ConfigurationOptions;

namespace OBase.Pazaryeri.DataAccess.Services.Concrete.Order
{
    public class PazarYeriSiparisDetayDalService : BaseDalService, IPazarYeriSiparisDetayDalService
    {
        private readonly IOptions<AppSettings> _appSettings;
        public PazarYeriSiparisDetayDalService(IRepository repository, IOptions<AppSettings> appSettings) : base(repository)
        {
            _appSettings = appSettings;
        }
        public async Task<PazarYeriSiparisDetay> GetOrderDetailByIdAsync(long orderId)
        {
            return await _repository.GetTable<PazarYeriSiparisDetay>().FirstOrDefaultAsync(x => x.Id == orderId);
        }
        public async Task<IEnumerable<PazarYeriSiparisDetay>> GetOrderDetailsdByIdAsync(long orderId)
        {
            return await _repository.GetTable<PazarYeriSiparisDetay>().Where(x => x.Id == orderId).Select(s => new PazarYeriSiparisDetay()
            {
                Miktar = s.Miktar,
                NetTutar = s.NetTutar,
                Barkod = s.Barkod,
                IndirimTutar = s.IndirimTutar,
                LineItemId = s.LineItemId,
                ObaseMalNo = s.ObaseMalNo,
                PazarYeriMalNo = s.PazarYeriMalNo,
                PaketItemId = s.PaketItemId
            }).ToListAsync();
        }
        public async Task<IEnumerable<PazarYeriSiparisDetay>> GetOrderDetailsAsync(long orderId, List<string> packageIdList = null)
        {
            return await _repository.GetTable<PazarYeriSiparisDetay>().Where(x => x.Id == orderId && (packageIdList == null || (packageIdList != null && packageIdList.Contains(x.PaketItemId)))).ToListAsync();
        }
        public async Task<IEnumerable<PazarYeriSiparisDetay>> GetOrderDetailsByOrderIdProductNoAsync(long orderId, List<string> productNoList = null)
        {
            return await _repository.GetTable<PazarYeriSiparisDetay>().Where(x => x.Id == orderId && (productNoList != null && productNoList.Contains(x.ObaseMalNo)))
                .Select(s => new PazarYeriSiparisDetay()
                {
                    ObaseMalNo = s.ObaseMalNo,
                    Miktar = s.Miktar,
                    LineItemId = s.LineItemId,
                }).ToListAsync();
        }
        public async Task<PazarYeriSiparisDetay> GetOrderDetailByLineItemIdAsync(string lineItemId)
        {
            return await _repository.GetTable<PazarYeriSiparisDetay>().FirstOrDefaultAsync(x => x.LineItemId == lineItemId);
        }
        public async Task<IEnumerable<PazarYeriSiparisDetay>> GetOrderDetailsByLineItemIdsAsync(long orderId, List<string> lineItemIdList = null)
        {
            return await _repository.GetTable<PazarYeriSiparisDetay>().Where(x => x.Id == orderId && (lineItemIdList == null || (lineItemIdList != null && lineItemIdList.Contains(x.LineItemId)))).ToListAsync();
        }
        public bool IsItemCancelled(string qpProductId, long orderId)
        {
            var result = _repository.ExecuteSqlCommand<QueryResult>(_appSettings.Value.RawDatabaseQueries.IsItemCancelledQuery, new List<OracleParameter> {
                            new OracleParameter
                            {
                                OracleDbType = OracleDbType.Varchar2,
                                Direction = ParameterDirection.Input,
                                ParameterName = Db.RawQuery.IsItemCancelledQuery.Parameters.QPProductId,
                                Value = qpProductId
                            },
                            new OracleParameter
                            {
                                OracleDbType = OracleDbType.Varchar2,
                                Direction = ParameterDirection.Input,
                                ParameterName = Db.RawQuery.IsItemCancelledQuery.Parameters.OrderId,
                                Value = orderId
                            }}.ToArray()).FirstOrDefault();
            return result.Count > 0;
        }
        public bool IsTheOrderToBeCancelled(long hbOrderId)
        {
            var result = _repository.ExecuteSqlCommand<OrderCancellationStatus>(_appSettings.Value.RawDatabaseQueries.IsTheOrderToBeCancelled).FirstOrDefault().IsCancelled;
            return result == 1;

        }
        public async Task AddOrderDetailsAsync(List<PazarYeriSiparisDetay> details)
        {
            await _repository.AddRangeAsync(details);
        }
        public async Task UpdateOrderDetailAsync(PazarYeriSiparisDetay model)
        {
            await _repository.UpdateAsync(model);
        }
        public async Task UpdateOrderDetailRangeAsync(IEnumerable<PazarYeriSiparisDetay> models)
        {
            await _repository.UpdateRangeAsync(models);
        }
        public decimal GetQuantityByLineItemId(string lineItemId)
        {
            return _repository.GetTable<PazarYeriSiparisDetay>()
                            .Where(x => x.LineItemId == lineItemId.ToString())
                            .Select(x => x.Miktar)
                            .FirstOrDefault();
        }
    }
    public class QueryResult : IEntity
    {
        public int Count { get; set; }
    }
}
