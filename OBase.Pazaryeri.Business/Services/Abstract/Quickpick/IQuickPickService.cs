using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBase.Pazaryeri.Business.Services.Abstract.Quickpick
{
    public interface IQuickPickService
    {
        Task<CommonQpDto<ProductCodeDto>> GetProductCodeByBarcodeAndSaleChannelId(string barcode, string saleChannelId);
        Task<CommonQpDto<ProductCodeDto>> GetInProductBarcodeByBarcode(string barcode);
        Task<CommonQpDto<List<ProductBarcodeDto>>> GetProductBarcodesByBarcodeAndSaleChannelId(string productCode, string saleChannelId);
        Task<CommonQpDto<List<ProductBarcodeDto>>> GetInProductBarcodeByProductCode(string productCode);
        Task<CommonQpDto<ProductDetailDto>> ProductAllInfoByShopCode(string productCode, string storeId, string saleChannelId);
        Task<CommonQpDto<ProductDetailDto>> ProductAllInfoByBarcode(string barcode, string storeId, string saleChannelId);
        CommonQpDto<List<DeliveryDto>> GetDeliverySlot(string storeId, string saleChannelId);
        Task<CommonResponseDto> CallTGCustomer(string qpOrderNumber, string pickerPhoneNumber);
        Task<CommonQpDto<List<CancelOptionResponseDto>>> GetCancelOptions(long orderId);

    }
}
