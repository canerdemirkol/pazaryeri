using Microsoft.AspNetCore.Mvc;
using OBase.Pazaryeri.Api.Attributes;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Api.Controllers
{


    [Route("api/v1/trendyolgo")]
    [ApiController]
    [SwaggerEndpointGroup("TrendyolGo")]
    [JwtAuthorize(ValidateUser = true, RequiredService = "TrendyolGo", RequiredRoles = new[] { "Admin" })]
    public class TrendyolGoController : BaseController
    {
        private readonly ITrendyolGoOrderService _tyGoOrderService;
        public TrendyolGoController(ITrendyolGoOrderService tyGoOrderService)
        {
            _tyGoOrderService = tyGoOrderService;
        }
        [Route("create-order")]
        [HttpPost]
        public async Task<IActionResult> CreateOrderFromWebHook([FromBody] TrendyolGoOrderDto order)
        {
            return ResponseResult(await _tyGoOrderService.SaveTyGoCreatedOrderAsync(order));
        }
    }
}