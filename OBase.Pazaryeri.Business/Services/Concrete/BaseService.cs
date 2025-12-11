using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Services.Concrete
{
    public class BaseService : IBaseService
    {
        private readonly IOptions<AppSettings> _appSetting;
        protected readonly IMailService _mailService;
        public readonly IPazarYeriSiparisDalService _pazarYeriSiparisDalService;
        public readonly IPazarYeriSiparisDetayDalService _pazarYeriSiparisDetayDalService;
        public BaseService(IPazarYeriSiparisDalService pazarYeriSiparisDalService, IPazarYeriSiparisDetayDalService pazarYeriSiparisDetayDalService, IOptions<AppSettings> appSetting, IMailService mailService)
        {
            _pazarYeriSiparisDalService = pazarYeriSiparisDalService;
            _pazarYeriSiparisDetayDalService = pazarYeriSiparisDetayDalService;
            _appSetting = appSetting;
            _mailService = mailService;
        }

        public IEnumerable<PazarYeriJobResultDetails> CalculateThreads(int totalCnt, int threadSize, List<PazarYeriJobResultDetails> details, long RefId, string MerchantId, int ThreadId)
        {
            {
                var logObject = new ThreadLog
                {
                    RefId = RefId,
                    MerchantId = MerchantId,
                    ThreadId = ThreadId,
                    TotalCount = totalCnt,
                    ThreadSize = threadSize,
                    ThreadCount = 0,
                    StartedDateTime = DateTime.Now
                };

                int threadCnt = 1;

                for (int i = 0; i < totalCnt; i++)
                {
                    details[i].ThreadNo = threadCnt;

                    if (((i + 1) % threadSize) == 0)
                    {
                        threadCnt++;
                    }
                }

                logObject.ThreadCount = details.GroupBy(g => g.ThreadNo).Count();
                logObject.CompletionDateTime = DateTime.Now;

                return details;
            }
        }
        public async Task UpdateOrderWithOrderDetails(string status, bool isSuccess, string? reasonId, long orderId, string content, List<string>? packageIdList = null, List<string>? lineItemIdList = null)
        {
            string IsCancelledEH = status == nameof(CommonEnums.StatusEnums.UnSupplied) || status == nameof(CommonEnums.StatusEnums.Cancelled) || status == nameof(CommonEnums.StatusEnums.CancelledPart) ? Character.E : Character.H;
            string HasSent = !isSuccess ? Character.H : Character.E;
            string Hata = !isSuccess ? content : string.Empty;

            var order = await _pazarYeriSiparisDalService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                order.SevkiyatPaketDurumu = isSuccess ? status : order.SevkiyatPaketDurumu;
                order.HasSent = HasSent;
                order.Hata = Hata;
                await _pazarYeriSiparisDalService.UpdateAsync(order);
            }
            IEnumerable<PazarYeriSiparisDetay>? orderDetails = null;
            if (packageIdList is not null)
            {
                orderDetails = await _pazarYeriSiparisDetayDalService.GetOrderDetailsAsync(orderId, packageIdList);
            }
            if (lineItemIdList is not null)
            {
                orderDetails = await _pazarYeriSiparisDetayDalService.GetOrderDetailsByLineItemIdsAsync(orderId, lineItemIdList);
            }
            if (orderDetails is not null)
            {
                foreach (var item in orderDetails)
                {
                    item.ReasonId = reasonId ?? item.ReasonId;
                    item.HasSent = HasSent;
                    item.IsCancelledEH = IsCancelledEH;
                    item.Hata = Hata;
                }
                await _pazarYeriSiparisDetayDalService.UpdateOrderDetailRangeAsync(orderDetails);
            }
        }
        public async Task SendFailedOrderMail(string subject, string body)
        {
            if (_appSetting.Value.MailSettings?.MailEnabled ?? false)
            {
                await _pazarYeriSiparisDalService.InsertEmailHareketAsync(subject, body);
            }
        }
    }
}