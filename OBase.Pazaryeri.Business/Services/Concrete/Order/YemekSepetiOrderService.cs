using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Helper;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Abstract.QuickPick;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using OBase.Pazaryeri.Domain.Entities;
using Polly;
using Polly.Retry;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Constants.Constants.YemekSepetiConstants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using static OBase.Pazaryeri.Domain.Helper.CommonHelper;

namespace OBase.Pazaryeri.Business.Services.Concrete.Order
{
    public class YemekSepetiOrderService : BaseService, IYemekSepetiOrderService
    {
        #region Variables

        private readonly IOptions<AppSettings> _appSetting;
        private readonly IPazarYeriBirimTanimDalService _pazarYeriBirimTanimDalService;
        private readonly IPazarYeriMalTanimDalService _pazarYeriMalTanimDalService;
        private readonly IPazarYeriAktarimDalService _pazarYeriAktarimDalService;
        private readonly IPazarYeriSiparisUrunDalService _pazarYeriSiparisUrunDalService;
        private readonly IPazarYeriSiparisEkBilgiDalService _pazarYeriSiparisEkBilgiDalService;
        private readonly IPazarYeriKargoAdresDalService _pazarYeriKargoAdresDalService;
        private readonly IPazarYeriFaturaAdresDalService _pazarYeriFaturaAdresDalService;
        private readonly IYemekSepetiClient _yemekSepetiClient;
        private readonly ITransactionDalService _transactionDalService;
        private readonly ApiDefinitions _apiDefinition;
        private readonly QPService.OrderDeliveryServiceSoapClient _qpClient;
        private readonly string _logFolderName = nameof(PazarYerleri.YemekSepeti);
        private readonly IOrderConvertService _orderConvertService;
        private readonly IQuickPickDalService _quickPickDalService;
        #endregion

        #region Ctor

        public YemekSepetiOrderService(IYemekSepetiClient yemekSepetiClient, IPazarYeriBirimTanimDalService pazarYeriBirimTanimDalService,
            IPazarYeriMalTanimDalService pazarYeriMalTanimDalService, IPazarYeriSiparisDalService pazarYeriSiparisDalService,
            IPazarYeriAktarimDalService pazarYeriAktarimDalService, ITransactionDalService transactionDalService, IPazarYeriFaturaAdresDalService pazarYeriFaturaAdresDalService,
            IPazarYeriKargoAdresDalService pazarYeriKargoAdresDalService, IPazarYeriSiparisEkBilgiDalService pazarYeriSiparisEkBilgiDalService,
            IPazarYeriSiparisUrunDalService pazarYeriSiparisUrunDalService,
            IPazarYeriSiparisDetayDalService pazarYeriSiparisDetayDalService, IOptions<AppSettings> options, IGetDalService getDalService, IMailService mailService, IOrderConvertService orderConvertService, IQuickPickDalService quickPickDalService) : base(pazarYeriSiparisDalService, pazarYeriSiparisDetayDalService, options, mailService)
        {
            _yemekSepetiClient = yemekSepetiClient;
            _pazarYeriBirimTanimDalService = pazarYeriBirimTanimDalService;
            _pazarYeriMalTanimDalService = pazarYeriMalTanimDalService;
            _pazarYeriAktarimDalService = pazarYeriAktarimDalService;
            _transactionDalService = transactionDalService;
            _pazarYeriFaturaAdresDalService = pazarYeriFaturaAdresDalService;
            _pazarYeriKargoAdresDalService = pazarYeriKargoAdresDalService;
            _pazarYeriSiparisEkBilgiDalService = pazarYeriSiparisEkBilgiDalService;
            _pazarYeriSiparisUrunDalService = pazarYeriSiparisUrunDalService;
            _appSetting = options;
            _apiDefinition = options.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Yemeksepeti);
            _qpClient = new QPService.OrderDeliveryServiceSoapClient(QPService.OrderDeliveryServiceSoapClient.EndpointConfiguration.OrderDeliveryServiceSoap12, remoteAddress: _appSetting.Value.WareHouseUrl);
            _orderConvertService = orderConvertService;
            _quickPickDalService = quickPickDalService;
        }

        #endregion

        #region Methods

        public async Task<CommonResponseDto> OrderUpdatePackageStatus(OrderStatuUpdateRequestDto orderDto, PazarYeriSiparis orderEntity)
        {
            Logger.Information("OrderUpdatePackageStatus Request :{@request} ", fileName: _logFolderName, orderDto);

            try
            {
                bool enableMarketPlaceServices = _appSetting.Value.EnableMarketPlaceServices;
                var status = orderDto.Status;

                if (!orderDto.ProductQuantities.Any())
                {
                    return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "En Az Bir Ürün Göndermelisiniz");
                }
                IEnumerable<PazarYeriSiparisUrun> mpOrderProducts = new List<PazarYeriSiparisUrun>();


                switch (status)
                {
                    case StatusEnums.Picking:
                        {
                            await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderEntity.SiparisId, nameof(StatusEnums.Picking), PazarYeri.Yemeksepeti);
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, CalcSuccessFromHTTPStatus(HttpStatusCode.OK), "Siparişin durumu yolda olarak güncellenmiştir.");
                        }
                    case StatusEnums.Collected:
                        {
                            if (orderEntity.SevkiyatPaketDurumu is not null
                                && (orderEntity.SevkiyatPaketDurumu == nameof(StatusEnums.Collected)
                                || orderEntity.SevkiyatPaketDurumu == nameof(StatusEnums.InTransit)))
                            {
                                // -> 2025-10-06 yemeksepeti bad-request workaround !
                                // -> yemeksepeti bize hata dönmesine rağmen statüyü geçirdiği oluyormuş
                                // eğer sipariş zaten collected veya in-transit durumuna geçmiş ise , tekrar işlem yapmana gerek yok.
                                // başarılı dönülebilir.
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName
                                    , CalcSuccessFromHTTPStatus(HttpStatusCode.OK)
                                    , $"Sipariş zaten {orderEntity.SevkiyatPaketDurumu} durumunda.");
                            }

                            SachetProduct[] sachetProducts = _apiDefinition.SachetProducts;
                            Dictionary<string, string> productNoMap = new();
                            int bagCount = 0;
                            decimal bagTotalPrice = 0;
                            YemekSepetiUpdateOrderRequestDto orderUpdateRequest = new()
                            {
                                Status = orderEntity.KargoSaglayiciAdi == OrderTransportType.VD ? OrderStatus.Dispatched : OrderStatus.ReadyForPickup,
                                Cancellation = null,
                                OrderId = orderEntity.SiparisId,
                                Items = new List<OrderItem>()
                            };

                            foreach (ProductQuantity productQuantity in orderDto.ProductQuantities)
                            {
                                SachetProduct sachetProduct = Array.Find(sachetProducts, sp => sp.ProductCode == productQuantity.ProductId);
                                if (sachetProduct is not null)
                                {
                                    decimal sachetPrice = 0;
                                    if (sachetProduct != null) { sachetPrice = sachetProduct.Price; }
                                    int bagQuantity = (int)Math.Round(productQuantity.Quantity);
                                    bagCount += bagQuantity;
                                    bagTotalPrice += bagQuantity * sachetPrice;
                                }
                                string strProductNo = productQuantity.ProductId.Trim();
                                string productMarketPlaceId = await _pazarYeriMalTanimDalService.GetProductMarketPlaceIdByObaseProductIdAsync(strProductNo, _apiDefinition.Merchantno);
                                productNoMap.Add(strProductNo, productMarketPlaceId);
                            }

                            IEnumerable<PazarYeriMalTanim> productsWithKGUnit = await _pazarYeriMalTanimDalService.GetProductSalesValueByUnitAsync(CommonConstants.KG, _apiDefinition.Merchantno);
                            mpOrderProducts = await _pazarYeriSiparisUrunDalService.GetOrderProductsByIdAsync(orderDto.OrderId);
                            Dictionary<string, TGProductAdditionalDataDto> mpOrderProductAdditionalDataDict = new();
                            decimal totalAmount = 0;
                            foreach (var mpOrderProduct in mpOrderProducts)
                            {
                                ProductQuantity qpProduct = orderDto.ProductQuantities.Find(qpX => qpX.ProductId == mpOrderProduct.ObaseMalNo);
                                var selectedProductDetail = orderEntity.PazarYeriSiparisDetails.Where(x => x.ObaseMalNo == qpProduct.ProductId).MaxBy(x => x.Miktar);
                                ProductQuantity qpAltProduct = orderDto.ProductQuantities.Find(qpX => qpX.AltProductId == mpOrderProduct.ObaseMalNo) ?? null;
                                TGProductAdditionalDataDto mpOrderProductAdditionalData = new();

                                if (qpAltProduct is not null && qpAltProduct.Quantity > 0)
                                {
                                    mpOrderProductAdditionalData.AltProductId = qpAltProduct.ProductId;
                                    mpOrderProductAdditionalData.altSuppliedQuantity = qpAltProduct.Quantity;
                                }
                                mpOrderProductAdditionalData.ProductId = qpProduct.ProductId;
                                mpOrderProductAdditionalData.LineItemId = selectedProductDetail.LineItemId;
                                mpOrderProductAdditionalData.orderedQuantity = mpOrderProduct.Miktar ?? 0;
                                mpOrderProductAdditionalData.isWeightedItem = productsWithKGUnit.Any(x => x.MalNo == mpOrderProduct.ObaseMalNo);
                                var pyProductUnitValue = productsWithKGUnit.FirstOrDefault(x => x.MalNo == mpOrderProduct.ObaseMalNo)?.PyUrunSatisDeger ?? 1;
                                mpOrderProductAdditionalData.tyQuantityCoefficient = (mpOrderProductAdditionalData.isWeightedItem) ? pyProductUnitValue : 1;
                                decimal suppliedQuantityTemp = 0;
                                decimal productMPUnitPrice = (selectedProductDetail.NetTutar / selectedProductDetail.Miktar);
                                mpOrderProductAdditionalData.productMPUnitPrice = productMPUnitPrice;
                                suppliedQuantityTemp = qpProduct.Quantity;
                                mpOrderProductAdditionalData.suppliedQuantity = suppliedQuantityTemp;

                                decimal unSuppliedQuantityTemp = (mpOrderProduct.Miktar ?? 0) - suppliedQuantityTemp;
                                mpOrderProductAdditionalData.unSuppliedQuantity = (unSuppliedQuantityTemp < 0) ? 0 : unSuppliedQuantityTemp;
                                mpOrderProductAdditionalData.isUnSupplied = mpOrderProductAdditionalData.unSuppliedQuantity > 0;
                                mpOrderProductAdditionalData.isFullyUnSupplied = suppliedQuantityTemp <= 0;
                                mpOrderProductAdditionalDataDict.Add(mpOrderProduct.ObaseMalNo, mpOrderProductAdditionalData);

                                if (mpOrderProductAdditionalData.isWeightedItem)
                                {
                                    var productWeight = qpProduct.Quantity;
                                    if (productWeight > 0 && !(productWeight >= mpOrderProduct.MinMiktar && productWeight <= mpOrderProduct.MaxMiktar))
                                    {
                                        return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, true, $"Ürünün miktarı {mpOrderProduct.MinMiktar}-{mpOrderProduct.MaxMiktar} aralığında bir değer olmalıdır.");
                                    }
                                }

                                totalAmount += productMPUnitPrice * mpOrderProductAdditionalData.suppliedQuantity;
                            }
                            try
                            {
                                PazarYeriSiparisEkBilgi pazarYeriSiparisEkBilgi = await _pazarYeriSiparisEkBilgiDalService.GetAdditionalDataAsync(orderDto.OrderId);
                                if (pazarYeriSiparisEkBilgi != null)
                                {
                                    pazarYeriSiparisEkBilgi.PosetSayisi = bagCount;
                                    pazarYeriSiparisEkBilgi.PosetTutari = bagTotalPrice;
                                    pazarYeriSiparisEkBilgi.GuncelFaturaTutar = decimal.Round(totalAmount, 2, MidpointRounding.AwayFromZero);
                                    await _pazarYeriSiparisEkBilgiDalService.UpdateOrderAdditionalDataAsync(pazarYeriSiparisEkBilgi);
                                }
                                foreach (var mpOrderProductNo in mpOrderProducts.Select(s => s.ObaseMalNo))
                                {
                                    TGProductAdditionalDataDto productAdditionalData = mpOrderProductAdditionalDataDict[mpOrderProductNo];
                                    var product = await _pazarYeriSiparisUrunDalService.GetOrderProductByIdAsync(orderDto.OrderId, mpOrderProductNo);
                                    if (product != null)
                                    {
                                        product.AltUrunMiktar = productAdditionalData.altSuppliedQuantity;
                                        product.AltUrunObaseMalNo = productAdditionalData.AltProductId ?? "";
                                        productNoMap.TryGetValue(product.AltUrunObaseMalNo, out string altMalNo);
                                        product.AltUrunPazarYeriMalNo = altMalNo ?? "";
                                        product.GuncelMiktar = productAdditionalData.suppliedQuantity;
                                        product.IsAlternativeEH = string.IsNullOrEmpty(product.AltUrunPazarYeriMalNo) ? Character.H : Character.E;
                                        await _pazarYeriSiparisUrunDalService.UpdateProductAsync(product);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("OrderUpdatePackageStatus => PazarYeriSiparisEkBilgi Hata: {exception}", _logFolderName, ex);
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.InternalServerError, _logFolderName, false, "Sipariş tabloları güncellenirken bir hata oluştu: " + ex.Message);
                            }
                            if (enableMarketPlaceServices)
                            {

                                Logger.Information("OrderUpdatePackageStatus => MarketPlaceOrderProductAdditionalDatas: {@request}", _logFolderName, Newtonsoft.Json.JsonConvert.SerializeObject(mpOrderProductAdditionalDataDict));

                                #region original products
                                var originalProducts = mpOrderProductAdditionalDataDict
                                       .Where(x => string.IsNullOrEmpty(x.Value.AltProductId))
                                       .Select(x => x.Value)
                                       .ToList();

                                Logger.Information("OrderUpdatePackageStatus => MarketPlaceOrderProductAdditionalDatas =>: Original Products => {@request}", _logFolderName, Newtonsoft.Json.JsonConvert.SerializeObject(originalProducts));

                                foreach (var original in originalProducts)
                                {

                                    ProductQuantity qpProduct = orderDto.ProductQuantities.Find(qpX => qpX.ProductId == original.ProductId);
                                    var selectedProductDetail = orderEntity.PazarYeriSiparisDetails.Where(x => x.ObaseMalNo == qpProduct.ProductId).MaxBy(x => x.Miktar);

                                    OrderItem orderItem = new()
                                    {
                                        Name = string.Empty,
                                        Sku = original.ProductId,
                                        Status = original.suppliedQuantity > 0 ? ProductStatus.InCart : ProductStatus.NotFound,
                                        Id = original.LineItemId
                                    };
                                    if (original.isWeightedItem)
                                    {
                                        var productWeight = original.suppliedQuantity;
                                        orderItem.Pricing = new()
                                        {
                                            TotalPrice = productWeight == 0 ? selectedProductDetail.BrutTutar : null,
                                            PricingType = ProductType.KG,
                                            Weight = productWeight == 0 ? selectedProductDetail.Weight ?? 0 : qpProduct.Quantity,
                                            Quantity = productWeight == 0 ? Convert.ToInt32(selectedProductDetail.Miktar) : Convert.ToInt32(productWeight)
                                        };
                                    }
                                    else
                                    {
                                        orderItem.Pricing = new()
                                        {
                                            TotalPrice = original.productMPUnitPrice * original.suppliedQuantity,
                                            PricingType = ProductType.Unit,
                                            Weight = 0,
                                            Quantity = Convert.ToInt32(original.suppliedQuantity)
                                        };
                                    }
                                    orderUpdateRequest.Items.Add(orderItem);
                                }
                                #endregion

                                #region replaced products
                                var replacedProducts = mpOrderProductAdditionalDataDict
                                      .Where(x => !string.IsNullOrEmpty(x.Value.AltProductId))
                                      .Select(x => x.Value)
                                      .ToList();
                                Logger.Information("OrderUpdatePackageStatus => MarketPlaceOrderProductAdditionalDatas =>: Replaced Products => {@request}", _logFolderName, Newtonsoft.Json.JsonConvert.SerializeObject(replacedProducts));

                                foreach (var replacedProduct in replacedProducts)
                                {
                                    ProductQuantity qpProduct = orderDto.ProductQuantities.Find(qpX => qpX.ProductId == replacedProduct.ProductId);
                                    var replacedProductDetail = orderEntity.PazarYeriSiparisDetails.Where(x => x.ObaseMalNo == qpProduct.ProductId).MaxBy(x => x.Miktar);

                                    OrderItem replacedOrderItem = new()
                                    {
                                        Name = string.Empty,
                                        Sku = replacedProduct.ProductId,
                                        Status = ProductStatus.Replaced,
                                        Id = replacedProduct.LineItemId

                                    };

                                    OrderItem incartOrderItem = new()
                                    {
                                       
                                        Name = string.Empty,
                                        Sku = replacedProduct.AltProductId,
                                        Status = ProductStatus.InCart,
                                        ReplacedId = replacedProduct.LineItemId,
                                    };

                                    if (replacedProduct.isWeightedItem)
                                    {
                                        replacedOrderItem.Pricing = new()
                                        {
                                            TotalPrice = replacedProduct.suppliedQuantity == 0 ? replacedProductDetail.BrutTutar : null,
                                            PricingType = ProductType.KG,
                                            Weight = replacedProduct.suppliedQuantity == 0 ? replacedProductDetail.Weight ?? 0 : qpProduct.Quantity,
                                            Quantity = replacedProduct.suppliedQuantity == 0 ? Convert.ToInt32(replacedProductDetail.Miktar) : Convert.ToInt32(replacedProduct.suppliedQuantity)
                                        };

                                        incartOrderItem.Pricing = new()
                                        {
                                            TotalPrice = replacedProduct.unSuppliedQuantity == 0 ? replacedProductDetail.BrutTutar : null,
                                            PricingType = ProductType.KG,
                                            Weight = replacedProduct.unSuppliedQuantity == 0 ? replacedProductDetail.Weight ?? 0 : replacedProduct.unSuppliedQuantity,
                                            Quantity = replacedProduct.unSuppliedQuantity == 0 ? Convert.ToInt32(replacedProduct.orderedQuantity) : Convert.ToInt32(replacedProduct.unSuppliedQuantity)
                                        };
                                    }
                                    else
                                    {
                                        replacedOrderItem.Pricing = new()
                                        {
                                            TotalPrice = replacedProduct.productMPUnitPrice * replacedProduct.suppliedQuantity,
                                            PricingType = ProductType.Unit,
                                            Weight = 0,
                                            Quantity = Convert.ToInt32(replacedProduct.suppliedQuantity)
                                        };

                                        incartOrderItem.Pricing = new()
                                        {
                                            TotalPrice = replacedProduct.productMPUnitPrice * replacedProduct.orderedQuantity,
                                            PricingType = ProductType.Unit,
                                            Weight = 0,
                                            Quantity = Convert.ToInt32(replacedProduct.orderedQuantity)
                                        };
                                    }
                                    orderUpdateRequest.Items.Add(replacedOrderItem);
                                    orderUpdateRequest.Items.Add(incartOrderItem);
                                }
                                #endregion


                                Logger.Information("OrderUpdatePackageStatus => UpdateOrderAsync Request: {@request}", _logFolderName, orderUpdateRequest);
                                var yemekSepetiResponse = await _yemekSepetiClient.UpdateOrderAsync(orderEntity.SiparisId, orderUpdateRequest);
                                Logger.Information("OrderUpdatePackageStatus => UpdateOrderAsync Response StatusCode: {statusCode}, Response: {response}", _logFolderName, yemekSepetiResponse?.ResponseMessage?.StatusCode ?? HttpStatusCode.InternalServerError, yemekSepetiResponse?.StringContent ?? string.Empty);
                                if (yemekSepetiResponse?.ResponseMessage?.IsSuccessStatusCode ?? false)
                                {
                                    await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderEntity.SiparisId, nameof(StatusEnums.Collected), PazarYeri.Yemeksepeti);
                                }

                                string qpMessage = ParseFaultResponseMessageUserFriendly(yemekSepetiResponse?.StringContent ?? "", yemekSepetiResponse?.ResponseMessage?.IsSuccessStatusCode ?? false);
                                return OrderHelper.ReturnQPResponseV2(yemekSepetiResponse?.ResponseMessage?.StatusCode ?? HttpStatusCode.InternalServerError
                                , _logFolderName
                                , CalcSuccessFromHTTPStatus(yemekSepetiResponse?.ResponseMessage?.StatusCode ?? HttpStatusCode.InternalServerError)
                                , $"Yemeksepeti Dönüşü -> <br /> {qpMessage}");
                            }
                            else
                            {
                                await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderEntity.SiparisId, nameof(StatusEnums.Prepared), PazarYeri.Yemeksepeti);
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için Yemek Sepeti üzerinde işlem yapılmamıştır.");
                            }
                        }
                    case StatusEnums.Cancelled:
                        {
                            if (enableMarketPlaceServices)
                            {
                                YemekSepetiUpdateOrderRequestDto orderUpdateRequest = new()
                                {
                                    Status = OrderStatus.Cancelled,
                                    Cancellation = new Cancellations { Reason = orderDto.ReasonId },
                                    OrderId = orderEntity.SiparisId,
                                    Items = new List<OrderItem>()
                                };
                                foreach (var mpOrderProduct in mpOrderProducts)
                                {
                                    OrderItem orderItem = new()
                                    {
                                        Id = mpOrderProduct.PySiparisNo,
                                        Name = string.Empty,
                                        Sku = mpOrderProduct.PazarYeriMalNo,
                                        Status = mpOrderProduct.Miktar <= 0 ? ProductStatus.NotFound : ProductStatus.InCart,
                                        ReplacedId = string.Empty,
                                        Pricing = null,
                                    };
                                    orderUpdateRequest.Items.Add(orderItem);
                                }

                                Logger.Information("OrderUpdatePackageStatus => Cancel Request: {@request}", _logFolderName, orderUpdateRequest);
                                var yemekSepetiResponse = await _yemekSepetiClient.UpdateOrderAsync(orderEntity.SiparisId, orderUpdateRequest);
                                Logger.Information("OrderUpdatePackageStatus => Cancel Response StatusCode: {statusCode}, Response: {@response}", _logFolderName, yemekSepetiResponse.ResponseMessage.StatusCode, yemekSepetiResponse?.StringContent ?? "");

                                if (yemekSepetiResponse.ResponseMessage.IsSuccessStatusCode)
                                {
                                    await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderEntity.SiparisId, nameof(StatusEnums.Cancelled), PazarYeri.Yemeksepeti);
                                }

                                var qpMessage = ParseFaultResponseMessageUserFriendly(yemekSepetiResponse?.StringContent ?? "", yemekSepetiResponse?.ResponseMessage?.IsSuccessStatusCode ?? false);
                                return OrderHelper.ReturnQPResponseV2(yemekSepetiResponse.ResponseMessage.StatusCode
                                , _logFolderName
                                , CalcSuccessFromHTTPStatus(yemekSepetiResponse.ResponseMessage.StatusCode)
                                , $"Yemeksepeti Dönüşü -> <br /> {qpMessage}");
                            }
                            else
                            {
                                await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderEntity.SiparisId, nameof(StatusEnums.Cancelled), PazarYeri.Yemeksepeti);
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için Yemek Sepeti üzerinde işlem yapılmamıştır.");
                            }
                        }
                    case StatusEnums.InTransit:
                        {
                            await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderEntity.SiparisId, nameof(StatusEnums.InTransit), PazarYeri.Yemeksepeti);
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, CalcSuccessFromHTTPStatus(HttpStatusCode.OK), "Siparişin durumu yolda olarak güncellenmiştir.");
                        }
                    case StatusEnums.Delivered:
                        {
                            await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderEntity.SiparisId, nameof(StatusEnums.Delivered), PazarYeri.Yemeksepeti);
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, CalcSuccessFromHTTPStatus(HttpStatusCode.OK), string.Empty);
                        }
                    case StatusEnums.Completed:
                        {
                            await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderEntity.SiparisId, nameof(StatusEnums.Completed), PazarYeri.Yemeksepeti);
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Completed işlemi tamamlandı.");
                        }
                    default:
                        {
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "Yemek Sepeti (Yemek Sepeti kuryesi ile teslimat) siparişlerinde bu statüye geçiş yapılamaz.");
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("OrderUpdatePackageStatus exception: {exception}", _logFolderName, ex);
                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.InternalServerError, _logFolderName, false, $"Sistem Hatası: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<CommonResponseDto>> SaveOrderOnQp(YemekSepetiOrderDto orderDto)
        {
            if (orderDto is null)
            {
                await SendFailedOrderMailFormattedAsync("YemekSepeti Siparişi Kaydedilirken Hata", $"The submitted order object cannot be empty!", orderDto);
                return ServiceResponse<CommonResponseDto>.Error("The submitted order object cannot be empty!");
            }
            if (!orderDto.Items?.Any() ?? true)
            {
                await SendFailedOrderMailFormattedAsync("YemekSepeti Siparişi Kaydedilirken Hata", $"The product list cannot be empty!", orderDto);
                return ServiceResponse<CommonResponseDto>.Error("The product list cannot be empty!");
            }
            try
            {
                Logger.Information("YemekSepetiOrderService SaveOrderOnQp Request : {@request} ", fileName: _logFolderName, orderDto);

                long qpOrderSeqId = 0;
                bool sendToQp = _apiDefinition.OrderSendToQp;
                string merchantNo = PazarYeri.Yemeksepeti;
                var pyBirimTanimDto = await _pazarYeriBirimTanimDalService.GetStoreDetailAsync(merchantNo, orderDto.Client.StoreId);

                var orderSkus = orderDto.Items?
                                 .Select(item => item.Sku?.Trim())
                                 .Where(b => !string.IsNullOrWhiteSpace(b))
                                 .Distinct()
                                 .ToList() ?? [];

                var pyTransferredProductList = await _pazarYeriAktarimDalService.GetPyTransferredProductListAsync(merchantNo, pyBirimTanimDto.BirimNo, orderSkus);


                var orderProductMalNos = pyTransferredProductList.Select(x => x.MalNo).Distinct().ToList();
                var pyProductList = await _pazarYeriMalTanimDalService.GetPyProductsAsync(merchantNo, orderProductMalNos);


                var dbSaveResult = false;
                var hasOrder = await _pazarYeriSiparisDalService.OrderExistByIdAsync(orderDto.OrderId, merchantNo);

                if (orderDto.Status != OrderStatus.Received && !hasOrder)
                {
                    string errorMessage = "The order with {orderStatus} status was not found in the database. Order Id: {orderId}, Order Code: {orderNo} , External Order Id:{orderDto.ExternalOrderId}";
                    Logger.Error(errorMessage, fileName: _logFolderName, orderDto.Status, orderDto.OrderId, orderDto.OrderCode, orderDto.ExternalOrderId);
                    return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = $"Order not found in the database.So cannot update status to {orderDto.Status} | Order Id: {orderDto.OrderId}, Order Code: {orderDto.OrderCode}, External Order Id:{orderDto.ExternalOrderId}", Success = true });
                }
                else if (orderDto.Status == OrderStatus.Received && hasOrder)
                {
                    string errorMessage = "Duplicate request detected. The order with {orderStatus} status already found in the database. Order Id: {orderId}, Order Code: {orderNo}";
                    Logger.Warning(errorMessage, fileName: _logFolderName, orderDto.Status, orderDto.OrderId, orderDto.OrderCode);

                    return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto
                    {
                        Message = $"Duplicate request detected.The order with {orderDto.Status} status already found in the database."
                        ,
                        Success = true
                    });
                }
                else if (orderDto.Status == OrderStatus.Received && !hasOrder)
                {
                    if (orderDto.Items is null || !orderDto.Items.Any())
                    {
                        await SendFailedOrderMailFormattedAsync("YemekSepeti Siparişi Kaydedilirken Hata", $"no items found!", orderDto);
                        return ServiceResponse<CommonResponseDto>.Error($"no items found!");
                    }

                    //veritabanında olmayan üründen sipariş gelmiş ise uyarı veriyoruz
                    var missingOrderItems = CheckMissingOrderItems(pyTransferredProductList, orderDto);
                    if (missingOrderItems.Any())
                    {
                        await SendFailedOrderMailFormattedAsync("YemekSepeti Siparişi Kaydedilirken Hata", $"Order items not found in transferred products! SKUs: {string.Join(", ", missingOrderItems.Select(i => i.Sku))}", orderDto);
                        return ServiceResponse<CommonResponseDto>.Error($"Order items not found in transferred products! SKUs: {string.Join(", ", missingOrderItems.Select(i => i.Sku))}");
                    }

                    if (orderDto.Items.Any(f => f.Barcode?.Count() == 0))
                    {
                        string errorMessage = $"items with no barcode found!";
                        var itemsWithNoBarcode = orderDto.Items.Where(f => f.Barcode?.Count() == 0).Select(i => i);
                        errorMessage += $" - SKUs: {string.Join(", ", itemsWithNoBarcode.Select(i => i.Sku))}";
                        Logger.Warning(errorMessage, fileName: _logFolderName);

                        // yya | 2025-09-18 | workaround!
                        // yemeksepeti tarafı bu barkodu olmayan ürünleri gönderme sorununu çözene kadar ,
                        // biz bu olayı loglayıp SKU'dan barkodu kendimiz bulalım , sonra siparişleri boşuna iptal etmesinler.
                        foreach (var item in itemsWithNoBarcode)
                        {
                            var productBarcodes = await _quickPickDalService.GetProductBarcodesWithPriorityNumberAsync(item.Sku);
                            if (productBarcodes.Any())
                            {
                                item.Barcode = productBarcodes.Select(b => b.Barcode).ToArray();
                                Logger.Information($"Barcodes assigned for missing item. SKU: {item.Sku}, Barcode: {string.Join(", ", item.Barcode)}", fileName: _logFolderName);
                            }
                        }
                        bool isStillMissingBarcodes = itemsWithNoBarcode.Where(f => f.Barcode?.Count() == 0).Any();
                        if (isStillMissingBarcodes)
                        {
                            await SendFailedOrderMailFormattedAsync("YemekSepeti Siparişi Kaydedilirken Hata", errorMessage, orderDto);
                            return ServiceResponse<CommonResponseDto>.Error(errorMessage);
                        }
                        // yya | 2025-09-18 | workaround!
                    }

                    qpOrderSeqId = await _pazarYeriSiparisDalService.GetSeqId();
                    dbSaveResult = await SaveOrderDb(orderDto, qpOrderSeqId, pyProductList, pyTransferredProductList);
                }
                else if (orderDto.Status == OrderStatus.Cancelled && hasOrder)
                {
                    var orderDb = await _pazarYeriSiparisDalService.GetOrderWithOrderIdAsync(orderDto.OrderId, merchantNo);

                    if (orderDb == null || (orderDb.SevkiyatPaketDurumu == OrderStatus.Cancelled && orderDb.DepoAktarildiEH == Character.E))
                    {
                        Logger.Warning("Sipariş zaten iptal edilmiş. Siparis Numarası: {orderNumber}", _logFolderName, orderDb?.Id);
                        return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = $"Order has already been cancelled. Order Number: {orderDto.OrderId}", Success = true });
                    }
                    if (orderDb is not null && orderDb.Id > 0)
                    {
                        qpOrderSeqId = orderDb.Id;
                    }
                    if (!orderDb.SevkiyatPaketDurumu.Contains(OrderStatus.Cancelled))
                    {
                        orderDb.SevkiyatPaketDurumu = OrderStatus.Cancelled;
                        if (orderDb.DepoAktarildiEH == Character.E && sendToQp)
                        {
                            var response = await _qpClient.CancelOrderAsync(orderDb.Id.ToString(), "1");
                            if (response.Response)
                            {
                                Logger.Information("Order is cancelled. QP Success With ID: {qpOrderId}", fileName: _logFolderName, orderDb.Id);
                                await _pazarYeriSiparisDalService.UpdateOrderAsync(orderDb);
                                return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = "Order is canceled.", Success = true });
                            }
                            else
                            {
                                await SendFailedOrderMailFormattedAsync("YemekSepeti Siparişi İptal Edilirken Hata", $"Order is not cancelled. QP Error With ID: {orderDb.Id} response:{response.Message}", orderDto);
                                Logger.Error("Order is not cancelled. QP Error With ID: {qpOrderId} response:{response}", fileName: _logFolderName, orderDb.Id, response.Message);
                                return ServiceResponse<CommonResponseDto>.Error($"An error occurred while cancelling the order. Please try again later. Err:{response.Message}", httpStatusCode: HttpStatusCode.InternalServerError);
                            }
                        }
                        else
                        {
                            await _pazarYeriSiparisDalService.UpdateOrderAsync(orderDb);
                            Logger.Information("Order is cancelled without QP. Order ID: {orderId}", fileName: _logFolderName, orderDb.Id);
                            return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = "Order is canceled.", Success = true });
                        }
                    }
                    dbSaveResult = true;
                }
                else if (orderDto.Status == OrderStatus.ReadyForPickup && hasOrder)
                {
                    await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderDto.OrderId, nameof(StatusEnums.Collected), merchantNo);
                    string message = $"{orderDto.OrderCode} nolu sipariş {nameof(StatusEnums.Collected)} statüsüne güncellendi.";
                    Logger.Information(message, _logFolderName);
                    return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = message, Success = true });
                }
                else if (orderDto.Status == OrderStatus.Dispatched && hasOrder)
                {
                    await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderDto.OrderId, nameof(StatusEnums.InTransit), merchantNo);
                    string message = $"{orderDto.OrderCode} nolu sipariş {nameof(StatusEnums.InTransit)} statüsüne güncellendi.";
                    Logger.Information(message, _logFolderName);
                    return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = message, Success = true });
                }
                else if (orderDto.Status == OrderStatus.Delivered && hasOrder)
                {
                    await _pazarYeriSiparisDalService.UpdateOrderStatusAsync(orderDto.OrderId, nameof(StatusEnums.Delivered), merchantNo);
                    string message = $"{orderDto.OrderCode} nolu sipariş {nameof(StatusEnums.Delivered)} statüsüne güncellendi.";
                    Logger.Information(message, _logFolderName);
                    return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = message, Success = true });
                }
                else
                {
                    // unknown status
                    string errorMessage = $"Handled but unknown order status: {orderDto.Status}. Order Id: {orderDto.OrderId}, Order Code: {orderDto.OrderCode}";
                    Logger.Warning(errorMessage, _logFolderName);
                    return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = errorMessage, Success = true });
                }

                long orderCountTransferredToQP = await _pazarYeriSiparisDalService.GetOrderWareHouseTransferredCountAsync(orderDto.OrderCode, merchantNo);
                bool _sendQp = sendToQp && orderCountTransferredToQP < 1;
                if (!dbSaveResult)
                {
                    await SendFailedOrderMailFormattedAsync("YemekSepeti Siparişi Kaydedilirken Hata", $"An error occurred while saving the order.2 Please try again.", orderDto);
                    return ServiceResponse<CommonResponseDto>.Error("An error occurred while saving the order.2 Please try again.", httpStatusCode: HttpStatusCode.InternalServerError);
                }
                else if (_sendQp)
                {
                    var sendQpResult = await SendOrderToQp(pyBirimTanimDto, pyProductList, pyTransferredProductList, qpOrderSeqId, orderDto);

                    if (sendQpResult)
                    {
                        var orderItem = await _pazarYeriSiparisDalService.GetOrderByIdAsync(qpOrderSeqId);
                        if (orderItem is not null)
                        {
                            orderItem.DepoAktarildiEH = Character.E;
                            await _pazarYeriSiparisDalService.UpdateOrderAsync(orderItem);
                        }
                        return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = "Order Saved", Success = true });
                    }
                    else
                    {
                        return ServiceResponse<CommonResponseDto>.Error("An error occurred while saving the order. Please try again.", httpStatusCode: HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    Logger.Information("Sipariş işlemleri tamamlandı. Siparis Numarası: {orderNumber}", _logFolderName, orderDto.OrderCode);
                    return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = $"The order has been updated to in {orderDto.Status} status..", Success = true });
                }
            }
            catch (Exception ex)
            {
                await SendFailedOrderMailFormattedAsync("YemekSepeti Siparişi Kaydedilirken Hata", $"Unhandled Exception!", orderDto, ex);
                Logger.Error("YemekSepetiOrderService > SaveOrderOnQp Hata {exception} ", fileName: _logFolderName, ex);
                return ServiceResponse<CommonResponseDto>.Error(ex.Message, httpStatusCode: HttpStatusCode.InternalServerError);
            }
        }

        public Task<CommonResponseDto> ClaimStatuUpdate(PostProductReturnRequestDto claimDto)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Utilities

        private async Task<bool> SaveOrderDb(YemekSepetiOrderDto order, long qpOrderSeqId,
            IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts)
        {
            try
            {
                PazarYeriSiparis pazarYeriSiparis;
                string invoiceId = Guid.NewGuid().ToString();
                string shipmentAddressId = Guid.NewGuid().ToString();
                var merchantNo = PazarYeri.Yemeksepeti;
                pazarYeriSiparis = new PazarYeriSiparis()
                {
                    PazarYeriNo = merchantNo,
                    Id = qpOrderSeqId,
                    PaketId = order.ExternalOrderId,
                    SiparisId = order.OrderId,
                    SiparisNo = order.ExternalOrderId,
                    SiparisTarih = order.Sys?.CreatedAt.ToLocalTime(),
                    ToplamTutar = Convert.ToDecimal(order.Payment?.OrderTotal),
                    MusteriId = order.Customer.Id,
                    MusteriAdi = order.Customer.FirstName,
                    MusteriSoyadi = order.Customer.LastName,
                    MusteriEmail = string.Empty,
                    KargoTakipNo = string.Empty,
                    KargoGondericiNumarasi = string.Empty,
                    KargoSaglayiciAdi = order.TransportType,
                    SevkiyatPaketDurumu = order.Status,
                    KargoAdresId = shipmentAddressId,
                    FaturaAdresId = invoiceId,
                    KoliAdeti = 0,
                    Desi = 0,

                };
                var deliveryAddress = order.Customer.DeliveryAddress;
                PazarYeriKargoAdres pazarYeriKargoAdres = new()
                {
                    Id = qpOrderSeqId,
                    KargoAdresId = shipmentAddressId,
                    AdSoyad = $"{order.Customer.FirstName} {order.Customer.LastName}",
                    Ad = order.Customer.FirstName,
                    Soyad = order.Customer.LastName,
                    Sehir = deliveryAddress.City,
                    PostaKod = deliveryAddress.Zipcode,
                    Semt = deliveryAddress.Street
                };
                StringBuilder fullAddress = new();
                if (!string.IsNullOrEmpty(deliveryAddress.Street))
                {
                    fullAddress.Append($"Street: {deliveryAddress.Street}");
                }
                if (!string.IsNullOrEmpty(deliveryAddress.Number))
                {
                    fullAddress.Append($" No: {deliveryAddress.Number}");
                }
                if (!string.IsNullOrEmpty(deliveryAddress.Entrance))
                {
                    fullAddress.Append($" Entrance: {deliveryAddress.Entrance}");
                }
                if (!string.IsNullOrEmpty(deliveryAddress.Floor))
                {
                    fullAddress.Append($" Floor:  {deliveryAddress.Floor}");
                }
                if (!string.IsNullOrEmpty(deliveryAddress.Building))
                {
                    fullAddress.Append($" Building:  {deliveryAddress.Building}");
                }
                if (!string.IsNullOrEmpty(deliveryAddress.Apartment))
                {
                    fullAddress.Append($" Apartment:  {deliveryAddress.Apartment}");
                }
                if (!string.IsNullOrEmpty(deliveryAddress.Intercom))
                {
                    fullAddress.Append($" Intercom:  {deliveryAddress.Intercom}");
                }
                if (!string.IsNullOrEmpty(deliveryAddress.Suburb))
                {
                    fullAddress.Append($" Suburb:  {deliveryAddress.Suburb}");
                }
                if (!string.IsNullOrEmpty(deliveryAddress.Block))
                {
                    fullAddress.Append($" Block:  {deliveryAddress.Block}");
                }
                pazarYeriKargoAdres.TamAdres = fullAddress.ToString();
                PazarYeriFaturaAdres pazarYeriFaturaAdres = new()
                {
                    Id = qpOrderSeqId,
                    FaturaAdresId = invoiceId,
                    AdSoyad = $"{order.Customer.FirstName} {order.Customer.LastName}",
                    Adi = order.Customer.FirstName,
                    Sehir = order.Customer.DeliveryAddress.City,
                    PostaKod = order.Customer.DeliveryAddress.Zipcode,
                    Semt = order.Customer.DeliveryAddress.Street,
                    Firma = string.Empty,
                    TamAdres = fullAddress.ToString()
                };

                List<PazarYeriSiparisDetay> pazarYeriSiparisDetayList = new();
                List<PazarYeriSiparisUrun> pazarYeriSiparisUrunList = new();
                PazarYeriSiparisEkBilgi packagingInfo = new();
                SachetProduct sachetProduct = _apiDefinition.SachetProducts.FirstOrDefault();
                foreach (var itemProduct in order.Items)
                {
                    if (itemProduct.Sku == sachetProduct?.ProductCode)
                    {
                        packagingInfo.PosetSayisi = itemProduct.OriginalPricing.Quantity;
                        packagingInfo.PosetTutari = itemProduct.OriginalPricing.TotalPrice;
                        packagingInfo.PySiparisNo = order.OrderCode;
                        packagingInfo.ObaseSiparisId = qpOrderSeqId;

                    }
                    var transferProduct = transferProducts.FirstOrDefault(x => itemProduct.Sku == x.PazarYeriMalNo && x.PazarYeriBirimNo == order.Client.StoreId);
                    var product = transferProduct is not null ? products.FirstOrDefault(x => x.MalNo.Trim() == transferProduct?.MalNo?.Trim()) : null;

                    if (pazarYeriSiparisUrunList.Exists(x => x.PazarYeriMalNo == transferProduct?.PazarYeriMalNo))
                    {
                        pazarYeriSiparisUrunList.First(x => x.PazarYeriMalNo == transferProduct?.PazarYeriMalNo).Miktar += (itemProduct.OriginalPricing.PricingType == YemekSepetiConstants.ProductType.KG ? Convert.ToDecimal(itemProduct.OriginalPricing.Weight) : itemProduct.OriginalPricing.Quantity);
                    }
                    else
                    {
                        pazarYeriSiparisUrunList.Add(new PazarYeriSiparisUrun()
                        {
                            ObaseSiparisId = qpOrderSeqId,
                            PySiparisNo = order.OrderCode,
                            ObaseMalNo = product?.MalNo ?? transferProduct?.MalNo,
                            PazarYeriMalNo = transferProduct?.PazarYeriMalNo,
                            AltUrunObaseMalNo = string.Empty,
                            AltUrunPazarYeriMalNo = string.Empty,
                            Miktar = itemProduct.OriginalPricing.PricingType == ProductType.KG ? (decimal)itemProduct.OriginalPricing.Weight : itemProduct.OriginalPricing.Quantity,
                            IsAlternativeEH = Character.H,
                            PazarYeriBirimId = order.Client.StoreId,
                            MinMiktar = itemProduct.OriginalPricing.MinQuantity,
                            MaxMiktar = itemProduct.OriginalPricing.MaxQuantity,
                            ImageUrl = itemProduct.ImageUrl,
                        });
                    }

                    pazarYeriSiparisDetayList.Add(new PazarYeriSiparisDetay()
                    {
                        Id = qpOrderSeqId,
                        LineItemId = itemProduct.Id,
                        PaketItemId = itemProduct.Id,
                        PazarYeriBirimId = order.Client.StoreId,
                        ObaseMalNo = product?.MalNo ?? transferProduct?.MalNo,
                        PazarYeriMalNo = transferProduct?.PazarYeriMalNo,
                        PazarYeriUrunKodu = string.Empty,
                        PazarYeriMalAdi = itemProduct.Name,
                        AlternatifUrunEH = Character.H,
                        Barkod = itemProduct.Barcode.Any() ? string.Join(',', itemProduct.Barcode) : itemProduct.Barcode[0],
                        Miktar = itemProduct.OriginalPricing.PricingType == ProductType.KG ? (decimal)itemProduct.OriginalPricing.Weight : itemProduct.OriginalPricing.Quantity,
                        NetTutar = itemProduct.OriginalPricing.TotalPrice,
                        BrutTutar = itemProduct.OriginalPricing.TotalPrice,
                        KdvOran = itemProduct.OriginalPricing.VatPercent,
                        SatisKampanyaId = string.Empty,
                        UrunBoyutu = string.Empty,
                        UrunRengi = string.Empty,
                        SiparisUrunDurumAdi = string.Empty,
                        IsCancelledEH = Character.H,
                        IsAlternativeEH = Character.H,
                        IsCollectedEH = Character.H,
                        Weight = itemProduct.OriginalPricing.PricingType == ProductType.KG ? Convert.ToDecimal(itemProduct.OriginalPricing?.Weight) : null,
                    });
                }

                #region Db Save

                await _transactionDalService.BeginTransactionAsync();
                await _pazarYeriSiparisDalService.AddOrderAsync(pazarYeriSiparis);
                await _pazarYeriSiparisDetayDalService.AddOrderDetailsAsync(pazarYeriSiparisDetayList);

                //siparişte poşet yoksa PAZAR_YERI_SIPARIS_EK_BILGI tablosuna eklemeye gerek yok.
                if (packagingInfo.ObaseSiparisId > 0)
                    await _pazarYeriSiparisEkBilgiDalService.AddAdditionalDataAsync(packagingInfo);

                if (pazarYeriSiparisUrunList is not null)
                {
                    var duplicates = pazarYeriSiparisUrunList
                        .GroupBy(p => new { p.ObaseSiparisId, p.ObaseMalNo, p.PazarYeriMalNo, p.PySiparisNo })
                        .Where(g => g.Count() > 1)
                        .ToList();

                    if (duplicates.Count > 0)
                    {
                        foreach (var duplicate in duplicates)
                        {
                            var key = duplicate.Key;
                            Logger.Error(
                                "SaveOrderDb - PazarYeriSiparisUrun Duplicate product  ObaseSiparisId: {obaseSipId}  ObaseMalNo:{malNo} PazarYeriMalNo:{pazarYeriMalNo} PySiparisNo:{pySipNo}",
                                fileName: _logFolderName,
                                null,
                                key.ObaseSiparisId,
                                key.ObaseMalNo,
                                key.PazarYeriMalNo,
                                key.PySiparisNo
                            );
                        }
                    }

                    await _pazarYeriSiparisUrunDalService.AddProductsAsync(pazarYeriSiparisUrunList.DistinctBy(x => new
                    {
                        x.ObaseSiparisId,
                        x.ObaseMalNo,
                        x.PazarYeriMalNo,
                        x.PySiparisNo
                    }).ToList());
                }
                if (pazarYeriFaturaAdres is not null)
                {
                    await _pazarYeriFaturaAdresDalService.AddInvoiceAddressAsync(pazarYeriFaturaAdres);
                }
                if (pazarYeriKargoAdres is not null)
                {
                    await _pazarYeriKargoAdresDalService.AddShipmentAddressAsync(pazarYeriKargoAdres);
                }
                await _transactionDalService.CommitTransactionAsync();

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                await SendFailedOrderMailFormattedAsync("YemekSepeti Siparişi Kaydedilirken Hata", $"An error occurred while saving order to DB.", order, ex);
                Logger.Error("SaveOrderDb Exception {exception} for Order: {order}", fileName: _logFolderName, ex, order.OrderCode);
                await _transactionDalService.RollbackTransactionAsync();
                return false;
            }
        }

        private async Task<bool> SendOrderToQp(PazarYeriBirimTanim pyBirimTanim, IEnumerable<PazarYeriMalTanim> pyProductList, IEnumerable<PazarYeriAktarim> pyTransferredProductList, long qpOrderSeqId, YemekSepetiOrderDto orderDto)
        {

            bool isSendToQp = false;
            try
            {

                YemekSepetiOrderDto orderRequest = orderDto;
                var qpModel = _orderConvertService.ToQpOrder(orderDto, pyBirimTanim, pyProductList, pyTransferredProductList, qpOrderSeqId, merchantNo: PazarYeri.Yemeksepeti, sachetProduct: _apiDefinition.SachetProducts);

                QPService.DeliveryRequest consumeDeliveryRequest = new() { DeliveryAction = qpModel };
                Logger.Information("Request sent to QP:{@request} ", fileName: _logFolderName, qpModel);

                var pipeline = new ResiliencePipelineBuilder()
                    .AddRetry(new RetryStrategyOptions
                    {
                        ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
                        MaxRetryAttempts = 3,
                        Delay = TimeSpan.FromSeconds(5),
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        OnRetry = args =>
                        {
                            Logger.Warning("YemekSepeti - SendOrderToQp - Retry attempt {Attempt} will be made after {Delay}s. Reason: {Message}", fileName: _logFolderName, args.AttemptNumber.ToString(), args.RetryDelay.TotalSeconds.ToString("F1"), args.Outcome.Exception?.Message ?? "Unknown error");
                            return ValueTask.CompletedTask;
                        }
                    })
                    .Build();

                await pipeline.ExecuteAsync(async cancellationToken =>
                {
                    try
                    {
                        var qpResponse = await _qpClient.ConsumeDeliveryAsync(consumeDeliveryRequest);
                        string consumeDeliveryResult = qpResponse.ConsumeDeliveryResult.Trim();
                        Logger.Information("Reponse ConsumeDeliveryAsync to QP:{qpResponse} ", fileName: _logFolderName, consumeDeliveryResult);

                        if (string.Equals(consumeDeliveryResult, "1") || consumeDeliveryResult.Contains("BURST REQUEST DETECTED!", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Information("QP Success With Id: {qpOrderSeqId}.", fileName: _logFolderName, qpOrderSeqId);
                            isSendToQp = true;
                            return;
                        }

                        string errorMessage = $"Sipariş Quickpick'e aktarılırken bir hata oluştu.Sipariş No: {orderDto.OrderCode}. Hata:{consumeDeliveryResult}";
                        Logger.Error("QP Error With Id: {qpOrderSeqId}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, errorMessage);
                        bool skipError = false;
                        if (_appSetting.Value.QpOrderErrorMessages is not null)
                        {
                            foreach (var item in _appSetting.Value.QpOrderErrorMessages)
                            {
                                if (errorMessage.Contains(item))
                                {
                                    skipError = true;
                                    break;
                                }
                            }
                        }
                        Logger.Information("skipError {SkipError}", fileName: _logFolderName, skipError);
                        if (!skipError)
                        {
                            throw new InvalidOperationException($"YemekSepeti - SendOrderToQp - Sipariş Quickpick'e aktarılırken bir hata oluştu ve tekrar gönderim sağlanacak. QP Response :{errorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("An exception occurred: {exception}", fileName: _logFolderName, ex);
                        throw new InvalidOperationException($"YemekSepeti - SendOrderToQp - Sipariş Quickpick'e aktarılırken bir hata oluştu ve tekrar gönderim sağlanacak. Error :{ex.Message + " " + ex.InnerException?.Message ?? ""} ");
                    }
                });
            }
            catch (Exception ex)
            {
                await SendFailedOrderMailFormattedAsync("YemekSepeti Siparişi Quickpick'e iletilemedi", $"An error occurred while sending order to QP.", orderDto, ex);
                Logger.Error("QP Error With Id: {qpOrderSeqId} For Order Number: {orderNumber}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, orderDto.OrderCode, ex);
            }
            return isSendToQp;
        }

        private List<Domain.Dtos.YemekSepeti.Item> CheckMissingOrderItems(IEnumerable<PazarYeriAktarim> pyTransferredProductList, YemekSepetiOrderDto orderDto)
        {

            //veritabanında olmayan üründen sipariş gelmiş ise uyarı veriyoruz
            var transferredSkus = new HashSet<string>(pyTransferredProductList.Select(p => p.MalNo));

            var missingOrderItems = orderDto.Items
                .Where(i => !transferredSkus.Contains(i.Sku))
                .ToList();

            return missingOrderItems;
        }

        private async Task SendFailedOrderMailFormattedAsync(string subject, string message, YemekSepetiOrderDto? orderDto = null, Exception? ex = null)
        {
            try
            {
                string body = "<table>";

                body += $"<tr><td>Tarih:</td><td>{DateTime.Now}</td></tr>";

                if (orderDto is not null)
                {
                    body += $"<tr><td>Order Id:</td><td>{orderDto.OrderId}</td></tr>";
                    body += $"<tr><td>Order Code:</td><td>{orderDto.OrderCode}</td></tr>";
                    body += $"<tr><td>External Order Id:</td><td>{orderDto.ExternalOrderId}</td></tr>";
                }

                body += $"<tr><td>Hata:</td><td>{message}</td></tr>";

                if (ex is not null)
                {
                    body += $"<tr><td>Exception Message:</td><td>{ex.Message}</td></tr>";
                    body += $"<tr><td>Inner Exception:</td><td>{ex.InnerException?.Message}</td></tr>";
                    body += $"<tr><td>Stack Trace:</td><td>{ex.StackTrace}</td></tr>";
                }

                body += "</table>";

                await SendFailedOrderMail(subject, body);
            }
            catch (Exception exc)
            {
                Logger.Error("An exception occurred while sending failed order email: {exception}", fileName: _logFolderName, exc);
            }
        }

        private string ParseFaultResponseMessageUserFriendly(string response, bool isSuccessStatusCode)
        {
            try
            {
                if (isSuccessStatusCode)
                    return response;

                if (string.IsNullOrWhiteSpace(response))
                    return "OBASE Parser: Boş response!";

                if (_apiDefinition.UsingApinizer.HasValue && _apiDefinition.UsingApinizer.Value)
                {
                    Domain.Dtos.CommonDtos.ApinizerFaultWrapperDto? faultWrapperDto = Newtonsoft.Json.JsonConvert.DeserializeObject<Domain.Dtos.CommonDtos.ApinizerFaultWrapperDto>(response);

                    if (faultWrapperDto?.Fault == null)
                        return $"OBASE Parser: Bozuk json içerik! (Apinizer) <br /> Response: {response}";

                    var faultResponseDto = faultWrapperDto.Fault;
                    return $"Hata Kodu: {faultResponseDto.FaultCode} - Durum Kodu: {faultResponseDto.FaultStatusCode} <br /> Hata Mesajı: {faultResponseDto.FaultString} <br /> ApiResponse: {faultResponseDto.ResponseFromApi}";
                }
                else
                {
                    YemekSepetiFaultWrapperDto? faultWrapperDto = Newtonsoft.Json.JsonConvert.DeserializeObject<YemekSepetiFaultWrapperDto>(response);

                    if (faultWrapperDto == null)
                        return $"OBASE Parser: Bozuk json içerik! (YS) <br /> Response: {response}";

                    return $"Hata Kodu: {faultWrapperDto.Code} <br /> Hata Mesajı: {faultWrapperDto.Message} <br /> ApiResponse: {response}";
                }

            }
            catch (Exception exc)
            {
                Logger.Error("An exception occurred while parsing response message: {exception}", fileName: _logFolderName, exc);
                return response;
            }

        }

        #endregion


    }
}