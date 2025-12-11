using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OBase.Pazaryeri.Api.Attributes;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Api.Controllers
{
    [Route("api/v1/hepsiExpress")]
    [ApiController]
	[Authorize]
    [SwaggerEndpointGroup("HepsiExpress")]
    public class HepsiExpressApiController : BaseController
    {

        #region Private
        private readonly IHepsiExpressOrderService _hepsiExpressOrderService;
        private string logFolderName = nameof(PazarYerleri.HepsiExpress);

        #endregion

        #region Const
        public HepsiExpressApiController(IHepsiExpressOrderService hepsiExpressOrderService)
        {
            _hepsiExpressOrderService = hepsiExpressOrderService;
        }
        #endregion

        [Route("orders")]
        [HttpPost]
        public async Task<IActionResult> CreateOrderFromWebHook([FromBody] HEOrderDto order)
        {
            CommonResponseDto commonRespDto;
            Logger.Information("HepsiExpressApiController > CreateOrderFromWebHook Request: {@request}", fileName: logFolderName, order);
            if (order == null)
            {
                commonRespDto = new CommonResponseDto() { Message = "Gönderilen sipariş nesnesi boş olamaz.", Success = false };
                return BadRequest(commonRespDto);
            }

            try
            {
                await _hepsiExpressOrderService.SaveOrderOnQp(order);
                commonRespDto = new CommonResponseDto { Message = "Sipariş başarılı bir şekilde kaydedildi.", Success = true };
                Logger.Information("HepsiExpressApiController > CreateOrderFromWebHook Response : {@response} ", fileName: logFolderName, commonRespDto);
            }
            catch (Exception ex)
            {
                commonRespDto = new CommonResponseDto() { Message = "Siparişler kaydedilirken bir hata oluştu.", Success = false };
                Logger.Error("HepsiExpressApiController > CreateOrderFromWebHook Hata {exception} ", fileName: logFolderName, ex);
            }
            return Ok(commonRespDto);
        }

        [Route("lineitems/{lineitemId}/cancel")]
        [HttpPut]
        public async Task<IActionResult> EndUserCancelation(string lineItemId, [FromBody] HEEndUserCancellationModel hepsiExpressEndUserCancellationModel)
        {
            CommonResponseDto commonRespDto;
            Logger.Information("Cancelled {cancelLineItemId} Request : {@request}", fileName: logFolderName, lineItemId, hepsiExpressEndUserCancellationModel);

            try
            {
                await _hepsiExpressOrderService.EndUserCancellation(lineItemId, hepsiExpressEndUserCancellationModel);
                commonRespDto = new CommonResponseDto { Message = "Sipariş iptal edildi.", Success = true };

            }
            catch (Exception ex)
            {
                commonRespDto = new CommonResponseDto() { Message = "Sipariş iptal edilirken bir hata oluştu.", Success = false };
                Logger.Error("HepsiExpressApiController > lineitems/{lineitemId}/cancel Hata {exception} ", fileName: logFolderName, ex);
            }
            return NoContent();
        }
    }
}