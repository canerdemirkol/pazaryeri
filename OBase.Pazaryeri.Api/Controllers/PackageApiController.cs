using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OBase.Pazaryeri.Api.Attributes;
using OBase.Pazaryeri.Business.Helper;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Helper.CommonHelper;

namespace OBase.Pazaryeri.Api.Controllers
{
	[Authorize]
	[Route("api/v1/package")]
	[ApiController]
    [SwaggerEndpointGroup("Common")]
    public class PackageApiController : BaseController
	{
		private readonly IPazarYeriSiparisDalService _pazarYeriSiparisDalService;
		private readonly IMerchantFactory _merchantFactory;

		public PackageApiController(IPazarYeriSiparisDalService pazarYeriSiparisDalService, IMerchantFactory merchantFactory)
		{
			_pazarYeriSiparisDalService = pazarYeriSiparisDalService;
			_merchantFactory = merchantFactory;
		}

		[Route("ReturnOrderClaimStatus")]
		[HttpPut]
		public async Task<IActionResult> AcceptOrRejectReturnClaim([FromBody] PostProductReturnRequestDto dto)
		{
			try
			{
				Logger.Information("PackageApi => AcceptOrRejectReturnClaim => Request: {@request}", fileName: CommonConstants.GeneralLogFile, dto);

				var orderModel = await _pazarYeriSiparisDalService.GetOrderByIdWithDetailsAsync(dto.OrderId);
				if (orderModel is null)
				{
					CommonResponseDto commonRespDto = new() { Message = "Sipariş Bulunamadı", Success = false };
					Logger.Warning("Response : {@response} ", fileName: CommonConstants.GeneralLogFile, commonRespDto);
					return Ok(commonRespDto);
				}

				IOrderService merchant = _merchantFactory.GetMerchantOrderService(orderModel.PazarYeriNo);
				var respDto = await merchant.ClaimStatuUpdate(dto);
				Logger.Information("PackageApi => AcceptOrRejectReturnClaim => Response: {@response}", fileName: CommonConstants.GeneralLogFile, respDto);

				return Ok(respDto);
			}
			catch (Exception e)
			{
				Logger.Error("PackageApi => AcceptOrRejectReturnClaim => Error: {exception} Request: {@request}", fileName: CommonConstants.GeneralLogFile, e, dto);
                string errorMessage = e.Message + e.InnerException?.Message;
                return base.Ok(new CommonResponseDto() { Message = errorMessage, Success = false });
			}
		}

		[Route("package-update-status")]
		[HttpPut]
		public async Task<IActionResult> PackageUpdateStatus([FromBody] OrderStatuUpdateRequestDto dto)
		{
			try
			{
				Logger.Information("PackageApi => PackageUpdateStatus => {@request}", fileName: CommonConstants.GeneralLogFile, dto);

				var orderModel = await _pazarYeriSiparisDalService.GetOrderByIdWithDetailsAsync(dto.OrderId);
				if (orderModel is null)
				{
					CommonResponseDto commonRespDto = new() { Message = "Sipariş Bulunamadı", Success = false };
					Logger.Warning("Response : {@response} ", fileName: CommonConstants.GeneralLogFile, commonRespDto);
					return Ok(commonRespDto);
				}

				dto.PackageId = orderModel.PaketId;
				dto.SiparisNo = orderModel.SiparisNo;
				dto.MerchantId = orderModel.PazarYeriSiparisDetails[0].PazarYeriBirimId;
				IOrderService merchant = _merchantFactory.GetMerchantOrderService(orderModel.PazarYeriNo);
				CommonResponseDto commonRespDTO = await merchant.OrderUpdatePackageStatus(dto, orderModel);
				var message = commonRespDTO.Message;
				var resultStatus = commonRespDTO.StatusCode;

				bool success = CalcSuccessFromHTTPStatus(resultStatus);

				var returnMessage = message ?? (success ? "İşlem Başarılı" : "İşlem Başarısız");
				var respDto = new CommonResponseDto() { StatusCode = resultStatus, Success = success, Message = returnMessage };
				Logger.Information("Statu Update Response : {@response}", fileName: CommonConstants.GeneralLogFile, respDto);
				return Ok(respDto);
			}
			catch (Exception ex)
			{
				Logger.Error("package-update-status > Error {exception}", fileName: CommonConstants.GeneralLogFile, ex);
				return Ok(new CommonResponseDto() { Message = $"Pazaryeri | Sistem Hatası: {ex.Message}", Success = false });
			}
		}
	}
}