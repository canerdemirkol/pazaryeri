using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OBase.Pazaryeri.Api.Attributes;
using OBase.Pazaryeri.Business.Services.Abstract.Sale;
using OBase.Pazaryeri.Domain.Dtos.Sale;

namespace OBase.Pazaryeri.Api.Controllers
{
    [Route("api/v1/sales")]
    [ApiController]
    [SwaggerEndpointGroup("SaleApi")]
    [JwtAuthorize(ValidateUser = true, RequiredService = "SaleInfo", RequiredRoles = new[] { "User" })]
    public class SaleApiController : BaseController
	{
		private readonly ISaleService _saleService;
		public SaleApiController(ISaleService saleService)
		{
            _saleService = saleService;
		}
        [Route("create")]
        [HttpPost]      
        public async Task<IActionResult> CreateSaleInfoFromWebHook([FromBody] SaleInfoDto saleInfo)
		{
			return ResponseResult(await _saleService.SaveSaleInfo(saleInfo));
		}
	}
}