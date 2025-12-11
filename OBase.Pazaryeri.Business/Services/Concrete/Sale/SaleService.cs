using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Helper;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Business.Services.Abstract.Sale;
using OBase.Pazaryeri.Business.Services.Concrete.General;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Abstract.QuickPick;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Sale;
using OBase.Pazaryeri.DataAccess.Services.Concrete.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.Sale;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using OBase.Pazaryeri.Domain.Entities;
using Polly;
using Polly.Retry;
using System.Data.SqlClient;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using static OBase.Pazaryeri.Domain.Helper.CommonHelper;
using  OBase.Pazaryeri.Domain.Extensions;

namespace OBase.Pazaryeri.Business.Services.Concrete.Sale
{
    public class SaleService : ISaleService
    {
        #region Variables
        private readonly ITransactionDalService _transactionDalService;
        private readonly ApiDefinitions _apiDefinition;
        private readonly IMailService _mailService;
        private readonly IGetDalService _getDalService;
        private readonly IOrderConvertService _orderConvertService;
        private readonly string _logFolderName = "SaleInfo";
        private readonly ISaleDalService _saleDalService;
        private readonly IAkilliETicaretClient _akilliETicaretClient;
        #endregion

        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mailService"></param>
        /// <param name="options"></param>
        /// <param name="getDalService"></param>
        /// <param name="orderConvertService"></param>
        /// <param name="transactionDalService"></param>
        public SaleService(
            IMailService mailService,
            IGetDalService getDalService,
            ISaleDalService saleDalService,
            IOrderConvertService orderConvertService,
            ITransactionDalService transactionDalService,
            IAkilliETicaretClient akilliETicaretClient)
        {
            _mailService = mailService;
            _getDalService = getDalService;
            _saleDalService = saleDalService;
            _orderConvertService = orderConvertService;
            _transactionDalService = transactionDalService;
            _akilliETicaretClient = akilliETicaretClient;
        }

        #endregion

        #region Methods
        public async Task<ServiceResponse<SaleInfoResponseDto>> SaveSaleInfo(SaleInfoDto saleInfoDto)
        {
            if (saleInfoDto is null)
            {
                await SendFailedOrderMailFormattedAsync("Satış Bilgisi Kaydedilirken Hata", $"The submitted order object cannot be empty!", saleInfoDto);
                return ServiceResponse<SaleInfoResponseDto>.Error("The submitted order object cannot be empty!");
            }
            if (!saleInfoDto.Items?.Any() ?? true)
            {
                await SendFailedOrderMailFormattedAsync("Satış Bilgisi Kaydedilirken Hata", $"The product list cannot be empty!", saleInfoDto);
                return ServiceResponse<SaleInfoResponseDto>.Error("The product list cannot be empty!"); 
            }
            try
            {
              
                Logger.Information("SaleService SaveSaleInfo Request : {@request} ", fileName: _logFolderName, saleInfoDto);              
                // SaleInfoDto'dan CashReceiptDto oluşturma
                var satisNoSeqId = await _saleDalService.GetSeqId();
                var cashReceipt = saleInfoDto.ToCashReceiptDto(satisNoSeqId);
                
                // Procedure'ü çağırma
                await _saleDalService.InsertCashReceiptAsync(cashReceipt);
                
                // Her item için detay kaydı oluşturma
                foreach (var item in saleInfoDto.Items)
                {
                    var cashReceiptDetail = item.ToCashReceiptDetailDto(satisNoSeqId, saleInfoDto.SaleDateUtc);
                    await _saleDalService.InsertCashReceiptDetailAsync(cashReceiptDetail);
                }
                
                // Her payment için ödeme detay kaydı oluşturma
                foreach (var payment in saleInfoDto.Payments)
                {
                    var cashReceiptPaymentDetail = payment.ToCashReceiptPaymentDetailDto(satisNoSeqId);
                    await _saleDalService.InsertCashReceiptPaymentDetailAsync(cashReceiptPaymentDetail);
                }
                
                // Her discount için indirim detay kaydı oluşturma
                foreach (var discount in saleInfoDto.Discounts)
                {
                    var cashReceiptDiscountDetail = discount.ToCashReceiptDiscountDetailDto(satisNoSeqId);
                    await _saleDalService.InsertCashReceiptDiscountDetailAsync(cashReceiptDiscountDetail);
                }

                return ServiceResponse<SaleInfoResponseDto>.Success(data: new SaleInfoResponseDto { Message = "Sale Info Saved", Success = true, SaleNo = satisNoSeqId.ToString() });               
            }
            catch (Exception ex)
            {
                await SendFailedOrderMailFormattedAsync("Satış Bilgisi Kaydedilirken Hata", $"Unhandled Exception!", saleInfoDto, ex);
                Logger.Error("SaleService >  Hata {exception} ", fileName: _logFolderName, ex);
                return ServiceResponse<SaleInfoResponseDto>.Error(ex.Message, httpStatusCode: HttpStatusCode.InternalServerError);
            }
        }

        #endregion

        #region Utilities
        private async Task SendFailedOrderMailFormattedAsync(string subject, string message, SaleInfoDto? orderDto = null, Exception? ex = null)
        {
            try
            {
                string body = "<table>";

                body += $"<tr><td>Tarih:</td><td>{DateTime.Now}</td></tr>";

                if (orderDto is not null)
                {
                    body += $"<tr><td>Order Id:</td><td>{orderDto.OrderId}</td></tr>";
                    body += $"<tr><td>Order Code:</td><td>{orderDto.OrderCode}</td></tr>";
                    body += $"<tr><td>External Order Id:</td><td>{orderDto.ExternalOrderId}</td></tr>";
                }

                body += $"<tr><td>Hata:</td><td>{message}</td></tr>";

                if (ex is not null)
                {
                    body += $"<tr><td>Exception Message:</td><td>{ex.Message}</td></tr>";
                    body += $"<tr><td>Inner Exception:</td><td>{ex.InnerException?.Message}</td></tr>";
                    body += $"<tr><td>Stack Trace:</td><td>{ex.StackTrace}</td></tr>";
                }

                body += "</table>";

                //await SendFailedOrderMail(subject, body);
            }
            catch (Exception exc)
            {
                Logger.Error("An exception occurred while sending failed order email: {exception}", fileName: _logFolderName, exc);
            }
        }

        #endregion


    }
}