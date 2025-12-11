using Hangfire.Server;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Client.Concrete;
using OBase.Pazaryeri.Business.Helper;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Core.Abstract.Repository;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Constants;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Domain.Helper;
using Polly;
using Polly.Retry;
using RestEase;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using static OBase.Pazaryeri.Domain.Helper.CommonHelper;

namespace OBase.Pazaryeri.Business.Services.Concrete.Order
{
    public class HepsiExpressOrderService : BaseService, IHepsiExpressOrderService
    {
        #region Variables

        private readonly IOptions<AppSettings> _appSettings;
        private readonly IPazarYeriBirimTanimDalService _pazarYeriBirimTanimDalService;
        private readonly IPazarYeriMalTanimDalService _pazarYeriMalTanimDalService;
        private readonly IPazarYeriAktarimDalService _pazarYeriAktarimDalService;
        private readonly IPazarYeriSiparisUrunDalService _pazarYeriSiparisUrunDalService;
        private readonly IPazarYeriSiparisEkBilgiDalService _pazarYeriSiparisEkBilgiDalService;
        private readonly IPazarYeriKargoAdresDalService _pazarYeriKargoAdresDalService;
        private readonly IPazarYeriFaturaAdresDalService _pazarYeriFaturaAdresDalService;
        private readonly ITransactionDalService _transactionDalService;
        private IHepsiExpressOrderClient _hepsiExpressOrderClient;

        private readonly ApiDefinitions _apiDefinition;
        private readonly QPService.OrderDeliveryServiceSoapClient _qpClient;
        string _logFolderName = Enum.GetName(typeof(PazarYerleri), PazarYerleri.HepsiExpress);
        private readonly IOrderConvertService _orderConvertService;
        #endregion

        #region Ctor

        public HepsiExpressOrderService(IOptions<AppSettings> appSetting, IMailService mailService, IPazarYeriBirimTanimDalService pazarYeriBirimTanimDalService = null, IPazarYeriMalTanimDalService pazarYeriMalTanimDalService = null,
            IPazarYeriSiparisDalService pazarYeriSiparisDalService = null, IPazarYeriAktarimDalService pazarYeriAktarimDalService = null, IPazarYeriSiparisDetayDalService pazarYeriSiparisDetayDalService = null,
            IPazarYeriSiparisUrunDalService pazarYeriSiparisUrunDalService = null, IPazarYeriSiparisEkBilgiDalService pazarYeriSiparisEkBilgiDalService = null, IPazarYeriKargoAdresDalService pazarYeriKargoAdresDalService = null,
            IPazarYeriFaturaAdresDalService pazarYeriFaturaAdresDalService = null, ITransactionDalService transactionDalService = null, IHepsiExpressOrderClient hepsiExpressOrderClient = null, IOrderConvertService orderConvertService = null) : base(pazarYeriSiparisDalService, pazarYeriSiparisDetayDalService, appSetting, mailService)
        {
            _appSettings = appSetting;
            _apiDefinition = _appSettings.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.HepsiExpress);
            _pazarYeriBirimTanimDalService = pazarYeriBirimTanimDalService;
            _pazarYeriMalTanimDalService = pazarYeriMalTanimDalService;
            _pazarYeriAktarimDalService = pazarYeriAktarimDalService;
            _pazarYeriSiparisUrunDalService = pazarYeriSiparisUrunDalService;
            _pazarYeriSiparisEkBilgiDalService = pazarYeriSiparisEkBilgiDalService;
            _pazarYeriKargoAdresDalService = pazarYeriKargoAdresDalService;
            _pazarYeriFaturaAdresDalService = pazarYeriFaturaAdresDalService;
            _transactionDalService = transactionDalService;
            if (_appSettings.Value.WareHouseUrl is not null)
            { _qpClient = new QPService.OrderDeliveryServiceSoapClient(QPService.OrderDeliveryServiceSoapClient.EndpointConfiguration.OrderDeliveryServiceSoap12, remoteAddress: _appSettings.Value.WareHouseUrl); }
            _hepsiExpressOrderClient = hepsiExpressOrderClient;
            _orderConvertService = orderConvertService;
        }

        #endregion

        #region Methods

        public async Task SaveOrderOnQp(HEOrderDto order)
        {
            var dbSaveResult = false;
            var firstOrderItem = order.Items.FirstOrDefault();
            long qpOrderSeqId = 0;
            bool sendToQp = _apiDefinition.OrderSendToQp;
            string merchantNo = PazarYeri.HepsiExpress;
            var pyBirimTanimDto = await _pazarYeriBirimTanimDalService.GetStoreDetailAsync(merchantNo, firstOrderItem.MerchantId.ToString());
            var pyProductList = await _pazarYeriMalTanimDalService.GetPyProductsAsync(merchantNo);
            var pyTransferredProductList = await _pazarYeriAktarimDalService.GetPyTransferredProductsAsync(merchantNo, firstOrderItem.MerchantId.ToString());

            string orderId = firstOrderItem.Id;
            order.Order.Id = orderId;
            if (firstOrderItem is not null && firstOrderItem.Status != HepsiExpressOrderStatus.Open && !await _pazarYeriSiparisDalService.OrderExistAsync(orderId, merchantNo))
            {
                Logger.Warning("The canceled order from Getir could not be found in the db. for Order Id: {orderId}, Order Number: {orderNo}", fileName: _logFolderName, orderId, firstOrderItem.OrderNumber);
                return;
            }
            else if (firstOrderItem is not null && firstOrderItem.Status == HepsiExpressOrderStatus.Open && !await _pazarYeriSiparisDalService.OrderExistAsync(orderId, merchantNo))
            {
                qpOrderSeqId = await _pazarYeriSiparisDalService.GetSeqId();
                dbSaveResult = await SaveOrderDb(order, qpOrderSeqId);

            }
            else if (firstOrderItem is not null && firstOrderItem.Status != HepsiExpressOrderStatus.Open && await _pazarYeriSiparisDalService.OrderExistAsync(orderId, merchantNo))
            {
                var orderDb = await _pazarYeriSiparisDalService.GetOrderWithOrderIdAsync(orderId, merchantNo);
                if (orderDb.SevkiyatPaketDurumu != HepsiExpressOrderStatus.Cancelled)
                {
                    orderDb.SevkiyatPaketDurumu = firstOrderItem.Status;
                    if (orderDb.DepoAktarildiEH == Character.E && sendToQp)
                    {
                        var response = await _qpClient.CancelOrderAsync(orderDb.Id.ToString(), "1");
                        if (response.Response)
                        {
                            Logger.Information("Order is cancelled. QP Success With ID: {qpOrderId}", fileName: _logFolderName, orderDb.Id);
                        }
                        else
                        {
                            Logger.Information("Order is not cancelled. QP Error With ID: {qpOrderId} response:{response}", fileName: _logFolderName, orderDb.Id, response.Message);
                        }
                    }
                    await _pazarYeriSiparisDalService.UpdateOrderAsync(orderDb);
                }
            }
            else
            {
                qpOrderSeqId = await _pazarYeriSiparisDalService.GetOrderIdByIdAsync(orderId, merchantNo);
                dbSaveResult = true;
            }
            long orderCountTransferredToQP = await _pazarYeriSiparisDalService.GetOrderWareHouseTransferredCountAsync(firstOrderItem.OrderNumber, merchantNo);
            bool _sendQp = sendToQp && (orderCountTransferredToQP < 1 ? true : false);
            if (_sendQp && dbSaveResult)
            {
                var sendQpResult = await SendOrderToQp(pyBirimTanim: pyBirimTanimDto, pyProductList: pyProductList, pyTransferredProductList: pyTransferredProductList, qpOrderSeqId: qpOrderSeqId, orderDto: order, sachetProducts:_appSettings.Value.SachetProducts);
                if (sendQpResult)
                {
                   
                    var orderItem = await _pazarYeriSiparisDalService.GetOrderByIdAsync(qpOrderSeqId);
                    if (orderItem is not null)
                    {
                        orderItem.DepoAktarildiEH = Character.E;
                        await _pazarYeriSiparisDalService.UpdateOrderAsync(orderItem);
                    }
                }               
            }
        }

        public async Task<CommonResponseDto> OrderUpdatePackageStatus(OrderStatuUpdateRequestDto orderDto, PazarYeriSiparis orderEntity)
        {
            Logger.Information("OrderUpdatePackageStatus Request :{@request} ", fileName: _logFolderName, orderDto);
            try
            {
                string supplierId = _apiDefinition.SupplierId;
                bool enableHEServices = _appSettings.Value.EnableMarketPlaceServices;
                var status = orderDto.Status;
                string merchantId = orderDto.MerchantId;
                IEnumerable<PazarYeriSiparisDetay> orderItems;
                IEnumerable<PazarYeriSiparisDetay> orderDetailsByObaseMalNo;
                string lineItemId = "";

                if (!orderDto.ProductQuantities.Any())
                {
                    return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "En Az Bir Ürün Göndermelisiniz");
                }

                IEnumerable<PazarYeriSiparisDetay> marketPlaceOrderLineItems = new List<PazarYeriSiparisDetay>();
                IEnumerable<PazarYeriSiparisUrun> mpOrderProducts = new List<PazarYeriSiparisUrun>();
                var packageId = orderDto.PackageId;
                switch (status)
                {
                    #region Picking
                    case StatusEnums.Picking:
                        {
                            if (enableHEServices)
                            {
                                Logger.Information("OrderUpdatePackageStatus => PutAsPicked Request for MerchantId: {merchantId} and OrderId: {pyOrderNo}", fileName: _logFolderName, orderDto.MerchantId, orderDto.SiparisNo);
                                var heResponse = await _hepsiExpressOrderClient.PutAsPicked(orderDto.MerchantId, orderDto.SiparisNo);
                                Logger.Information("OrderUpdatePackageStatus => PutAsPicked Response StatusCode : {statusCode}, Response : {@response}", fileName: _logFolderName, heResponse.ResponseMessage.StatusCode, heResponse?.GetContent());
                                await UpdateOrderWithOrderDetails(StatusEnums.Picking.ToString(), heResponse.ResponseMessage.IsSuccessStatusCode, null, orderDto.OrderId, heResponse.StringContent);
                                return OrderHelper.ReturnQPResponseV2(heResponse.ResponseMessage.StatusCode, _logFolderName, CalcSuccessFromHTTPStatus(heResponse.ResponseMessage.StatusCode), heResponse.StringContent);
                            }
                            else
                            {
                                await UpdateOrderWithOrderDetails(StatusEnums.Picking.ToString(), true, null, orderDto.OrderId, string.Empty);
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için HepsiExpress üzerinde işlem yapılmamıştır.");
                            }
                        }
                    #endregion

                    #region Collected
                    case StatusEnums.Collected:
                        {
                            orderDetailsByObaseMalNo = await _pazarYeriSiparisDetayDalService.GetOrderDetailsByOrderIdProductNoAsync(orderDto.OrderId, orderDto.ProductQuantities.Select(x => x.ProductId).ToList());

                            List<string> cancelledItemList = new List<string>();
                            foreach (ProductQuantity productQuantity in orderDto.ProductQuantities)
                            {
                                bool isItemCancelled = _pazarYeriSiparisDetayDalService.IsItemCancelled(productQuantity.ProductId, orderDto.OrderId);
                                if (isItemCancelled) cancelledItemList.Add("Ürün : " + productQuantity.ProductId + "-");
                            }
                            if (cancelledItemList.Any())
                            {
                                string message = "OrderUpdatePackageStatus => Collected Warning > Siparişte bulunan aşağıdaki ürünler iptal edildiğinden toplama tamamlanamıyor.Lütfen iptal olan ürünleri çıkartıp tekrar deneyiniz.{cancelledItemList}";

                                Logger.Warning(message, _logFolderName, string.Join("\n", cancelledItemList));
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.Conflict, _logFolderName, false, message);
                            }

                            if (cancelledItemList.Count == orderDto.ProductQuantities.Count)
                            {
                                string message = "OrderUpdatePackageStatus => Collected Warning > OrderId: {pyOrderNo} Bu sipariş iptal edildiğinden toplama tamamlanamıyor.";
                                Logger.Warning(message, fileName: _logFolderName, orderDto.SiparisNo);
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.Conflict, _logFolderName, false, message);
                            }

                            if (orderDetailsByObaseMalNo.Any())
                            {
                                HECompleteOrderRequestDto hepsiExpressCompleteOrderReqDto = new HECompleteOrderRequestDto()
                                {
                                    Deci = orderDto.Deci,
                                    ParcelQuantity = orderDto.ParcelQuantity,
                                };
                                var orderDetails = await GetOrderDetailsV2(orderDto.MerchantId, orderDto.SiparisNo);

                                foreach (var productQuantity in orderDto.ProductQuantities)
                                {
                                    var hasAltProduct = orderDto.ProductQuantities.FirstOrDefault(x => x.AltProductId == productQuantity.ProductId);

                                    if (hasAltProduct == null && productQuantity.Quantity == 0)
                                    {
                                        var productCancelReasonId = GetHexReasonId(productQuantity.ReasonId);
                                        var productCancelReasonRequest = new HEReasonDto() { ReasonId = productCancelReasonId };
                                        lineItemId = orderDetailsByObaseMalNo.FirstOrDefault(x => x.ObaseMalNo == productQuantity.ProductId)?.LineItemId;
                                        //LineItem Bazlı İptal İşlemi
                                        if (!string.IsNullOrEmpty(lineItemId))
                                        {

                                            Logger.Information("OrderUpdatePackageStatus => Collected-CancelOrderByLineItemId Request for MerchantId: {merchantId} and OrderId: {pyOrderNo} request: @request", fileName: _logFolderName, orderDto.MerchantId, orderDto.SiparisNo, productCancelReasonRequest);
                                            var productCancelReasonResponse = await _hepsiExpressOrderClient.CancelOrderByLineItemId(orderDto.MerchantId, lineItemId, productCancelReasonRequest);
                                            Logger.Information("OrderUpdatePackageStatus => => Collected-CancelOrderByLineItemId Response StatusCode : {statusCode}, Response : {@response}", fileName: _logFolderName, productCancelReasonResponse.ResponseMessage.StatusCode, productCancelReasonResponse?.GetContent());

                                            if (productCancelReasonResponse.ResponseMessage.StatusCode != HttpStatusCode.Conflict)
                                            {
                                                await UpdateOrderWithOrderDetails(nameof(StatusEnums.CancelledPart), productCancelReasonResponse.ResponseMessage.IsSuccessStatusCode, productQuantity.ReasonId, orderDto.OrderId, productCancelReasonResponse.StringContent, lineItemIdList: new List<string>() { lineItemId });
                                            }
                                        }
                                        continue;
                                    }

                                    if (productQuantity.Quantity == 0) continue;

                                    lineItemId = orderDetailsByObaseMalNo.FirstOrDefault(x => x.ObaseMalNo == productQuantity.ProductId)?.LineItemId;

                                    var itemDetail = orderDetails.Items.FirstOrDefault(x => x.Id == lineItemId);

                                    var addedProduct = itemDetail == null ? await _pazarYeriMalTanimDalService.GetProductMarketPlaceIdByObaseProductIdAsync(productQuantity.ProductId, PazarYeri.HepsiExpress) : null;
                                    var merchantProductNo = itemDetail != null ? itemDetail.Sku : addedProduct;
                                    var productDetailList = await _pazarYeriMalTanimDalService.GetProductDetailsAsync(PazarYeri.HepsiExpress, merchantProductNo);
                                    var unitProduct = productDetailList?.FirstOrDefault();
                                    decimal productQuantityInOrder = 0;
                                    decimal calculatedRelativeQuantity = 0;

                                    if (orderDetailsByObaseMalNo.FirstOrDefault(x => x.ObaseMalNo == unitProduct.MalNo) != null)
                                    {
                                        calculatedRelativeQuantity = orderDetailsByObaseMalNo.FirstOrDefault(x => x.ObaseMalNo == unitProduct.MalNo).Miktar;
                                    }
                                    else if (unitProduct.PyUrunSatisBirim.Trim() != CommonConstants.KG)
                                    {
                                        calculatedRelativeQuantity = (long)productQuantity.Quantity;
                                    }
                                    else if (unitProduct.PyUrunSatisBirim.Trim() == CommonConstants.KG)
                                    {
                                        calculatedRelativeQuantity = (long)(productQuantity.Quantity / (long)(productDetailList.FirstOrDefault(x => x.MalNo == productQuantity.ProductId).PyUrunSatisDeger * 1000));
                                    }

                                    if (unitProduct.PyUrunSatisBirim.Trim() == CommonConstants.KG)
                                    {
                                        productQuantityInOrder = calculatedRelativeQuantity * unitProduct.PyUrunSatisDeger.Value;
                                    }
                                    else
                                    {
                                        productQuantityInOrder = itemDetail?.Quantity ?? productQuantity.Quantity;
                                    }
                                    if (productQuantityInOrder != productQuantity.Quantity || productQuantity.AltProductId != null)
                                    {
                                        HEChangeOrderDetailsDto hepsiExpressChangeOrderDetailsDto = new HEChangeOrderDetailsDto()
                                        {
                                            NewSku = itemDetail != null ? itemDetail?.Sku : addedProduct
                                        };

                                        if (itemDetail == null)
                                        {
                                            lineItemId = lineItemId == null ? orderDetailsByObaseMalNo.FirstOrDefault(x => x.ObaseMalNo == orderDto.ProductQuantities.FirstOrDefault(x => x.ProductId == productQuantity.AltProductId).ProductId)?.LineItemId : lineItemId;

                                            hepsiExpressChangeOrderDetailsDto.NewLineItemId = lineItemId;
                                        }
                                        if (unitProduct.PyUrunSatisBirim.Contains(CommonConstants.KG))
                                        {
                                            hepsiExpressChangeOrderDetailsDto.NewGram = (long)(productQuantity.Quantity * 1000);
                                        }
                                        else
                                        {
                                            hepsiExpressChangeOrderDetailsDto.NewQuantity = (long)productQuantity.Quantity;
                                        }

                                        productQuantityInOrder = unitProduct.PyUrunSatisBirim.Trim() == CommonConstants.KG ? 1 : productQuantity.Quantity;

                                        Logger.Information("OrderUpdatePackageStatus => Collected-ChangeHexOrder Request for MerchantId: {merchantId} and OrderId: {pyOrderNo} request: @request", fileName: _logFolderName, orderDto.MerchantId, orderDto.SiparisNo, hepsiExpressChangeOrderDetailsDto);
                                        var changeHexOrderResponse = await _hepsiExpressOrderClient.ChangeHexOrder(orderDto.MerchantId, lineItemId, hepsiExpressChangeOrderDetailsDto);
                                        Logger.Information("OrderUpdatePackageStatus => => Collected-ChangeHexOrder Response StatusCode : {statusCode}, Response : {@response}", fileName: _logFolderName, changeHexOrderResponse.ResponseMessage.StatusCode, changeHexOrderResponse?.GetContent());


                                        if (changeHexOrderResponse.ResponseMessage.IsSuccessStatusCode && productQuantity.Quantity != 0)
                                        {
                                            var newLineItemIdReturnModel = JsonConvert.DeserializeObject<HEChangeOrderResponseDto>(changeHexOrderResponse.StringContent);
                                            var newLineItemId = newLineItemIdReturnModel.NewLineItemId;

                                            if (itemDetail != null)
                                            {
                                                hepsiExpressCompleteOrderReqDto.LineItemRequests.Add(new LineItemRequest() { Id = Guid.Parse(newLineItemId), Quantity = (long)productQuantityInOrder });
                                                var detail = await _pazarYeriSiparisDetayDalService.GetOrderDetailByLineItemIdAsync(lineItemId);
                                                var amount = itemDetail.UnitPrice.Amount;
                                                var quantity = hepsiExpressChangeOrderDetailsDto.NewQuantity;
                                                if (detail is not null)
                                                {
                                                    detail.LineItemId = newLineItemId;
                                                    detail.PaketItemId = newLineItemId;
                                                    detail.NetTutar = amount > 0 ? (amount * quantity) : detail.NetTutar;
                                                    detail.ParaBirimiKodu = itemDetail.UnitPrice.Currency;
                                                    detail.Miktar = quantity;
                                                    if (string.IsNullOrEmpty(hepsiExpressChangeOrderDetailsDto.NewMerchantSku))
                                                    {
                                                        detail.PazarYeriMalNo = hepsiExpressChangeOrderDetailsDto.NewSku;
                                                    }
                                                    if (string.IsNullOrEmpty(hepsiExpressChangeOrderDetailsDto.NewSku))
                                                    {
                                                        detail.ObaseMalNo = hepsiExpressChangeOrderDetailsDto.NewMerchantSku;
                                                    }
                                                    await _pazarYeriSiparisDetayDalService.UpdateOrderDetailAsync(detail);
                                                }
                                            }
                                            else
                                            {
                                                hepsiExpressCompleteOrderReqDto.LineItemRequests.Add(new LineItemRequest() { Id = Guid.Parse(newLineItemId), Quantity = (long)productQuantityInOrder });

                                                var transferProduct = await _pazarYeriAktarimDalService.GetProductsByProductNoAsync(new List<string> { unitProduct.MalNo }, PazarYeri.HepsiExpress);

                                                var detail = await _pazarYeriSiparisDetayDalService.GetOrderDetailByLineItemIdAsync(lineItemId);
                                                var amount = transferProduct.FirstOrDefault().SatisFiyat;
                                                var quantity = hepsiExpressChangeOrderDetailsDto.NewQuantity;
                                                if (detail is not null)
                                                {
                                                    detail.LineItemId = newLineItemId;
                                                    detail.PaketItemId = newLineItemId;
                                                    detail.NetTutar = amount > 0 ? (amount ?? 0 * quantity) : detail.NetTutar;
                                                    detail.ParaBirimiKodu = Currency.Try;
                                                    detail.Miktar = quantity;
                                                    if (string.IsNullOrEmpty(hepsiExpressChangeOrderDetailsDto.NewMerchantSku))
                                                    {
                                                        detail.PazarYeriMalNo = hepsiExpressChangeOrderDetailsDto.NewSku;
                                                    }
                                                    if (string.IsNullOrEmpty(hepsiExpressChangeOrderDetailsDto.NewSku))
                                                    {
                                                        detail.ObaseMalNo = hepsiExpressChangeOrderDetailsDto.NewMerchantSku;
                                                    }
                                                    await _pazarYeriSiparisDetayDalService.UpdateOrderDetailAsync(detail);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            return OrderHelper.ReturnQPResponseV2(changeHexOrderResponse.ResponseMessage.StatusCode, _logFolderName, CalcSuccessFromHTTPStatus(changeHexOrderResponse.ResponseMessage.StatusCode), changeHexOrderResponse.StringContent);
                                        }
                                    }
                                    else
                                    {
                                        if (unitProduct.PyUrunSatisBirim.Contains(CommonConstants.KG))
                                        {
                                            calculatedRelativeQuantity = 1;
                                        }
                                        if (!string.IsNullOrEmpty(lineItemId))
                                            hepsiExpressCompleteOrderReqDto.LineItemRequests.Add(new LineItemRequest() { Id = Guid.Parse(lineItemId), Quantity = calculatedRelativeQuantity });
                                    }
                                }
                                Logger.Information("OrderUpdatePackageStatus => Collected-CompleteOrder Request for MerchantId: {merchantId} and OrderId: {pyOrderNo} request: @request", fileName: _logFolderName, orderDto.MerchantId, orderDto.SiparisNo, hepsiExpressCompleteOrderReqDto);
                                var completeOrderResponse = await _hepsiExpressOrderClient.CompleteOrder(orderDto.MerchantId, hepsiExpressCompleteOrderReqDto);
                                Logger.Information("OrderUpdatePackageStatus => => Collected-CompleteOrder Response StatusCode : {statusCode}, Response : {@response}", fileName: _logFolderName, completeOrderResponse.ResponseMessage.StatusCode, completeOrderResponse?.GetContent());

                                var lineItemIdList = orderDetailsByObaseMalNo.Select(x => x.LineItemId).ToList();

                                if (!completeOrderResponse.ResponseMessage.IsSuccessStatusCode)
                                {
                                    string message = completeOrderResponse.StringContent;
                                    var model = JsonConvert.DeserializeObject<HEFailedResponseDto>(completeOrderResponse.StringContent);
                                    if (model != null)
                                    {
                                        message = model.Message;
                                    }

                                    // Conflict olması zaten daha önce tamamlanmış olduğu anlamına geliyor
                                    if (completeOrderResponse.ResponseMessage.StatusCode != HttpStatusCode.Conflict)
                                    {
                                        var completeCollectionStatus = completeOrderResponse.ResponseMessage.StatusCode;
                                        await UpdateOrderWithOrderDetails(nameof(StatusEnums.Collected), completeOrderResponse.ResponseMessage.IsSuccessStatusCode, null, orderDto.OrderId, completeOrderResponse.StringContent, lineItemIdList: lineItemIdList);

                                    }
                                    else
                                    {
                                        return OrderHelper.ReturnQPResponseV2(completeOrderResponse.ResponseMessage.StatusCode, _logFolderName, CalcSuccessFromHTTPStatus(completeOrderResponse.ResponseMessage.StatusCode), "Siparişler daha önceden toplanmış veya iptal edilmiştir!");
                                    }
                                }
                                else
                                {
                                    var packageNumberModel = JsonConvert.DeserializeObject<HEComplateSuccessOrderDto>(completeOrderResponse.StringContent);
                                    var order = await _pazarYeriSiparisDalService.GetOrderByIdAsync(orderDto.OrderId);
                                    if (order is not null)
                                    {
                                        order.PaketId = packageNumberModel.PackageNumber;
                                        await _pazarYeriSiparisDalService.UpdateOrderAsync(order);
                                    }
                                }
                                await UpdateOrderWithOrderDetails(nameof(StatusEnums.Collected), completeOrderResponse.ResponseMessage.IsSuccessStatusCode, null, orderDto.OrderId, completeOrderResponse.StringContent, lineItemIdList: lineItemIdList);

                                if (completeOrderResponse.ResponseMessage.IsSuccessStatusCode)
                                {
                                    Thread.Sleep(200);
                                    var packageNumberModel = JsonConvert.DeserializeObject<HEComplateSuccessOrderDto>(completeOrderResponse.StringContent);
                                    HEInTransitRequestDto.Root inTransitOrderReuest = new HEInTransitRequestDto.Root
                                    {
                                        shippedDate = DateTime.Now,
                                        estimatedArrivalDate = DateTime.Now.AddHours(2),
                                        trackingNumber = "",
                                        trackingPhoneNumber = "",
                                        trackingUrl = "",
                                        cost = 0,
                                        tax = 0,
                                        deci = 0
                                    };

                                    Logger.Information("OrderUpdatePackageStatus => Collected-InTransitOrder Request for MerchantId: {merchantId} and OrderId: {pyOrderNo} request: @request", fileName: _logFolderName, orderDto.MerchantId, orderDto.SiparisNo, inTransitOrderReuest);
                                    var inTransitOrderResponse = await _hepsiExpressOrderClient.InTransitOrder(merchantId, packageNumberModel.PackageNumber, inTransitOrderReuest);
                                    Logger.Information("OrderUpdatePackageStatus => Collected-InTransitOrder Response StatusCode : {statusCode}, Response : {@response}", fileName: _logFolderName, inTransitOrderResponse.ResponseMessage.StatusCode, completeOrderResponse?.GetContent());

                                    if (inTransitOrderResponse.ResponseMessage.StatusCode == HttpStatusCode.OK)
                                    {
                                        var order = await _pazarYeriSiparisDalService.GetOrderByIdAsync(orderDto.OrderId);
                                        if (order != null)
                                        {
                                            order.SevkiyatPaketDurumu = nameof(StatusEnums.InTransit);
                                            await _pazarYeriSiparisDalService.UpdateAsync(order);
                                        }
                                        return OrderHelper.ReturnQPResponseV2(inTransitOrderResponse.ResponseMessage.StatusCode, _logFolderName, CalcSuccessFromHTTPStatus(inTransitOrderResponse.ResponseMessage.StatusCode), inTransitOrderResponse.StringContent);
                                    }
                                    else
                                    {
                                        return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, CalcSuccessFromHTTPStatus(inTransitOrderResponse.ResponseMessage.StatusCode), inTransitOrderResponse.StringContent);
                                    }
                                }
                                else
                                {
                                    return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, completeOrderResponse.ResponseMessage.IsSuccessStatusCode, completeOrderResponse.StringContent);
                                }
                            }
                            else
                            {
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, false, "Sipariş detay tablosunda ürünler bulunamadı.");
                            }
                        }

                    #endregion

                    #region Completed
                    case StatusEnums.Completed:
                        {
                            HEDeliverRequestDto.Root hEDeliverRequestDto = new HEDeliverRequestDto.Root { receivedBy = "", receivedDate = DateTime.Now };
                            Logger.Information("OrderUpdatePackageStatus => Completed-DeliverOrder Request for MerchantId: {merchantId} and OrderId: {pyOrderNo} request: @request", fileName: _logFolderName, orderDto.MerchantId, orderDto.SiparisNo, hEDeliverRequestDto);
                            var deliverResponse = await _hepsiExpressOrderClient.DeliverOrder(merchantId, orderDto.PackageId, hEDeliverRequestDto);
                            Logger.Information("OrderUpdatePackageStatus => Completed-DeliverOrderr Response StatusCode : {statusCode}, Response : {@response}", fileName: _logFolderName, deliverResponse.ResponseMessage.StatusCode, deliverResponse?.GetContent());

                            if (deliverResponse.ResponseMessage.StatusCode == HttpStatusCode.OK)
                            {
                                var order = await _pazarYeriSiparisDalService.GetOrderByIdAsync(orderDto.OrderId);
                                if (order is not null)
                                {
                                    order.SevkiyatPaketDurumu = nameof(StatusEnums.Completed);
                                    order.HasSent = Character.E;
                                    order.Hata = string.Empty;
                                    await _pazarYeriSiparisDalService.UpdateOrderAsync(order);
                                }
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, deliverResponse.ResponseMessage.IsSuccessStatusCode, deliverResponse.StringContent);
                            }
                            else
                            {
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, false, "Sipariş Bulunamadı");
                            }
                        }
                    #endregion

                    #region UnDelivered
                    case StatusEnums.UnDelivered:
                        HEUnDeliverRequestDto.Root HEUnDeliverRequest = new HEUnDeliverRequestDto.Root { undeliveredReason = "CustomerUnreachable", undeliveredDate = DateTime.Now };
                        Logger.Information("OrderUpdatePackageStatus => Completed-UnDeliverOrder Request for MerchantId: {merchantId} and OrderId: {pyOrderNo} request: @request", fileName: _logFolderName, orderDto.MerchantId, orderDto.SiparisNo, HEUnDeliverRequest);
                        var unDeliverResponse = await _hepsiExpressOrderClient.UnDeliverOrder(merchantId, orderDto.PackageId, HEUnDeliverRequest);
                        Logger.Information("OrderUpdatePackageStatus => Completed-UnDeliverOrder Response StatusCode : {statusCode}, Response : {@response}", fileName: _logFolderName, unDeliverResponse.ResponseMessage.StatusCode, unDeliverResponse?.GetContent());

                        if (unDeliverResponse.ResponseMessage.StatusCode == HttpStatusCode.OK)
                        {
                            var order = await _pazarYeriSiparisDalService.GetOrderByIdAsync(orderDto.OrderId);
                            if (order is not null)
                            {
                                order.SevkiyatPaketDurumu = nameof(StatusEnums.UnDelivered);
                                order.HasSent = Character.E;
                                order.Hata = string.Empty;
                                await _pazarYeriSiparisDalService.UpdateOrderAsync(order);
                            }
                            return OrderHelper.ReturnQPResponseV2(unDeliverResponse.ResponseMessage.StatusCode, _logFolderName, unDeliverResponse.ResponseMessage.IsSuccessStatusCode, unDeliverResponse.StringContent);
                        }
                        else
                        {
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.NotFound, _logFolderName, false, "Sipariş Bulunamadı");
                        }
                    #endregion

                    #region Cancelled
                    case StatusEnums.Cancelled:
                        orderItems = await _pazarYeriSiparisDetayDalService.GetOrderDetailsdByIdAsync(orderDto.OrderId);
                        var cancelReasonId = GetHexReasonId(orderDto.ReasonId);
                        var reasonModel = new HEReasonDto() { ReasonId = cancelReasonId };

                        Logger.Information("OrderUpdatePackageStatus => Cancelled Request for MerchantId: {merchantId} and OrderId: {pyOrderNo}", fileName: _logFolderName, orderDto.MerchantId, orderDto.SiparisNo);
                        var cancelResponse = await _hepsiExpressOrderClient.PutOrderAsCanceled(merchantId, orderDto.SiparisNo, reasonModel);
                        Logger.Information("OrderUpdatePackageStatus => Cancelled Response StatusCode : {statusCode}, Response : {@response}", fileName: _logFolderName, cancelResponse.ResponseMessage.StatusCode, cancelResponse?.GetContent());
                        await UpdateOrderWithOrderDetails(nameof(StatusEnums.Cancelled), true, orderDto.ReasonId, orderDto.OrderId, cancelResponse.StringContent, orderItems.Select(x => x.PaketItemId).ToList());
                        return OrderHelper.ReturnQPResponseV2(cancelResponse.ResponseMessage.StatusCode, _logFolderName, cancelResponse.ResponseMessage.IsSuccessStatusCode, cancelResponse.StringContent);
                    #endregion

                    #region CancelledPart
                    case StatusEnums.CancelledPart:
                        orderDetailsByObaseMalNo = await _pazarYeriSiparisDetayDalService.GetOrderDetailsByOrderIdProductNoAsync(orderDto.OrderId, orderDto.ProductQuantities.Select(x => x.ProductId).ToList());

                        var reasonId = GetHexReasonId(orderDto.ReasonId);
                        var reasonRequest = new HEReasonDto() { ReasonId = reasonId };
                        lineItemId = orderDetailsByObaseMalNo.FirstOrDefault(x => x.ObaseMalNo == orderDto.ProductQuantities.FirstOrDefault()?.ProductId)?.LineItemId;
                        if (string.IsNullOrEmpty(lineItemId))
                        {
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.NotFound, _logFolderName, false, "Ürün Bulunamadı!");
                        }
                        //LineItem Bazlı İptal İşlemi
                        if (!string.IsNullOrEmpty(lineItemId))
                        {
                            Logger.Information("OrderUpdatePackageStatus => CancelledPart Request for MerchantId: {merchantId} and OrderId: {pyOrderNo} request: @request", fileName: _logFolderName, orderDto.MerchantId, orderDto.SiparisNo, reasonRequest);
                            var cancelOrderResponse = await _hepsiExpressOrderClient.CancelOrderByLineItemId(merchantId, lineItemId, reasonRequest);
                            Logger.Information("OrderUpdatePackageStatus => CancelledPart Statu Message => {statusCode} Response : {@response}", fileName: _logFolderName, orderDto.OrderId, cancelOrderResponse.ResponseMessage.StatusCode, cancelOrderResponse.StringContent ?? "");

                            if (cancelOrderResponse.ResponseMessage.StatusCode != HttpStatusCode.Conflict)
                            {
                                await UpdateOrderWithOrderDetails(StatusEnums.CancelledPart.ToString(), cancelOrderResponse.ResponseMessage.IsSuccessStatusCode, orderDto.ReasonId, orderDto.OrderId, cancelOrderResponse.StringContent, new List<string>() { lineItemId });

                                return OrderHelper.ReturnQPResponseV2(cancelOrderResponse.ResponseMessage.StatusCode, _logFolderName, cancelOrderResponse.ResponseMessage.IsSuccessStatusCode, cancelOrderResponse.StringContent);
                            }
                            else
                            {
                                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.Conflict, _logFolderName, false, "Bu Ürün Zaten İptal Edilmiş!");
                            }
                        }
                        else
                        {
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.NotFound, _logFolderName, false, "Line Item Id'ye Ait Bir Ürün Bulunamadı");
                        }
                    #endregion

                    #region UnPack
                    //Hazırlandı Statüsünü Geri Alma
                    //* Bu metod oluşturduğunuz paketleri bozmanıza olanak tanır.Son kullanıcı paketi bozduğunuz an siparişini Hazırlandı yerine bir önceki statüsünde görür.
                    //* https://developers.hepsiexpress.com/?docs=dokuman/siparis-entegrasyonu/paket-bozma
                    //
                    //case StatusEnums.UnPack:

                    //    var unPackReason = await this._hexClient.UnPackOrder(merchantId, orderDto.PackageNumber);
                    //    Logger.LogInformation($"OrderId : {orderDto.OrderId}, OrderStatus : UnPack, StatusCode : {unPackReason.ResponseMessage.StatusCode.ToString()}, StringContent : {unPackReason.StringContent}");
                    //    if (!unPackReason.ResponseMessage.IsSuccessStatusCode)
                    //    {
                    //        var completeReasonStatus = (HttpStatusCode)unPackReason.ResponseMessage.StatusCode;
                    //        return ReturnResponseByStatusCode(completeReasonStatus, unPackReason.StringContent);
                    //    }

                    //    _repository.SetOrderDetailsTableById(Consts.HAYIR, orderDto.OrderId, string.Empty);
                    //    _repository.SetAsSentOrderTableById(StatusEnums.UnPack.ToString(), orderDto.OrderId);

                    //    return CreateNewResponseAndReturn(HttpStatusCode.OK,unPackReason.ResponseMessage.IsSuccessStatusCode, unPackReason.StringContent);
                    #endregion

                    default:
                        return OrderHelper.ReturnQPResponseV2(HttpStatusCode.Conflict, _logFolderName, false, "Gönderdiğiniz Statu Durumuna Güncelleme Yapılamaz");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("OrderUpdatePackageStatus exception: {exception}", _logFolderName, ex);
                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.InternalServerError, _logFolderName, false, ex.Message);
            }
        }

        public async Task EndUserCancellation(string lineItemId, HEEndUserCancellationModel hepsiExpressEndUserCancellationModel)
        {
            var quantity = _pazarYeriSiparisDetayDalService.GetQuantityByLineItemId(hepsiExpressEndUserCancellationModel.Id);
            if (quantity != hepsiExpressEndUserCancellationModel.Quantity)
            {
                Logger.Error("EndUserCancellation => Gelen siparişin kayıtlı siparişteki Quantity değerleri eşleşmemektedir! LineItemId : {lineItemId}", fileName: _logFolderName, lineItemId);
                throw new Exception("Quantity Değerleri Eşleşmemektedir!");
            }
            var orderId = await _pazarYeriSiparisDalService.GetOrderIdByIdAsync(hepsiExpressEndUserCancellationModel.Id, PazarYeri.HepsiExpress);
            await UpdateOrderWithOrderDetails(nameof(StatusEnums.Cancelled), true, hepsiExpressEndUserCancellationModel.CancelReasonCode, orderId, "");

            long orderNumber = 0;
            long.TryParse(hepsiExpressEndUserCancellationModel.OrderNumber, out orderNumber);

            if (orderNumber > 0 && _pazarYeriSiparisDetayDalService.IsTheOrderToBeCancelled(orderNumber))
            {
                var qpOrderId = await _pazarYeriSiparisDalService.GetOrderByIdAsync(orderNumber);
                QPService.CancelServiceResultModel cancelResult = await _qpClient.CancelOrderAsync(qpOrderId.ToString(), "1");// Quickpick ekibi default olarak 1 yazılmasını söyledi.
                Logger.Information("QP CancelOrderAsync OK. cancelResult : {@cancelResult}", fileName: _logFolderName, cancelResult);
            }
        }

        public Task<CommonResponseDto> ClaimStatuUpdate(PostProductReturnRequestDto claimDto)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Utilities

        private async Task<bool> SaveOrderDb(HEOrderDto order, long qpOrderSeqId)
        {
            try
            {
                PazarYeriSiparis pazarYeriSiparis;
                string invoiceId = Guid.NewGuid().ToString();
                string shipmentAddressId = Guid.NewGuid().ToString();
                var merchantNo = PazarYeri.HepsiExpress;
                var merchantList = await _pazarYeriBirimTanimDalService.GetMerchantListAsync(merchantNo);
                var firstOrderItem = order.Items.FirstOrDefault();
                string shopId = firstOrderItem.MerchantId.ToString();

                var store = merchantList.FirstOrDefault(x => x.PazarYeriBirimNo.Trim() == shopId.Trim());
                var products = await _pazarYeriMalTanimDalService.GetProductObaseNoAsync(merchantNo);
                var transferProducts = await _pazarYeriAktarimDalService.GetProductObaseNoAndBarcodeAsync(merchantNo, shopId);

                pazarYeriSiparis = new PazarYeriSiparis()
                {
                    PazarYeriNo = merchantNo,
                    Id = qpOrderSeqId,
                    PaketId = order.Order.Id,
                    SiparisId = order.Order.Id,
                    SiparisNo = firstOrderItem.OrderNumber,
                    SiparisTarih = UnixTimeStampToDateTime(firstOrderItem.OrderDate.Ticks, merchantNo),
                    TahminiTeslimBaslangicTarih = UnixTimeStampToDateTime(firstOrderItem.DueDate.Ticks, merchantNo),
                    TahminiTeslimBitisTarih = UnixTimeStampToDateTime(firstOrderItem.DueDate.Ticks, merchantNo),
                    ToplamIndirimTutar = Convert.ToDouble(order.Items.Sum(x => x.HbDiscount.TotalPrice.Amount)),
                    ToplamTutar = order.Items.Sum(x => x.TotalPrice.Amount) - order.Items.Sum(x => x.HbDiscount.TotalPrice.Amount),
                    ParaBirimiKodu = firstOrderItem.TotalPrice.Currency,
                    MusteriId = firstOrderItem.CustomerId.ToString(),
                    MusteriAdi = firstOrderItem.CustomerName,
                    KargoGondericiNumarasi = string.Empty,
                    SevkiyatPaketDurumu = firstOrderItem.Status,
                    KargoAdresId = shipmentAddressId,
                    FaturaAdresId = invoiceId,
                    KargoSaglayiciAdi = firstOrderItem.CargoCompanyModel.Name,
                    KargoTakipUrl = firstOrderItem.CargoCompanyModel.TrackingUrl?.ToString(),
                    KoliAdeti = 0,
                    Desi = 0
                };
                PazarYeriKargoAdres pazarYeriKargoAdres = new PazarYeriKargoAdres()
                {
                    Id = qpOrderSeqId,
                    KargoAdresId = shipmentAddressId,
                    AdSoyad = firstOrderItem.CustomerName,
                    Ad = firstOrderItem.CustomerName,
                    Adres1 = firstOrderItem.ShippingAddress.AddressAddress,
                    TamAdres = firstOrderItem.ShippingAddress.AddressAddress,
                    Sehir = firstOrderItem.ShippingAddress.City[..20],
                    Semt = firstOrderItem.ShippingAddress.Town,
                    UlkeKod = firstOrderItem.ShippingAddress.CountryCode,
                };
                PazarYeriFaturaAdres pazarYeriFaturaAdres = new PazarYeriFaturaAdres()
                {
                    Id = qpOrderSeqId,
                    FaturaAdresId = invoiceId,
                    AdSoyad = firstOrderItem.CustomerName,
                    Adi = firstOrderItem.CustomerName,
                    Adres1 = firstOrderItem.Invoice.Address.AddressAddress,
                    Sehir = firstOrderItem.Invoice.Address.City[..20],
                    Semt = firstOrderItem.Invoice.Address.Town,
                    UlkeKod = firstOrderItem.Invoice.Address.CountryCode,
                    Firma = string.Empty,

                };
                //PazarYeriSiparisEkBilgi packagingInfo = new PazarYeriSiparisEkBilgi()
                //{
                //    PosetSayisi = order.packagingInfo.bagCount,
                //    PosetTutari = order.packagingInfo.totalPackagingPrice,
                //    PySiparisNo = order.orderNumber.ToString(),
                //    ObaseSiparisId = qpOrderSeqId
                //};

                List<PazarYeriSiparisDetay> pazarYeriSiparisDetayList = new List<PazarYeriSiparisDetay>();
                PazarYeriSiparisDetay pazarYeriSiparisDetay;

                List<PazarYeriSiparisUrun> pazarYeriSiparisUrunList = new List<PazarYeriSiparisUrun>();
                PazarYeriSiparisUrun pazarYeriSiparisUrun;

                if (order.Items.Any())
                {
                    string imageUrl = string.Empty;
                    foreach (var itemProduct in order.Items)
                    {
                        var transferProduct = transferProducts.FirstOrDefault(x => (x.PazarYeriMalNo == itemProduct?.Sku?.Trim() && x.PazarYeriBirimNo == shopId));
                        var product = transferProduct != null ? products.FirstOrDefault(x => x.MalNo.Trim() == transferProduct?.MalNo?.Trim()) : null;

                        pazarYeriSiparisUrun = new PazarYeriSiparisUrun()
                        {
                            ObaseSiparisId = qpOrderSeqId,
                            PySiparisNo = firstOrderItem.OrderNumber,
                            ObaseMalNo = product?.MalNo ?? (itemProduct.Sku ?? string.Empty),
                            AltUrunObaseMalNo = string.Empty,
                            AltUrunPazarYeriMalNo = string.Empty,
                            Miktar = itemProduct.Quantity,
                            IsAlternativeEH = Character.H,
                            ImageUrl = imageUrl
                        };
                        pazarYeriSiparisUrunList.Add(pazarYeriSiparisUrun);

                        pazarYeriSiparisDetay = new PazarYeriSiparisDetay()
                        {
                            LineItemId = itemProduct.Id,
                            PaketItemId = itemProduct.Id,
                            PazarYeriBirimId = shopId,
                            ObaseMalNo = product?.MalNo ?? (itemProduct.Sku ?? string.Empty),
                            PazarYeriMalNo = itemProduct.Sku,
                            PazarYeriMalAdi = itemProduct.Name,
                            PazarYeriUrunKodu = string.Empty,
                            AlternatifUrunEH = Character.H,
                            Barkod = itemProduct.Sku ?? string.Empty,
                            Miktar = itemProduct.Quantity,
                            NetTutar = itemProduct.TotalPrice.Amount,
                            IndirimTutar = itemProduct.HbDiscount.TotalPrice.Amount,
                            KdvTutar = itemProduct.Vat.Amount,
                            BrutTutar = itemProduct.TotalPrice.Amount,
                            KdvOran = itemProduct.VatRate,
                            SatisKampanyaId = string.Empty,
                            UrunBoyutu = string.Empty,
                            UrunRengi = string.Empty,
                            SiparisUrunDurumAdi = string.Empty,
                            ParaBirimiKodu = itemProduct.UnitPrice.Currency
                        };

                        pazarYeriSiparisDetayList.Add(pazarYeriSiparisDetay);
                    }
                }

                #region Db Save
                await _transactionDalService.BeginTransactionAsync();
                await _pazarYeriSiparisDalService.AddOrderAsync(pazarYeriSiparis);
                await _pazarYeriSiparisDetayDalService.AddOrderDetailsAsync(pazarYeriSiparisDetayList);
                //await _pazarYeriSiparisEkBilgiDalService.AddAdditionalDataAsync(packagingInfo);
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
                //if ((_appSettings.Value.MailSettings?.MailEnabled ?? false))
                //{
                //    await _mailService.SendMailAsync(_logFolderName + $" Hata! Sipariş veritabanına kaydedilemedi. Sipariş Id: {order.Order.Id ?? ""}", ex.Message + " " + ex.InnerException?.Message ?? "");
                //}
                await SendFailedOrderMail("HepsiExpress Servisinde Hata", $"Hata! Sipariş veritabanına kaydedilemedi. Sipariş Id: {order.Order.Id ?? ""} - {ex.Message} {ex.InnerException?.Message ?? ""} ");
                Logger.Error("SaveOrderDb Exception {exception} for Order: {order}", fileName: _logFolderName, ex, order.Items.FirstOrDefault()?.OrderNumber);
                await _transactionDalService.RollbackTransactionAsync();
                return false;
            }
        }

        private async Task<HEOrderDetailsDto> GetOrderDetailsV2(string merchantId, string orderNumber)
        {
            _hepsiExpressOrderClient = new HepsiExpressOrderClient(_appSettings);
            var response = await _hepsiExpressOrderClient.GetOrderDetails(merchantId, orderNumber);
            return response.ResponseMessage.IsSuccessStatusCode ? JsonConvert.DeserializeObject<HEOrderDetailsDto>(response.StringContent) : new HEOrderDetailsDto();
        }

        private string GetHexReasonId(string reasonId)
        {
            return reasonId switch
            {
                "23" or "37" => "26",
                "24" or "26" or "36" => "83",
                "25" or "27" or "34" => "82",
                _ => reasonId
            };
        }

        private async Task<bool> SendOrderToQp(PazarYeriBirimTanim pyBirimTanim, IEnumerable<PazarYeriMalTanim> pyProductList, IEnumerable<PazarYeriAktarim> pyTransferredProductList, long qpOrderSeqId,  HEOrderDto orderDto, SachetProduct[] sachetProducts = null, StringBuilder errorMessages = null, string merchantNo = "")
        {
            bool isSendToQp = false;      
            try
            {
                var qpModel = _orderConvertService.ToQpOrder(orderDto, pyBirimTanim, pyProductList, pyTransferredProductList, qpOrderSeqId, merchantNo: PazarYeri.HepsiExpress, sachetProduct: _apiDefinition.SachetProducts);
                QPService.DeliveryRequest consumeDeliveryRequest = new QPService.DeliveryRequest() { DeliveryAction = qpModel };
                Logger.Information("Request sent to QP:{@qpRequest} ", fileName: _logFolderName, qpModel);
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
                         Logger.Warning("HepsiExpress - SendOrderToQp - Retry attempt {Attempt} will be made after {Delay}s. Reason: {Message}", fileName: _logFolderName, args.AttemptNumber.ToString(), args.RetryDelay.TotalSeconds.ToString("F1"), args.Outcome.Exception?.Message ?? "Unknown error");
                         return ValueTask.CompletedTask;
                     }
                 }).Build();

                await pipeline.ExecuteAsync(async cancellationToken => {

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

                        string errorMessage = $"Sipariş Quickpick'e aktarılırken bir hata oluştu.Sipariş Id: {orderDto.Order.Id}. Hata:{consumeDeliveryResult}";
                        Logger.Error("QP Error With Id: {qpOrderSeqId}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, errorMessage);
                        throw new InvalidOperationException($"HepsiExpress - SendOrderToQp - Sipariş Quickpick'e aktarılırken bir hata oluştu ve tekrar gönderim sağlanacak. QP Response :{errorMessage}");
                    }
                    catch (Exception ex)
                    {
                        if ((_appSettings.Value.MailSettings?.MailEnabled ?? false))
                        {
                            await _mailService.SendMailAsync(_logFolderName + $" Hata! Sipariş Quickpick'e iletilemedi. Sipariş Id: {orderDto.Order.Id}", ex.Message + " " + ex.InnerException?.Message ?? "");
                        }
                        Logger.Error("QP Error With Id: {qpOrderSeqId} For Order Id: {orderNumber}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, orderDto.Order.Id, ex);
                        throw new InvalidOperationException($"HepsiExpress - SendOrderToQp - Sipariş Quickpick'e aktarılırken bir hata oluştu ve tekrar gönderim . Error :{ex.Message + " " + ex.InnerException?.Message ?? ""} ");
                    }
                });

            }
            catch (Exception ex)
            {
                if ((_appSettings.Value.MailSettings?.MailEnabled ?? false))
                {
                    await _mailService.SendMailAsync(_logFolderName + $" Hata! Sipariş Id: {orderDto.Order.Id ?? ""}", ex.Message + " " + ex.InnerException?.Message ?? "");
                }
                string errorMessage = $"Sipariş Quickpick'e aktarılırken veya Veritabanında yazılırken bir hata oluştu.Sipariş Id: {orderDto.Order.Id}. Hata:{ex}";
                Logger.Error("QP Error With Id: {qpOrderSeqId}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, errorMessage);
            }
            return isSendToQp;
        }
        #endregion

    }
}