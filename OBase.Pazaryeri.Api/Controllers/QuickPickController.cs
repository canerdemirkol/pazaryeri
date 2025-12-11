using Microsoft.AspNetCore.Mvc;
using OBase.Pazaryeri.Api.Attributes;
using OBase.Pazaryeri.Business.Services.Abstract.Quickpick;

namespace OBase.Pazaryeri.Api.Controllers
{
    [Route("api/v1/quickpick")]
    [ApiController]
    [SwaggerEndpointGroup("QuickPick")]
    public class QuickPickController : ControllerBase
    {
        private readonly IQuickPickService _service;

        public QuickPickController(IQuickPickService service)
        {
            this._service = service;
        }
        [Route("ProductShopCode")]
        [HttpGet]
        public async Task<IActionResult> GetProductCodeByBarcodeAndSaleChannelId([FromQuery] string barcode)
        {
            var response = await _service.GetInProductBarcodeByBarcode(barcode);
            return Ok(response);
        }


        [Route("ProductBarcode")]
        [HttpGet]
        public async Task<IActionResult> ProductBarcode([FromQuery] string productCode)
        {
            var response = await _service.GetInProductBarcodeByProductCode(productCode);
            return Ok(response);
        }


        [Route("ProductAllInfoByShopCode")]
        [HttpGet]
        public async Task<IActionResult> ProductAllInfoByShopCode([FromQuery] string productCode, [FromQuery] string storeId, [FromQuery] string saleChannelId)
        {
            var response = await _service.ProductAllInfoByShopCode(productCode, storeId, saleChannelId);
            return Ok(response);
        }


        [Route("ProductAllInfoByBarcode")]
        [HttpGet]
        public async Task<IActionResult> ProductAllInfoByBarcode([FromQuery] string barcode, [FromQuery] string storeId, [FromQuery] string saleChannelId)
        {
            var response = await _service.ProductAllInfoByBarcode(barcode, storeId, saleChannelId);
            return Ok(response);
        }


        [Route("AvailableTimeGroup")]
        [HttpGet]
        public IActionResult GetDeliverySlot([FromQuery] string storeId, [FromQuery] string saleChannelId)
        {
            var response = _service.GetDeliverySlot(storeId, saleChannelId);
            return Ok(response);
        }

        [Route("CallTYGoClient")]
        [HttpGet]
        public async Task<IActionResult> CallTCustomer([FromQuery] string qpOrderNumber, [FromQuery] string pickerPhoneNumber)
        {
            var response = await _service.CallTGCustomer(qpOrderNumber, pickerPhoneNumber);
            return Ok(response);
        }

        [Route("GetirCancelOptions")]
        [HttpGet]
        public async Task<IActionResult> GetGetirCancelOptions(long orderId)
        {
                var response = await _service.GetCancelOptions(orderId);
                return Ok(response);
        }
    }
}