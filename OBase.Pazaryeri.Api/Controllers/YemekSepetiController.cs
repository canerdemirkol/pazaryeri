using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OBase.Pazaryeri.Api.Attributes;
using OBase.Pazaryeri.Business.Services.Abstract.PushPrice;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;

namespace OBase.Pazaryeri.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [SwaggerEndpointGroup("YemekSepeti")]
    [JwtAuthorize(ValidateUser = true, RequiredService = "YemekSepeti", RequiredRoles = new[] { "Admin" })]
    public class YemekSepetiController : BaseController
    {
        private readonly IYemekSepetiPushPriceStockService _yemekSepetiPushPriceStockService;

        public YemekSepetiController(IYemekSepetiPushPriceStockService yemekSepetiPushPriceStockService)
        {
            _yemekSepetiPushPriceStockService = yemekSepetiPushPriceStockService;
        }

        [HttpPost("SendVerifyUrl")]
        public IActionResult SendVerifyUrl(YemekSepetiVerifyRequestDto request)
        {
            return ResponseResult(_yemekSepetiPushPriceStockService.SendVerifyUrl(request));
        }
    }
}
