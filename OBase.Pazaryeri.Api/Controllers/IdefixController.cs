using Microsoft.AspNetCore.Mvc;
using OBase.Pazaryeri.Api.Attributes;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Business.Services.Concrete.Order;
using OBase.Pazaryeri.Domain.Dtos.Idefix;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Api.Controllers
{


    [Route("api/v1/idefix")]
    [ApiController]
    [SwaggerEndpointGroup("Idefix")]
    [JwtAuthorize(ValidateUser = true, RequiredService = "Idefix", RequiredRoles = new[] { "Admin" })]
    public class IdefixController : BaseController
    {
        private readonly IIdefixOrderService _idefixOrderService;
        public IdefixController(IIdefixOrderService idefixOrderService)
        {
            _idefixOrderService = idefixOrderService;
        }
        [Route("create-order")]
        [HttpPost]
        public async Task<IActionResult> CreateOrderFromWebHook([FromBody] IdefixOrderDto order)
        {
            return ResponseResult(await _idefixOrderService.SaveIdefixCreatedOrderAsync(order));
        }
    }
}