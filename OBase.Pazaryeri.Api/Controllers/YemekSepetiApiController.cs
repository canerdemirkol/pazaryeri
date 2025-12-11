using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OBase.Pazaryeri.Api.Attributes;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;

namespace OBase.Pazaryeri.Api.Controllers
{
    [Route("api/v1/yemeksepeti")]
    [ApiController]
    [SwaggerEndpointGroup("YemekSepetiApi")]
    [JwtAuthorize(ValidateUser = true, RequiredService = "YemekSepetiApi", RequiredRoles = new[] { "User" })]
    public class YemekSepetiApiController : BaseController
	{
		private readonly IYemekSepetiOrderService _yemekSepetiOrderService;
		public YemekSepetiApiController(IYemekSepetiOrderService yemekSepetiOrderService)
		{
			_yemekSepetiOrderService = yemekSepetiOrderService;
		}
        [Route("create-order")]
        [HttpPost]      
        public async Task<IActionResult> CreateOrderFromWebHook([FromBody] YemekSepetiOrderDto order)
		{
			//return await Task.FromResult(Ok("Test Başarılı"));
			return ResponseResult(await _yemekSepetiOrderService.SaveOrderOnQp(order));
		}
	}
}