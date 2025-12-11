using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Dtos.Sale;
using OBase.Pazaryeri.Domain.Entities;

namespace OBase.Pazaryeri.DataAccess.Services.Abstract.Sale
{
    public interface ISaleDalService : IBaseDalService
    {
        Task<long> GetSeqId();
        Task InsertCashReceiptAsync(CashReceiptDto cashReceipt);
        Task InsertCashReceiptDetailAsync(CashReceiptDetailDto cashReceiptDetail);
        Task InsertCashReceiptPaymentDetailAsync(CashReceiptPaymentDetailDto cashReceiptPaymentDetail);
        Task InsertCashReceiptDiscountDetailAsync(CashReceiptDiscountDetailDto cashReceiptDiscountDetail);
        Task InsertEmailHareketAsync(string subject, string body);
    }
}