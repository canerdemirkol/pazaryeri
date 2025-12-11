#region General
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static OBase.Pazaryeri.Domain.Constants.Constants;
#endregion

#region Project
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using System.Net;
using OBase.Pazaryeri.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Concrete.Order;
using OBase.Pazaryeri.Domain.Dtos.Getir.Login;
using OBase.Pazaryeri.Api.Attributes;

#endregion

namespace OBase.Pazaryeri.Api.Controllers
{
    [Route("api/v1/getir")]
    [ApiController]
    [SwaggerEndpointGroup("GetirCarsi")]
    public class GetirCarsiApiController : BaseController
    {
        private readonly IGetirCarsiOrderService _getirOrderService;

        private readonly IGetirCarsiLoginService _getirCarsiLoginService;
        public GetirCarsiApiController(IGetirCarsiOrderService getirOrderService, IGetirCarsiLoginService getirCarsiLoginService)
        {
            _getirOrderService = getirOrderService;
            _getirCarsiLoginService = getirCarsiLoginService;
        }
		[Route("create-order")]
		[HttpPost]
        public async Task<IActionResult> CreateOrderFromWebHook([FromHeader(Name = CommonConstants.XApiKey)] string xApiKey, [FromBody] GetirOrderDto order)
        {
            return ResponseResult(await _getirOrderService.SaveOrderOnQp(order));
        }

        [Route("reset-password")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] NewPasswordDto newPasswordDto)
        {
            return ResponseResult(await _getirCarsiLoginService.ResetPassword(newPasswordDto.newPassword));
        }

    }
}