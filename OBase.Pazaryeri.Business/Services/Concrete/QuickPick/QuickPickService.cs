using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Quickpick;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Abstract.QuickPick;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Getir;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.Trendyol;
using RestEase;
using System.Net;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;

namespace OBase.Pazaryeri.Business.Services.Concrete.QuickPick
{
    public class QuickPickService : IQuickPickService, ILogable
    {
        #region Variables

        private readonly IQuickPickDalService _quickPickDalService;
        private readonly ITrendyolGoClient _trendyolGoClient;
        private readonly ITrendyolClient _trendyolClient;
        private readonly IOptions<AppSettings> _options;
        private readonly ApiDefinitions _apiDefinitionTrendyolGo;
        private readonly ApiDefinitions _apiDefinitionTrendyol;
        private readonly string logFile = CommonConstants.QuickPickLogFile;
        private readonly IGetirCarsiClient _getirCarsiClient;
        private readonly IPazarYeriSiparisDalService _pazarYeriSiparisDalService;

        #endregion

        #region Ctor

        public QuickPickService(IQuickPickDalService quickPickDalService, ITrendyolGoClient trendyolGoClient, IOptions<AppSettings> options, IGetirCarsiClient getirCarsiClient, IPazarYeriSiparisDalService pazarYeriSiparisDalService, ITrendyolClient trendyolClient)
        {
            _quickPickDalService = quickPickDalService;
            _trendyolGoClient = trendyolGoClient;
            _options = options;
            _apiDefinitionTrendyolGo = _options.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.TrendyolGo);
            _apiDefinitionTrendyol = _options.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.Trendyol);
            _getirCarsiClient = getirCarsiClient;
            _pazarYeriSiparisDalService = pazarYeriSiparisDalService;
            _trendyolClient = trendyolClient;
        }

        #endregion

        #region Methods

        public async Task<CommonResponseDto> CallTGCustomer(string qpOrderNumber, string pickerPhoneNumber)
        {
            TrendyolCallCustomerResponseDto tgResponse = new TrendyolCallCustomerResponseDto();
            try
            {

                Logger.Information($"CallTGCustomer > OrderNumber:{qpOrderNumber} | pickerPhoneNumber:{pickerPhoneNumber}", fileName: logFile);

                string validationMessages = Core.Utility.Helper.ValidateCallRequest(qpOrderNumber, pickerPhoneNumber);
                if (validationMessages.Length > 0)
                {
                    return new CommonResponseDto
                    {
                        Message = validationMessages,
                        Success = false,
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
                TrendyolCallCustomerDto callCustomerDto = await _quickPickDalService.GetOrderNumberAndMarketPlaceStoreIdByQpIdAsync(long.Parse(qpOrderNumber));

                if(callCustomerDto is null)
                {
                    return new CommonResponseDto
                    {
                        Message = $"PazaryeriService: {qpOrderNumber} numaralı sipariş bulunamadı!",
                        StatusCode = (HttpStatusCode.BadRequest),
                        Success = false
                    };
                }
                var orderModel = await _pazarYeriSiparisDalService.GetOrderByIdWithDetailsAsync(long.Parse(qpOrderNumber));
                Response<TrendyolCallCustomerResponseDto> customerCallResponseResponse = await _trendyolGoClient.GetInstantCall(_apiDefinitionTrendyolGo.SupplierId, orderModel.PaketId, new TGGetInstantCallRequestDto() { sellerPhoneNumber=pickerPhoneNumber});
                tgResponse = customerCallResponseResponse.GetContent();

                Logger.Information("CallTGCustomer > OrderNumber:{qpOrderNumber} | pickerPhoneNumber:{pickerPhoneNumber} | apiResponse:{@response}", fileName: logFile, qpOrderNumber, pickerPhoneNumber, tgResponse);
                if (tgResponse == null)
                {
                    tgResponse = new TrendyolCallCustomerResponseDto
                    {
                        IsSuccess = true,
                        StatusCode = 200
                    };
                }
            }
            catch (Exception ex)
            {
                tgResponse = new TrendyolCallCustomerResponseDto()
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    AdditionalInfo = ex.Message
                };
                Logger.Error("CallTGCustomer Error: {exception} for orderId: {qpOrderNumber}", fileName: logFile, ex, qpOrderNumber);
            }

            return new CommonResponseDto
            {
                Message = Core.Utility.Helper.ConvertCallResponseMessageToTurkish(string.IsNullOrEmpty(tgResponse.AdditionalInfo)?tgResponse.message: tgResponse.AdditionalInfo),
                StatusCode = (HttpStatusCode)tgResponse.StatusCode,
                Success = tgResponse.IsSuccess
            };
        }

        public CommonQpDto<List<DeliveryDto>> GetDeliverySlot(string storeId, string saleChannelId)
        {
            List<DeliveryDto> deliveryList = new List<DeliveryDto>();
            for (int i = 0; i < 3; i++)
            {
                DeliveryDto delivery = new DeliveryDto()
                {
                    DeliveryDate = DateTime.Now.AddDays(i).ToString("dd.MM.yyyy"),
                    DeliveryTime = "2359",
                    DeliveryTimeId = 2359
                };
                deliveryList.Add(delivery);
            };
            CommonQpDto<List<DeliveryDto>> response = new CommonQpDto<List<DeliveryDto>>()
            {
                Message = "",
                Success = true,
                Data = deliveryList
            };
            return response;
        }

        public async Task<CommonQpDto<ProductCodeDto>> GetInProductBarcodeByBarcode(string barcode)
        {
            CommonQpDto<ProductCodeDto> response = new CommonQpDto<ProductCodeDto>();
            try
            {
                string productCode = await _quickPickDalService.GetInProductBarcodeByBarcodeAsync(barcode);
                response = new CommonQpDto<ProductCodeDto>()
                {
                    Message = string.IsNullOrEmpty(productCode) ? "Ürün kodu bulunamadı." : "Başarılı",
                    Success = string.IsNullOrEmpty(productCode) ? false : true,
                    Data = new ProductCodeDto() { ProductCode = productCode ?? string.Empty }
                };
            }
            catch (Exception ex)
            {
                response = new CommonQpDto<ProductCodeDto>()
                {
                    Message = "Hata Mesajı : " + ex.Message,
                    Success = false,
                    Data = null
                };
                Logger.Error("GetInProductBarcodeByBarcode Error: {exception} for barcode: {barcode}", fileName: logFile, ex, barcode);
            }
            return response;
        }

        public async Task<CommonQpDto<List<ProductBarcodeDto>>> GetInProductBarcodeByProductCode(string productCode)
        {
            CommonQpDto<List<ProductBarcodeDto>> response = new CommonQpDto<List<ProductBarcodeDto>>();
            try
            {
                var barcodes = (await _quickPickDalService.GetInProductBarcodeByProductCodeAsync(productCode)).ToList();
                List<ProductBarcodeDto> barcodeModelList = new List<ProductBarcodeDto>();
                foreach (var barcode in barcodes)
                {
                    barcodeModelList.Add(new ProductBarcodeDto() { Barcode = barcode.Barcode, IsPriority = barcode.IsPriority });
                }
                response = new CommonQpDto<List<ProductBarcodeDto>>()
                {
                    Message = barcodes.Any() ? "Başarılı" : "Barkod bulunamadı.",
                    Success = barcodes.Any() ? true : false,
                    Data = barcodeModelList
                };
            }
            catch (Exception ex)
            {
                response = new CommonQpDto<List<ProductBarcodeDto>>()
                {
                    Message = "Hata Mesajı : " + ex.Message,
                    Success = false,
                    Data = null
                };
                Logger.Error("GetInProductBarcodeByProductCode Error: {exception} for productCode: {productCode}", fileName: logFile, ex, productCode);
            }
            return response;
        }

        public async Task<CommonQpDto<List<ProductBarcodeDto>>> GetProductBarcodesByBarcodeAndSaleChannelId(string productCode, string saleChannelId)
        {
            CommonQpDto<List<ProductBarcodeDto>> response = new CommonQpDto<List<ProductBarcodeDto>>();
            try
            {
                string merchantNo = _quickPickDalService.GetDecodedMerchantNo(saleChannelId);
                var barcodes = (await _quickPickDalService.GetProductBarcodesByBarcodeAndSaleChannelIdAsync(productCode, merchantNo)).ToList();
                List<ProductBarcodeDto> barcodeModelList = new List<ProductBarcodeDto>();
                foreach (var barcode in barcodes)
                {
                    barcodeModelList.Add(new ProductBarcodeDto() { Barcode = barcode, IsPriority = false });
                }
                response = new CommonQpDto<List<ProductBarcodeDto>>()
                {
                    Message = barcodes.Any() ? "Başarılı" : "Barkod bulunamadı.",
                    Success = barcodes.Any() ? true : false,
                    Data = barcodeModelList
                };
            }
            catch (Exception ex)
            {
                response = new CommonQpDto<List<ProductBarcodeDto>>()
                {
                    Message = "Hata Mesajı : " + ex.Message,
                    Success = false,
                    Data = null
                };
                Logger.Error("GetProductBarcodesByBarcodeAndSaleChannelId Error: {exception} for productCode: {productCode}, saleChannelId:{saleChannelId}", fileName: logFile, ex, productCode, saleChannelId);
            }
            return response;
        }

        public async Task<CommonQpDto<ProductCodeDto>> GetProductCodeByBarcodeAndSaleChannelId(string barcode, string saleChannelId)
        {
            CommonQpDto<ProductCodeDto> response = new();
            try
            {
                string merchantNo = _quickPickDalService.GetDecodedMerchantNo(saleChannelId);
                string productCode = await _quickPickDalService.GetProductCodeByBarcodeAndSaleChannelIdAsync(barcode, merchantNo);
                response = new CommonQpDto<ProductCodeDto>()
                {
                    Message = string.IsNullOrEmpty(productCode) ? "Ürün kodu bulunamadı." : "Başarılı",
                    Success = !string.IsNullOrEmpty(productCode),
                    Data = new ProductCodeDto() { ProductCode = productCode ?? string.Empty }
                };
            }
            catch (Exception ex)
            {
                response = new CommonQpDto<ProductCodeDto>()
                {
                    Message = "Hata Mesajı : " + ex.Message,
                    Success = false,
                    Data = null
                };
                Logger.Error("GetProductCodeByBarcodeAndSaleChannelId Error: {exception} for barcode: {barcode}, saleChannelId:{saleChannelId}", fileName: logFile, ex, barcode, saleChannelId);
            }
            return response;
        }

        public async Task<CommonQpDto<ProductDetailDto>> ProductAllInfoByBarcode(string barcode, string storeId, string saleChannelId)
        {
            CommonQpDto<ProductDetailDto> response = new CommonQpDto<ProductDetailDto>();
            try
            {
                string merchantNo = _quickPickDalService.GetDecodedMerchantNo(saleChannelId);
                string productCode = await _quickPickDalService.GetInProductBarcodeByBarcodeAsync(barcode);
                if (string.IsNullOrWhiteSpace(productCode))
                {
                    response = new CommonQpDto<ProductDetailDto>()
                    {
                        Message = "Ürün bulunamadı.",
                        Success = false,
                        Data = null
                    };
                    return response;
                }

                var productDetail = (await _quickPickDalService.ProductAllInfoMerchantViewAsync(productCode, merchantNo, storeId));
                List<ProductBarcodeDto> barcodeModelList = new()
                {
                    new ProductBarcodeDto() { Barcode = barcode, IsPriority = false }
                };
                if (productDetail != null)
                    productDetail.Barcodes = barcodeModelList;

                response = new CommonQpDto<ProductDetailDto>()
                {
                    Message = productDetail == null ? "Ürün detayları bulunamadı." : "Başarılı",
                    Success = productDetail != null,
                    Data = productDetail
                };
            }
            catch (Exception ex)
            {
                response = new CommonQpDto<ProductDetailDto>()
                {
                    Message = "Hata Mesajı : " + ex.Message,
                    Success = false,
                    Data = null
                };
                Logger.Error("ProductAllInfoByBarcode Error: {exception} for barcode: {barcode}, saleChannelId:{saleChannelId}", fileName: logFile, ex, barcode, saleChannelId);
            }
            return response;
        }

        public async Task<CommonQpDto<ProductDetailDto>> ProductAllInfoByShopCode(string productCode, string storeId, string saleChannelId)
        {
            CommonQpDto<ProductDetailDto> response = new CommonQpDto<ProductDetailDto>();
            try
            {
                //string merchantNo = _quickPickDalService.GetDecodedMerchantNo(saleChannelId);
                string merchantNo = string.Empty;

                if (saleChannelId == SaleChannelId.TrendyolGO.ToString())
                {
                    merchantNo = PazarYeri.TrendyolGo;
                }
                else if (saleChannelId == SaleChannelId.Getir.ToString())
                {
                    merchantNo = PazarYeri.GetirCarsi;
                }
                else if (saleChannelId == SaleChannelId.YemekSepeti.ToString())
                {
                    merchantNo = PazarYeri.Yemeksepeti;
                }
                else if (saleChannelId == SaleChannelId.HepsiExpress.ToString())
                {
                    merchantNo = PazarYeri.HepsiExpress;
                }
                else if (saleChannelId == SaleChannelId.Idefix.ToString())
                {
                    merchantNo = PazarYeri.Idefix;
                }
                var barcodes = (await _quickPickDalService.GetProductBarcodesWithPriorityNumberAsync(productCode)).ToList();
                var productDetail = (await _quickPickDalService.ProductAllInfoMerchantViewAsync(productCode, merchantNo, storeId));
                string marketplaceStoreId = await _quickPickDalService.GetMarketPlaceStoreIdByObaseStoreIdAsync(storeId, merchantNo);

                string imageUrl = string.Empty;
                string imageUrls = _quickPickDalService.GetProductPhoto(marketplaceStoreId, productCode);
                if (!string.IsNullOrEmpty(imageUrls))
                {
                    imageUrl = imageUrls.Split(",")[0];
                }
                List<ProductBarcodeDto> barcodeModelList = new();

                foreach (var barcode in barcodes)
                {
                    barcodeModelList.Add(new ProductBarcodeDto() { Barcode = barcode.Barcode, IsPriority = barcode.IsPriority });
                }

                if (productDetail != null)
                {
                    productDetail.Barcodes = barcodeModelList;
                    productDetail.ImageMedium = imageUrl;
                    productDetail.ImageSmall = imageUrl;
                }
                response = new CommonQpDto<ProductDetailDto>()
                {
                    Message = productDetail == null ? "Ürün detayları bulunamadı." : "Başarılı",
                    Success = productDetail != null,
                    Data = productDetail
                };
            }
            catch (Exception ex)
            {
                response = new CommonQpDto<ProductDetailDto>()
                {
                    Message = "Hata Mesajı : " + ex.Message,
                    Success = false,
                    Data = null
                };
                Logger.Error("ProductAllInfoByShopCode Error: {exception} for productCode: {productCode}, storeId: {storeId}, saleChannelId:{saleChannelId}", fileName: logFile, ex, productCode, storeId, saleChannelId);
            }
            return response;
        }

        public async Task<CommonQpDto<List<CancelOptionResponseDto>>> GetCancelOptions(long orderId)
        {
            CommonQpDto<List<CancelOptionResponseDto>> cancelOptionList = new CommonQpDto<List<CancelOptionResponseDto>>();
            GenericGetirResponse<List<CancelOptionResponseDto>> getirResponse = new GenericGetirResponse<List<CancelOptionResponseDto>>();
            var order = await _pazarYeriSiparisDalService.GetOrderByIdAsync(orderId);
            if (order is not null)
            {
                var response = await _getirCarsiClient.CancelOptions(order.SiparisId);
                if (response.ResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    getirResponse = JsonConvert.DeserializeObject<GenericGetirResponse<List<CancelOptionResponseDto>>>(response.StringContent);
                    if (string.IsNullOrEmpty(getirResponse.Meta.returnMessage))
                    {
                        cancelOptionList.Data = getirResponse.Data;
                        cancelOptionList.Success = true;
                        cancelOptionList.Message = string.Empty;
                    }
                    else
                    {
                        cancelOptionList.Data = getirResponse.Data;
                        cancelOptionList.Success = false;
                        cancelOptionList.Message = $"{getirResponse.Meta.returnCode} - {getirResponse.Meta.returnMessage}";
                        Logger.Warning("GetCancelOptions Exception {exception} for Order: {orderId}", fileName: logFile, cancelOptionList.Message, orderId);
                    }
                }
                else
                {
                    cancelOptionList.Data = getirResponse.Data;
                    cancelOptionList.Success = false;
                    cancelOptionList.Message = $"Getir Çarşı'dan Sipariş nedenleri çekilirken bir hata oluştu. Getir Status Code {response.ResponseMessage.StatusCode}";
                    Logger.Warning("GetCancelOptions Exception {exception} for Order: {orderId}", fileName: logFile, cancelOptionList.Message, orderId);
                }
            }
            else
            {
                cancelOptionList.Data = null;
                cancelOptionList.Success = false;
                cancelOptionList.Message = "Sipariş bulunamadı.";
            }

            return cancelOptionList;
        }

        #endregion


    }
}
