using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Client.Concrete;
using OBase.Pazaryeri.Business.Helper;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Idefix;
using OBase.Pazaryeri.Domain.Dtos.Idefix.Shipment;
using OBase.Pazaryeri.Domain.Dtos.Idefix.Unsupplied;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using Polly;
using Polly.Retry;
using RestEase;
using System.Globalization;
using System.Net;
using System.ServiceModel;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using static OBase.Pazaryeri.Domain.Helper.CommonHelper;


namespace OBase.Pazaryeri.Business.Services.Concrete.Order
{
    public class IdefixOrderService : BaseService, IIdefixOrderService
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
        private readonly IOsmIdefixClient _osmIdefixClient;
        private readonly ITransactionDalService _transactionDalService;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.Idefix);

        private readonly QPService.OrderDeliveryServiceSoapClient _qpClient;

        private readonly IOrderConvertService _orderConvertService;

        #endregion

        #region Ctor

        public IdefixOrderService(IPazarYeriBirimTanimDalService pazarYeriBirimTanimDalService,
            IPazarYeriMalTanimDalService pazarYeriMalTanimDalService, IPazarYeriSiparisDalService pazarYeriSiparisDalService,
            IPazarYeriAktarimDalService pazarYeriAktarimDalService, IOsmIdefixClient osmIdefixClient, IPazarYeriSiparisDetayDalService pazarYeriSiparisDetayDalService,
            IPazarYeriSiparisUrunDalService pazarYeriSiparisUrunDalService, IPazarYeriSiparisEkBilgiDalService pazarYeriSiparisEkBilgiDalService, IOptions<AppSettings> appSetting,
            ITransactionDalService transactionDalService, IPazarYeriKargoAdresDalService pazarYeriKargoAdresDalService, IPazarYeriFaturaAdresDalService pazarYeriFaturaAdresDalService, IMailService mailService, IOrderConvertService orderConvertService) : base(pazarYeriSiparisDalService, pazarYeriSiparisDetayDalService, appSetting, mailService)
        {
            _pazarYeriBirimTanimDalService = pazarYeriBirimTanimDalService;
            _pazarYeriMalTanimDalService = pazarYeriMalTanimDalService;
            _pazarYeriAktarimDalService = pazarYeriAktarimDalService;
            _osmIdefixClient = osmIdefixClient;
            _pazarYeriSiparisUrunDalService = pazarYeriSiparisUrunDalService;
            _pazarYeriSiparisEkBilgiDalService = pazarYeriSiparisEkBilgiDalService;
            _appSetting = appSetting;
            if (_appSetting.Value.WareHouseUrl is not null)
            {
                _qpClient = new QPService.OrderDeliveryServiceSoapClient(QPService.OrderDeliveryServiceSoapClient.EndpointConfiguration.OrderDeliveryServiceSoap12, remoteAddress: _appSetting.Value.WareHouseUrl);
            }
            _transactionDalService = transactionDalService;
            _pazarYeriKargoAdresDalService = pazarYeriKargoAdresDalService;
            _pazarYeriFaturaAdresDalService = pazarYeriFaturaAdresDalService;
            _apiDefinition = _appSetting.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.Idefix);
            _orderConvertService = orderConvertService;
        }

        #endregion

        #region Methods

        public async Task<CommonResponseDto> OrderUpdatePackageStatus(OrderStatuUpdateRequestDto orderDto, PazarYeriSiparis orderEntity)
        {
            Logger.Information("OrderUpdatePackageStatus Request :{@request} ", fileName: _logFolderName, orderDto);

            try
            {
                string supplierId = _apiDefinition.SupplierId;
                bool enableIdefixServices = _appSetting.Value.EnableMarketPlaceServices;
                var status = orderDto.Status;
                if (!orderDto.ProductQuantities.Any())
                {
                    return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "En az Bir Ürün Göndermelisiniz");
                }

                var packageId = orderDto.PackageId;

                switch (status)
                {
                    case StatusEnums.Picking:
                        return await HandlePickingAsync(orderDto, packageId, supplierId, enableIdefixServices);

                    case StatusEnums.Completed:
                        {
                            await UpdateOrderWithOrderDetails(nameof(StatusEnums.Completed), true, null, orderDto.OrderId, string.Empty);
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Completed işlemi tamamlandı.");
                        }
                    case StatusEnums.Cancelled:
                        return await HandleCancelledAsync(orderDto, orderEntity, enableIdefixServices);

                    case StatusEnums.Collected:
                        return await HandleCollectedAsync(orderDto, orderEntity, packageId, supplierId, enableIdefixServices);

                    case StatusEnums.InTransit:
                        {
                            await UpdateOrderWithOrderDetails(nameof(StatusEnums.InTransit), true, null, orderDto.OrderId, string.Empty);
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, CalcSuccessFromHTTPStatus(HttpStatusCode.OK), "Siparişin durumu yolda olarak güncellenmiştir.");
                        }
                        ;
                    case StatusEnums.Delivered:
                        {
                            await UpdateOrderWithOrderDetails(nameof(StatusEnums.Delivered), true, null, orderDto.OrderId, string.Empty);
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, CalcSuccessFromHTTPStatus(HttpStatusCode.OK), string.Empty);
                        }
                    default:
                        {
                            return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "Idefix siparişlerinde bu statüye geçiş yapılamaz.");
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("OrderUpdatePackageStatus exception: {exception}", _logFolderName, ex);
                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.InternalServerError, _logFolderName, false, $"Statü geçişi sırasında bir hata oluştu.Hata:{ex.Message}");
            }

        }

        public async Task<List<IdefixOrderDto>> GetIdefixOrdersByVendorIdAsync(string vendorId, string state, string startDate, string endDate)
        {
            List<IdefixOrderDto> orders = new List<IdefixOrderDto>();
            bool loopCondition = true;
            int page = 0;
            try
            {
                while (loopCondition)
                {
                    Logger.Information($"GetShipmentList => Request: vendorId: {vendorId} - startDate : {startDate} - endDate : {endDate} - state: {state}", _logFolderName);
                    var result = await _osmIdefixClient.GetShipmentList(vendorId: vendorId, startDate: startDate, endDate: endDate, page: page.ToString(), state: state);
                    if (result.ResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        Logger.Information("GetShipmentList => orders, API Request with Http Status Code: {statusCode}, API Response: {response}", fileName: _logFolderName, result.ResponseMessage.StatusCode, result?.StringContent ?? "");
                        var root = JsonConvert.DeserializeObject<IdefixGenericResponse<IdefixOrderDto>>(result.StringContent);

                        orders.AddRange(root.Items);
                        page++;
                        loopCondition = page < root.PageCount;
                    }
                    else
                    {
                        var errorResponse = JsonConvert.DeserializeObject<IdefixErrorResponse>(result.StringContent);
                        await SendFailedOrderMailFormattedAsync("Idefix Servisinde Hata", $"Hata! Siparişler çekilemedi. Pazar Yeri Vendor No: {_apiDefinition.SupplierId ?? ""} \nResult: {errorResponse}");
                        Logger.Warning("Execution Type: {orderStatus} orders, API Request Error with Http Status Code: {statusCode}, API Response: {response}", fileName: _logFolderName, state, result.ResponseMessage.StatusCode, errorResponse);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Get {state} orders request failed for {SupplierId}. Error: {e}", fileName: _logFolderName, state, _apiDefinition.SupplierId, e);
            }

            return orders;
        }

        public async Task<ServiceResponse<UnsuppliedResponse>> MarkShipmentAsUnsuppliedAsync(string vendorId, string shipmentId, UnsuppliedRequest unsupplied)
        {
            try
            {
                Logger.Information("MarkShipmentAsUnsuppliedAsync => Unsupplied items Request: {@request}", _logFolderName, unsupplied);
                var result = await _osmIdefixClient.MarkShipmentAsUnsuppliedAsync(vendorId: vendorId, shipmentId: shipmentId, request: unsupplied);

                var res = new UnsuppliedResponse
                {
                    Items = unsupplied.Items
                };

                if (result.ResponseMessage.IsSuccessStatusCode)
                {
                    res.Unsupplied = true;

                    Logger.Information("MarkShipmentAsUnsuppliedAsync Unsupplied items: {unsuppliedItems} orders, API Request with Http Status Code: {statusCode}, API Response: {response}", fileName: _logFolderName, unsupplied, result.ResponseMessage.StatusCode, result?.StringContent ?? "");

                    return ServiceResponse<UnsuppliedResponse>.Success(data: res);
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<IdefixErrorResponse>(result.StringContent);
                    Logger.Error("MarkShipmentAsUnsuppliedAsync Unsupplied items: {unsuppliedItems} orders, API Request Error with Http Status Code: {statusCode}, API Response: {response}", fileName: _logFolderName, unsupplied, result.ResponseMessage.StatusCode, errorResponse);
                    return ServiceResponse<UnsuppliedResponse>.Error(errorMessage: $"{errorResponse.Message} - {errorResponse.ErrorCode} - {errorResponse.Code}");
                }
            }
            catch (Exception ex)
            {

                Logger.Error("MarkShipmentAsUnsuppliedAsync {unsuppliedItems} orders request failed for {SupplierId}. Error: {e}", fileName: _logFolderName, unsupplied, _apiDefinition.SupplierId, ex);
                return ServiceResponse<UnsuppliedResponse>.Error(errorMessage: ex.ToString());
            }

        }

        public async Task ProcessIdefixCreatedOrdersAsync(Dictionary<string, string> properties)
        {
            string merchantNo = PazarYeri.Idefix;
            bool sendToQp = bool.Parse(properties[IdefixConstants.Parameters.SendToQp]);
            int dayCount = int.Parse(properties[IdefixConstants.Parameters.GetOrdersDayCount]);
            DateTime dateTimeNow = DateTime.Now.ToLocalTime();
            string startDate = dateTimeNow.Date.AddDays(-1).Add(new TimeSpan(23, 59, 59)).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            string endDate = dateTimeNow.AddDays(dayCount).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            var storeList = await _pazarYeriBirimTanimDalService.GetStoreDetailsListAsync(merchantNo, onlyActive: true);
            Logger.Information("Get Orders from: Start Date {startDate}", fileName: _logFolderName, startDate);
            foreach (var store in storeList)
            {
                var storeCreatedOrderList = await GetIdefixOrdersByVendorIdAsync(vendorId: store.PazarYeriBirimNo, state: IdefixConstants.OrderState.ShipmentReady, startDate, endDate);
                if (storeCreatedOrderList is not null && storeCreatedOrderList.Any())
                {
                    foreach (var storeOrder in storeCreatedOrderList)
                    {
                        var orderBarcodes = storeOrder.Items?
                                  .Select(item => item.Barcode?.Trim())
                                  .Where(b => !string.IsNullOrWhiteSpace(b))
                                  .Distinct()
                                  .ToList() ?? [];

                        var pyOrderTransferredProductList = await _pazarYeriAktarimDalService.GetPyTransferredProductsAsync(merchantNo, store.PazarYeriBirimNo, orderBarcodes);

                        //siparişteki ürün sayisi ile aktarimdaki urun sayisi ayni olmali
                        if (!pyOrderTransferredProductList.Any() && pyOrderTransferredProductList.Count() != orderBarcodes.Count())
                        {
                            Logger.Information("Get Orders from: End Date {endDate}", fileName: _logFolderName, endDate);
                            await SendFailedOrderMailFormattedAsync("Idefix - Siapriş Kayıt Hata", $"Hata! Sipariş veritabanına kaydedilemedi.Sipariş içindeki ürünler PAZAR_YERI_AKTARIM tablsounda mevcut degil. Ürün barkodları: {string.Join(",", orderBarcodes.ToArray())}",storeOrder);
                        }
                        else
                        {
                            var orderProductMalNos = pyOrderTransferredProductList.Select(x => x.MalNo).Distinct().ToList();
                            var pyOrderProductList = await _pazarYeriMalTanimDalService.GetPyProductsAsync(merchantNo, orderProductMalNos);

                            //if (storeOrder.Items is not null)
                            //{
                            //    storeOrder.Items = storeOrder.Items
                            //        .Where(i => !string.IsNullOrWhiteSpace(i.Barcode))
                            //        .GroupBy(x => x.Barcode!.Trim())
                            //        .Select(g => g.First())
                            //        .ToList();
                            //}

                            await SaveIdefixCreatedOrdersAsync(storeOrder, sendToQp, store, pyOrderProductList, pyOrderTransferredProductList, merchantNo);

                        }
                    }
                }
            }
            Logger.Information("Get Orders from: End Date {endDate}", fileName: _logFolderName, endDate);
        }

        public async Task ProcessIdefixCancelledOrdersAsync(Dictionary<string, string> properties)
        {
            string merchantNo = PazarYeri.Idefix;
            int dayCount = int.Parse(properties[IdefixConstants.Parameters.GetOrdersDayCount]);
            var storeList = await _pazarYeriBirimTanimDalService.GetStoreDetailsListAsync(merchantNo, onlyActive: true);
            DateTime dateTimeNow = DateTime.Now.ToLocalTime();
            string startDate = dateTimeNow.Date.AddDays(-dayCount).Add(new TimeSpan(23, 59, 59)).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            string endDate = dateTimeNow.Date.AddDays(1).AddTicks(-1).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            Logger.Information("Get Cancelled Orders from: Start Date {startDate}", fileName: _logFolderName, startDate);
            foreach (var store in storeList)
            {
                var storeCancelledOrderList = await GetIdefixOrdersByVendorIdAsync(vendorId: store.PazarYeriBirimNo, state: IdefixConstants.OrderState.ShipmentCancelled, startDate: startDate, endDate: endDate);
                if (storeCancelledOrderList is not null && storeCancelledOrderList.Any())
                {
                    await SaveIdefixCancelledOrdersAsync(storeCancelledOrderList);
                }
            }

            Logger.Information("Get Cancelled Orders from: End Date {endDate}", fileName: _logFolderName, endDate);
        }

        public async Task SaveIdefixCreatedOrdersAsync(IdefixOrderDto storeCreatedOrder, bool sendToQp, PazarYeriBirimTanim pyBirimTanimDto, IEnumerable<PazarYeriMalTanim> pyProductList, IEnumerable<PazarYeriAktarim> pyTransferredProductList, string merchantNo)
        {
            Logger.Information("SaveIdefixCreatedOrdersAsync  is running.", fileName: _logFolderName);
            using OperationContextScope operationContextScope = new(_qpClient.InnerChannel);
            string supplierId = _apiDefinition.SupplierId;
            StringBuilder errorMessages = new("");
            try
            {
                Logger.Information("SaveIdefixCreatedOrdersAsync Store Id:{store} Created Order Id:{orderId} Order: {@response}", fileName: _logFolderName, storeCreatedOrder.VendorId, storeCreatedOrder.Id, storeCreatedOrder);
                long qpOrderSeqId;
                bool dbResultVal;
                try
                {
                    if (!await _pazarYeriSiparisDalService.OrderExistAsync(storeCreatedOrder.Id.ToString(), merchantNo))
                    {
                        qpOrderSeqId = await _pazarYeriSiparisDalService.GetSeqId();
                        dbResultVal = await SaveOrderDb(storeCreatedOrder, storeCreatedOrder.VendorId.ToString(), qpOrderSeqId, pyProductList, pyTransferredProductList);
                    }
                    else
                    {
                        qpOrderSeqId = await _pazarYeriSiparisDalService.GetOrderIdByIdAsync(storeCreatedOrder.Id.ToString(), merchantNo);
                        dbResultVal = true;
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Append($"Sipariş No: {storeCreatedOrder.OrderNumber} için hata alındı. Hata: {ex.Message} Detay: {ex.InnerException?.Message ?? ""}");
                    qpOrderSeqId = 0;
                    dbResultVal = false;
                    Logger.Error("SaveIdefixCreatedOrdersAsync An error occurred while querying or saving the order id{orderId} from the db.Hata: {exception}", fileName: _logFolderName, storeCreatedOrder.OrderNumber, ex);
                }

                long orderCountTransferredToQP = await _pazarYeriSiparisDalService.GetOrderWareHouseTransferredCountAsync(storeCreatedOrder.OrderNumber, merchantNo);
                long ordersCountWithSameOrderID = await _pazarYeriSiparisDalService.GetOrdersCountWithSameOrderIdAsync(storeCreatedOrder.OrderNumber, merchantNo);
                bool _sendQp = sendToQp && orderCountTransferredToQP < 1;


                if (ordersCountWithSameOrderID > 1)
                {
                    var createdOrder = await _pazarYeriSiparisDalService.GetCreatedOrderByOrderNumberAsync(storeCreatedOrder.OrderNumber, merchantNo, IdefixConstants.OrderState.ShipmentReady);

                    if (createdOrder != null && storeCreatedOrder.Id.ToString() == createdOrder.PaketId)
                    {
                        Logger.Information("Existing order! ID: {orderId} Order Number: {orderNumber}", fileName: _logFolderName, createdOrder.Id, createdOrder.SiparisNo);
                        await HandleOrderStatusUpdateAsync(createdOrder, supplierId);
                    }
                }

                if (_sendQp && dbResultVal)
                {
                    var sendQpResult = await SendOrderToQp(pyBirimTanimDto, pyProductList, pyTransferredProductList, qpOrderSeqId, merchantNo: PazarYeri.Idefix, sachetProduct: _apiDefinition.SachetProducts, errorMessages: errorMessages, orderDto: storeCreatedOrder);
                    if (sendQpResult)
                    {
                        var orderItem = await _pazarYeriSiparisDalService.GetOrderByIdAsync(qpOrderSeqId);
                        if (orderItem is not null)
                        {
                            orderItem.DepoAktarildiEH = Character.E;
                            await _pazarYeriSiparisDalService.UpdateOrderAsync(orderItem);
                        }
                    }
                    else
                    {
                        Logger.Error("QP Error With Id: {qpOrderSeqId}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, errorMessages);
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessages.AppendLine($"Hata: {ex.Message} Detay: {ex.InnerException?.Message ?? ""}");
                Logger.Error("SaveIdefixCreatedOrdersAsync Hata:{exception}", fileName: _logFolderName, ex);
                await SendFailedOrderMailFormattedAsync("Idefix Servisinde Hata", $"Hata! Siparişler veritabanına kaydedilemedi. - {errorMessages}", storeCreatedOrder , ex);
            }
        }

        public async Task<ServiceResponse<CommonResponseDto>> SaveIdefixCreatedOrderAsync(IdefixOrderDto order)
        {
            Logger.Information("SaveIdefixCreatedOrderAsync  is running.", fileName: _logFolderName);
            using OperationContextScope operationContextScope = new(_qpClient.InnerChannel);
            string supplierId = _apiDefinition.SupplierId;
            StringBuilder errorMessages = new("");
            try
            {
                string merchantNo = PazarYeri.Idefix;
                bool sendToQp = true;
                PazarYeriBirimTanim pyBirimTanimDto = await _pazarYeriBirimTanimDalService.GetStoreDetailAsync(merchantNo, order.VendorId.ToString());

                var orderBarcodes = order.Items?
                           .Select(item => item.Barcode?.Trim())
                           .Where(b => !string.IsNullOrWhiteSpace(b))
                           .Distinct()
                           .ToList();


                var pyTransferredProductList = await _pazarYeriAktarimDalService.GetPyTransferredProductsAsync(merchantNo, order.VendorId.ToString(), orderBarcodes);
                var orderProductMalNos = pyTransferredProductList.Select(x => x.MalNo).Distinct().ToList();

                var pyProductList = await _pazarYeriMalTanimDalService.GetPyProductsAsync(merchantNo, orderProductMalNos);


                //siparişteki ürün sayisi ile aktarimdaki urun sayisi ayni olmali
                if (!pyTransferredProductList.Any() && pyTransferredProductList.Count() != orderBarcodes.Count())
                {
                    var message = $"Hata! Sipariş veritabanına kaydedilemedi. Pazar Yeri Birim No: {order.VendorId} - Idefix Sipariş Id: {order.OrderNumber} - Sipariş içindeki ürünler PAZAR_YERI_AKTARIM tablsounda mevcut degil. Ürün barkodları: {string.Join(",", orderBarcodes.ToArray())}";
                    await SendFailedOrderMailFormattedAsync("Idefix Servisinde Hata", message, order);
                    return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = message, Success = false });
                }

                Logger.Information("SaveIdefixCreatedOrderAsync Store Id:{store} Created Order Id:{orderId} Order: {@response}", fileName: _logFolderName, order.VendorId, order.OrderNumber, order);
                long qpOrderSeqId;
                bool dbResultVal;
                try
                {
                    if (!await _pazarYeriSiparisDalService.OrderExistAsync(order.Id.ToString(), merchantNo))
                    {
                        qpOrderSeqId = await _pazarYeriSiparisDalService.GetSeqId();
                        dbResultVal = await SaveOrderDb(order, order.VendorId.ToString(), qpOrderSeqId, pyProductList, pyTransferredProductList);
                    }
                    else
                    {
                        qpOrderSeqId = await _pazarYeriSiparisDalService.GetOrderIdByIdAsync(order.Id.ToString(), merchantNo);
                        dbResultVal = true;
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Append($"Sipariş No: {order.OrderNumber} için hata alındı. Hata: {ex.Message} Detay: {ex.InnerException?.Message ?? ""}");
                    qpOrderSeqId = 0;
                    dbResultVal = false;
                    Logger.Error("SaveIdefixCreatedOrderAsync An error occurred while querying or saving the order id{orderId} from the db.Hata: {exception}", fileName: _logFolderName, order.OrderNumber, ex);
                }

                long orderCountTransferredToQP = await _pazarYeriSiparisDalService.GetOrderWareHouseTransferredCountAsync(order.OrderNumber, merchantNo);
                bool _sendQp = sendToQp && orderCountTransferredToQP < 1;

                if (_sendQp && dbResultVal)
                {
                    var sendQpResult = await SendOrderToQp(pyBirimTanimDto, pyProductList, pyTransferredProductList, qpOrderSeqId, merchantNo: PazarYeri.Idefix, sachetProduct: _apiDefinition.SachetProducts, errorMessages: errorMessages, orderDto: order);
                    if (sendQpResult)
                    {
                        var orderItem = await _pazarYeriSiparisDalService.GetOrderByIdAsync(qpOrderSeqId);
                        if (orderItem is not null)
                        {
                            orderItem.DepoAktarildiEH = Character.E;
                            await _pazarYeriSiparisDalService.UpdateOrderAsync(orderItem);
                        }
                    }
                    else
                    {
                        Logger.Error("QP Error With Id: {qpOrderSeqId}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, errorMessages);
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessages.AppendLine($"Hata: {ex.Message} Detay: {ex.InnerException?.Message ?? ""}");
                Logger.Error("SaveIdefixCreatedOrderAsync Hata:{exception}", fileName: _logFolderName, ex);
                await SendFailedOrderMailFormattedAsync("Idefix Servisinde Hata", $"Hata! Siparişler veritabanına kaydedilemedi.{errorMessages}", order , ex);
                return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = errorMessages.ToString(), Success = false });
            }
            return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = "Order Saved", Success = true });
        }

        public async Task SaveIdefixCancelledOrdersAsync(List<IdefixOrderDto> storeCancelledOrderList)
        {
            StringBuilder errorMessages = new StringBuilder("");
            Logger.Information("SaveIdefixCancelledOrdersAsync  is running.", fileName: _logFolderName);
            foreach (var itemOrder in storeCancelledOrderList)
            {
                try
                {
                    var order = await _pazarYeriSiparisDalService.GetOrderWithOrderIdAsync(itemOrder.OrderNumber, PazarYeri.Idefix);
                    if (order == null || (order?.SevkiyatPaketDurumu == IdefixConstants.OrderState.ShipmentCancelled && order?.DepoAktarildiEH == Character.E))
                    {
                        continue;
                    }
                    else if (order?.DepoAktarildiEH == Character.H)
                    {
                        // henüz quickpick aktarımı yapılmamış siparişler için iptal işlemi yapılmayacak
                        continue;
                    }
                    else
                    {

                        var oldestOrder = await _pazarYeriSiparisDalService.GetOldestOrderWithOrderIdAsync(itemOrder.OrderNumber, PazarYeri.Idefix);
                        var id = order.Id;
                        var response = await _qpClient.CancelOrderAsync(oldestOrder.Id.ToString(), "1");
                        if (response.Response)
                        {
                            Logger.Information("Order is cancelled. QP Success With ID: {qpOrderId}", fileName: _logFolderName, oldestOrder.Id);
                            if (order.Id != oldestOrder.Id)
                            {
                                oldestOrder.SevkiyatPaketDurumu = IdefixConstants.OrderState.ShipmentCancelled;
                                oldestOrder.DepoAktarildiEH = Character.E;
                                await _pazarYeriSiparisDalService.UpdateOrderAsync(oldestOrder);
                            }
                            order.SevkiyatPaketDurumu = IdefixConstants.OrderState.ShipmentCancelled;
                            order.DepoAktarildiEH = Character.E;
                            await _pazarYeriSiparisDalService.UpdateOrderAsync(order);
                        }
                        else
                        {
                            errorMessages.AppendLine($"İptal sipariş Quickpick'e aktarılırken hata alındı. Sipariş No: {itemOrder.OrderNumber}. Id: {id}");
                            Logger.Error("Order is not cancelled. QP Error With ID: {qpOrderId} response:{response}", fileName: _logFolderName, id, response.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.AppendLine($"İptal sipariş işlenirken hata alındı. Sipariş No: {itemOrder.OrderNumber}. Hata: {ex.Message} Detay: {ex.InnerException?.Message ?? ""}");
                    Logger.Error("SaveIdefixCancelledOrdersAsync Hata: {exception}", fileName: _logFolderName, ex);
                    await SendFailedOrderMailFormattedAsync("Idefix Servisinde Hata", $"Hata! İptal siparişler işlenirken hata alındı. - {errorMessages}", itemOrder, ex);
                }

            }
        }

        public async Task<CommonResponseDto> ClaimStatuUpdate(PostProductReturnRequestDto claimDto)
        {
            Logger.Information("ClaimStatuUpdate Request:{@request}", _logFolderName, claimDto);
            switch (claimDto.Result)
            {
                //case TyGoConstants.Accepted:
                //	return await _tyGoReturnService.AcceptClaimAsync(claimDto);
                //case TyGoConstants.Rejected:
                //	return await _tyGoReturnService.RejectClaimAsync(claimDto);
                default:
                    return new CommonResponseDto() { Message = "ReasonCode sadece ACCEPTED veya REJECTED olabilir." };
            }
        }

        #endregion

        #region Private Methods


        #region OrderUpdatePackageStatus
        private async Task<CommonResponseDto> HandlePickingAsync(
            OrderStatuUpdateRequestDto orderDto,
            string packageId,
            string supplierId,
            bool enableIdefixServices)
        {
            if (enableIdefixServices)
            {

                var pickingResponse = await SendStatusUpdateAsync(
                    supplierId,
                    packageId,
                    IdefixConstants.OrderState.ShipmentPicking,
                    "Picking");


                if (pickingResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    await UpdateOrderWithOrderDetails(
                        CommonEnums.StatusEnums.Picking.ToString(),
                        pickingResponse.ResponseMessage.IsSuccessStatusCode,
                        null,
                        orderDto.OrderId,
                        pickingResponse.StringContent);

                    return new CommonResponseDto()
                    {
                        StatusCode = pickingResponse.ResponseMessage.StatusCode,
                        Success = CalcSuccessFromHTTPStatus(pickingResponse.ResponseMessage.StatusCode),
                        Message = pickingResponse.StringContent
                    };
                }
                else
                {
                    var errorResponse = LogError("Picking", pickingResponse);
                    var qpMessage = ParseFaultResponseMessageUserFriendly(pickingResponse.StringContent ?? "", false);
                    return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest
                    , _logFolderName
                    , false
                    ,$"Idefix Dönüşü -> <br /> {qpMessage}");
                }
            }
            else
            {
                await UpdateOrderWithOrderDetails(CommonEnums.StatusEnums.Picking.ToString(), true, null, orderDto.OrderId, string.Empty);
                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true,
                    "Pazaryeri servisleri pasif durumda olduğu için Idefix üzerinde işlem yapılmamıştır.");
            }
        }

        private async Task<CommonResponseDto> HandleCancelledAsync(
            OrderStatuUpdateRequestDto orderDto,
            PazarYeriSiparis orderEntity,
            bool enableIdefixServices)
        {
            if (enableIdefixServices)
            {
                UnsuppliedRequest unsuppliedRequest = new UnsuppliedRequest();
                var mpOrderProducts = await _pazarYeriSiparisUrunDalService.GetOrderProductsByIdAsync(orderDto.OrderId);
                var marketPlaceOrderLineItems = orderEntity.PazarYeriSiparisDetails;

                foreach (var mpOrderProduct in marketPlaceOrderLineItems)
                {
                    ProductQuantity productQuantity = orderDto.ProductQuantities.Find(qpX => qpX.AltProductId == mpOrderProduct.ObaseMalNo) ?? null;
                    unsuppliedRequest.Items.Add(new UnsuppliedItem()
                    {
                        Id = Convert.ToInt32(mpOrderProduct.PaketItemId),
                        ReasonId = GetReasonIdOrDefault(productQuantity?.ReasonId)
                    });
                }

                var markShipmentAsUnsuppliedAsync = await MarkShipmentAsUnsuppliedAsync(_apiDefinition.SupplierId, orderEntity.PaketId, unsuppliedRequest);
                if (markShipmentAsUnsuppliedAsync.Data.Unsupplied)
                {
                    await UpdateOrderWithOrderDetails(nameof(StatusEnums.Cancelled), markShipmentAsUnsuppliedAsync.IsSuccessful, orderDto.ReasonId, orderDto.OrderId, string.Empty);
                    var qpMessage = ParseFaultResponseMessageUserFriendly("", markShipmentAsUnsuppliedAsync.IsSuccessful);
                    return OrderHelper.ReturnQPResponseV2(markShipmentAsUnsuppliedAsync.HttpStatusCode, _logFolderName,
                        CalcSuccessFromHTTPStatus(markShipmentAsUnsuppliedAsync.HttpStatusCode), qpMessage);
                }
                else
                {
                    return OrderHelper.ReturnQPResponseV2(HttpStatusCode.InternalServerError, _logFolderName, false,
                        "Sipariş İptal işlemi sırasında - Tedarik edilemedi bilgisi gönderirken bir hata oluştu: " + markShipmentAsUnsuppliedAsync.ErrorMessages);
                }
            }
            else
            {
                await UpdateOrderWithOrderDetails(nameof(StatusEnums.Cancelled), true, null, orderDto.OrderId, string.Empty, null);
                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true,
                    "Pazaryeri servisleri pasif durumda olduğu için İdefix Sepeti üzerinde işlem yapılmamıştır.");
            }
        }

        private async Task<CommonResponseDto> HandleCollectedAsync(
            OrderStatuUpdateRequestDto orderDto,
            PazarYeriSiparis orderEntity,
            string packageId,
            string supplierId,
            bool enableIdefixServices)
        {


            SachetProduct[] sachetProducts = _apiDefinition.SachetProducts;
            Dictionary<string, string> productNoMap = new();
            int bagCount = 0;
            decimal bagTotalPrice = 0;

            UnsuppliedRequest unsuppliedRequest = new UnsuppliedRequest();


            foreach (ProductQuantity productQuantity in orderDto.ProductQuantities)
            {
                SachetProduct sachetProduct = sachetProducts?.FirstOrDefault(sp => sp.ProductCode == productQuantity.ProductId);
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


            var mpOrderProducts = await _pazarYeriSiparisUrunDalService.GetOrderProductsByIdAsync(orderDto.OrderId);

            IEnumerable<PazarYeriMalTanim> productsWithKGUnit = await _pazarYeriMalTanimDalService.GetProductSalesValueByUnitAsync(CommonConstants.KG, _apiDefinition.Merchantno);
            var marketPlaceOrderLineItems = orderEntity.PazarYeriSiparisDetails;

            Dictionary<string, TGProductAdditionalDataDto> mpOrderProductAdditionalDataDict = new();
            decimal totalAmount = 0;

            bool areUnSuppliedProductsPresent = false;



            foreach (var mpOrderProduct in mpOrderProducts)
            {
                ProductQuantity qpProduct = orderDto.ProductQuantities.Find(qpX => qpX.ProductId == mpOrderProduct.ObaseMalNo);

                var productDetails = marketPlaceOrderLineItems.Where(x => x.ObaseMalNo == qpProduct.ProductId).ToList();

                var sumProductQuantity = productDetails.Sum(x => x.Miktar);
                var selectedProductDetail = productDetails.FirstOrDefault();

                ProductQuantity qpAltProduct = orderDto.ProductQuantities.Find(qpX => qpX.AltProductId == mpOrderProduct.ObaseMalNo) ?? null;
                TGProductAdditionalDataDto mpOrderProductAdditionalData = new();

                if (qpAltProduct is not null && qpAltProduct.Quantity > 0)
                {
                    mpOrderProductAdditionalData.AltProductId = qpAltProduct.ProductId;
                    mpOrderProductAdditionalData.altSuppliedQuantity = qpAltProduct.Quantity;
                }

                mpOrderProductAdditionalData.orderedQuantity = sumProductQuantity;
                mpOrderProductAdditionalData.isWeightedItem = productsWithKGUnit.Any(x => x.MalNo == mpOrderProduct.ObaseMalNo);
                var pyProductUnitValue = productsWithKGUnit.FirstOrDefault(x => x.MalNo == mpOrderProduct.ObaseMalNo)?.PyUrunSatisDeger ?? 1;
                mpOrderProductAdditionalData.tyQuantityCoefficient = (mpOrderProductAdditionalData.isWeightedItem) ? pyProductUnitValue : 1;
                decimal suppliedQuantityTemp = 0;
                decimal productMPUnitPrice = (selectedProductDetail.NetTutar / selectedProductDetail.Miktar);


                decimal unSuppliedQuantityTemp = sumProductQuantity - (qpProduct.Quantity < 0 ? 0 : qpProduct.Quantity);

                mpOrderProductAdditionalData.unSuppliedQuantity = (unSuppliedQuantityTemp < 0) ? 0 : unSuppliedQuantityTemp;
                mpOrderProductAdditionalData.isUnSupplied = mpOrderProductAdditionalData.unSuppliedQuantity > 0;
                mpOrderProductAdditionalData.isFullyUnSupplied = qpProduct.Quantity <= 0;



                suppliedQuantityTemp = sumProductQuantity - (unSuppliedQuantityTemp < 0 ? 0 : unSuppliedQuantityTemp);
                mpOrderProductAdditionalData.suppliedQuantity = suppliedQuantityTemp;

                mpOrderProductAdditionalDataDict.Add(mpOrderProduct.ObaseMalNo, mpOrderProductAdditionalData);
                var productQuantity = orderDto.ProductQuantities?.FirstOrDefault(x => x.ProductId == selectedProductDetail.ObaseMalNo);

                if (mpOrderProductAdditionalData.isUnSupplied)
                {
                    if (mpOrderProductAdditionalData.isWeightedItem)
                    {
                        areUnSuppliedProductsPresent = areUnSuppliedProductsPresent || (mpOrderProductAdditionalData.tySuppliedProductCount != mpOrderProduct.Miktar);
                    }
                    else
                    {
                        areUnSuppliedProductsPresent = true;
                    }

                    int unsuppliedItemCount = 0;
                    foreach (PazarYeriSiparisDetay sipDetay in productDetails)
                    {
                        unsuppliedItemCount++;
                        unsuppliedRequest.Items.Add(new UnsuppliedItem()
                        {
                            Id = Convert.ToInt32(sipDetay.PaketItemId),
                            ReasonId = GetReasonIdOrDefault(productQuantity?.ReasonId)
                        });
                        if (unsuppliedItemCount == mpOrderProductAdditionalData.unSuppliedQuantity)
                            break;

                    }
                }

                totalAmount += productMPUnitPrice * mpOrderProductAdditionalData.suppliedQuantity;
            }

            totalAmount = totalAmount + bagTotalPrice;
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

            if (enableIdefixServices)
            {
                if (unsuppliedRequest.Items.Any())
                {
                    var markShipmentAsUnsuppliedAsync = await MarkShipmentAsUnsuppliedAsync(_apiDefinition.SupplierId, orderEntity.PaketId, unsuppliedRequest);
                    if (markShipmentAsUnsuppliedAsync.Data.Unsupplied)
                    {   //UnSupplied
                        var packageIdList = unsuppliedRequest.Items.Select(x => x.Id.ToString()).ToList();
                        await UpdateOrderWithOrderDetails(nameof(StatusEnums.UnSupplied),
                                           markShipmentAsUnsuppliedAsync.IsSuccessful,
                                           UnSuppliedStatuEnums.TedarikEdilemediVeyaStoktaYok.ToString(),
                                           orderDto.OrderId,
                                           string.Empty,
                                           packageIdList: packageIdList);
                        return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, markShipmentAsUnsuppliedAsync.IsSuccessful, $"{markShipmentAsUnsuppliedAsync.HttpStatusCode}-{"İşlem Başarılı"}");
                    }
                    else
                    {
                        return OrderHelper.ReturnQPResponseV2(HttpStatusCode.InternalServerError, _logFolderName, false, "Tedarik edilemedi bilgisi gönderirken bir hata oluştu: " + markShipmentAsUnsuppliedAsync.ErrorMessages);
                    }
                }
                else
                {
                    //Shipment Invoiced
                    return await HandleShipmentInvoicedAsync(supplierId, packageId, orderDto.OrderId);

                }
            }
            else
            {
                await UpdateOrderWithOrderDetails(nameof(StatusEnums.Prepared), true, null, orderDto.OrderId, string.Empty);
                return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için İdefix üzerinde işlem yapılmamıştır.");
            }
        }


        private async Task<CommonResponseDto> HandleShipmentInvoicedAsync(string supplierId, string packageId, long orderId)
        {

            var invoicedResponse = await SendStatusUpdateAsync(
                supplierId,
                packageId,
                IdefixConstants.OrderState.ShipmentInvoiced,
                "Shipment Invoiced");

            if (invoicedResponse.ResponseMessage.IsSuccessStatusCode)
            {

                await UpdateOrderWithOrderDetails(nameof(StatusEnums.Collected),
                    invoicedResponse.ResponseMessage.IsSuccessStatusCode,
                    null,
                    orderId,
                    invoicedResponse.StringContent);

                string packageStatusMessage = invoicedResponse?.StringContent ?? "";
                return OrderHelper.ReturnQPResponseV2(
                    invoicedResponse.ResponseMessage.StatusCode,
                    _logFolderName,
                    CalcSuccessFromHTTPStatus(invoicedResponse.ResponseMessage.StatusCode),
                    packageStatusMessage
                );
            }
            else
            {
                var errorResponse = LogError("Invoiced", invoicedResponse);
                var qpMessage = ParseFaultResponseMessageUserFriendly(invoicedResponse.StringContent ?? "", false);
                    return OrderHelper.ReturnQPResponseV2(invoicedResponse?.ResponseMessage?.StatusCode ?? HttpStatusCode.InternalServerError
                    , _logFolderName
                    , false
                    , $"Fatura Hazır bilgisi gönderirken bir hata oluştu. Idefix Dönüşü -> <br /> {qpMessage}");
            }
        }

        private async Task HandleOrderStatusUpdateAsync(PazarYeriSiparis createdOrder, string supplierId)
        {

            var pickingResponse = await SendStatusUpdateAsync(
                supplierId,
                createdOrder.PaketId,
                IdefixConstants.OrderState.ShipmentPicking,
                "Picking");

            if (!pickingResponse.ResponseMessage.IsSuccessStatusCode)
            {
                LogError("Picking", pickingResponse);
                return;
            }

            var invoicedResponse = await SendStatusUpdateAsync(
                supplierId,
                createdOrder.PaketId,
                IdefixConstants.OrderState.ShipmentInvoiced,
                "Shipment Invoiced");

            if (invoicedResponse.ResponseMessage.IsSuccessStatusCode)
            {
                await MarkOrderAsInvoicedAsync(createdOrder);
                Logger.Information("SaveIdefixCreatedOrdersAsync > UpdatePackageAsInvoiced Success Status: {statusCode}, Response: {@response}",
                    fileName: _logFolderName, invoicedResponse.ResponseMessage.StatusCode, invoicedResponse?.GetContent());
            }
            else
            {
                LogError("Invoiced", invoicedResponse);
            }
        }

        private async Task<Response<CommonResponseDto>> SendStatusUpdateAsync(string supplierId, string packageId, string status, string logPrefix)
        {
            Logger.Information("OrderUpdatePackageStatus => {prefix} Request SupplierId :{supplierId} PackageId : {packageId}",
                fileName: _logFolderName, logPrefix, supplierId, packageId);

            var response = await _osmIdefixClient.UpdateShipmentStatusAsync(
                supplierId,
                packageId,
                new UpdateShipmentStatusRequest { Status = status });

            Logger.Information("OrderUpdatePackageStatus => {prefix} Response Statu Message : {statusCode}, Response : {@response}",
                fileName: _logFolderName, logPrefix, response.ResponseMessage.StatusCode, response?.StringContent ?? "");

            return response;
        }

        private async Task MarkOrderAsInvoicedAsync(PazarYeriSiparis createdOrder)
        {
            createdOrder.SevkiyatPaketDurumu = nameof(StatusEnums.Invoiced);
            createdOrder.Hata = string.Empty;
            createdOrder.HasSent = Character.E;
            await _pazarYeriSiparisDalService.UpdateOrderAsync(createdOrder);
        }

        private IdefixErrorResponse LogError(string step, Response<CommonResponseDto> response)
        {
            var errorResponse = JsonConvert.DeserializeObject<IdefixErrorResponse>(response.StringContent);

            Logger.Error("OrderUpdatePackageStatus {step} API Request Error with Http Status Code: {statusCode}, API Response: {response}",
                fileName: _logFolderName, step, response.ResponseMessage.StatusCode, errorResponse);

            return errorResponse;
        }
        #endregion

        private int GetReasonIdOrDefault(string reasonId, int defaultValue = 500)
        {
            if (!string.IsNullOrWhiteSpace(reasonId) &&
                int.TryParse(reasonId, out var parsedReasonId))
            {
                return parsedReasonId;
            }

            return defaultValue;
        }
        private List<string> GetUnsuppliedLineItemPackageItemIds(IEnumerable<PazarYeriSiparisUrun> mpOrderProducts, IEnumerable<PazarYeriSiparisDetay> marketPlacesOrderDetails, List<ProductQuantity> qpProductQuantities, IEnumerable<PazarYeriMalTanim> productsWithKGUnit)
        {
            var allOrderItemsLineItemIDs = marketPlacesOrderDetails.ToList().Select(s => s.PaketItemId);
            var SuppliedLineItemItemIds = GetSuppliedLineItemPackageItemIds(mpOrderProducts, marketPlacesOrderDetails, qpProductQuantities, productsWithKGUnit);
            IEnumerable<string> unSuppliedLineItemIds = allOrderItemsLineItemIDs.Except(SuppliedLineItemItemIds);
            return unSuppliedLineItemIds.ToList();
        }

        private List<string> GetSuppliedLineItemPackageItemIds(IEnumerable<PazarYeriSiparisUrun> mpOrderProducts, IEnumerable<PazarYeriSiparisDetay> marketPlacesOrderDetails, List<ProductQuantity> qpProductQuantities, IEnumerable<PazarYeriMalTanim> productsWithKGUnit)
        {
            List<string> suppliedPackageItemIds = new List<string>();

            foreach (var mpOrderProduct in mpOrderProducts)
            {
                ProductQuantity qpProduct = qpProductQuantities.FirstOrDefault(qpX => qpX.ProductId == mpOrderProduct.ObaseMalNo);
                if (qpProduct is null)
                    continue;

                decimal qpProductQuantity = qpProduct.Quantity;
                int tySuppliedProductCount = (int)Math.Round(qpProductQuantity);
                if (productsWithKGUnit.Any(x => x.MalNo == mpOrderProduct.ObaseMalNo))
                {
                    var pyProductUnitValue = productsWithKGUnit.FirstOrDefault(x => x.MalNo == mpOrderProduct.ObaseMalNo).PyUrunSatisDeger ?? 1;

                    tySuppliedProductCount = (int)Math.Ceiling(qpProductQuantity / pyProductUnitValue);
                }

                IEnumerable<string> suppliedProductPackageItemIds = marketPlacesOrderDetails.Where(x => x.ObaseMalNo == mpOrderProduct.ObaseMalNo).Select(mpO => mpO.PaketItemId).Take(tySuppliedProductCount);

                suppliedPackageItemIds.AddRange(suppliedProductPackageItemIds);
            }

            return suppliedPackageItemIds;
        }

        private async Task<bool> SaveOrderDb(IdefixOrderDto itemOrder, string storeId, long qpOrderSeqId, IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts)
        {
            try
            {
                PazarYeriSiparis pazarYeriSiparis;
                string invoiceId = Guid.NewGuid().ToString();
                string shipmentAddressId = Guid.NewGuid().ToString();
                var merchantNo = PazarYeri.Idefix;

                pazarYeriSiparis = new PazarYeriSiparis()
                {
                    PazarYeriNo = merchantNo,
                    Id = qpOrderSeqId,
                    PaketId = itemOrder.Id.ToString(),
                    SiparisId = itemOrder.OrderNumber,
                    SiparisNo = itemOrder.OrderNumber,
                    SiparisTarih = itemOrder.CreatedAt,
                    TahminiTeslimBaslangicTarih = itemOrder.EstimatedDeliveryDate,
                    ToplamIndirimTutar = itemOrder.TotalDiscount,
                    ToplamTutar = itemOrder.TotalPrice,
                    TcKimlikNo = (itemOrder.ShippingAddress?.IdentificationNumber ?? "").Length > 11 ? (itemOrder.ShippingAddress?.IdentificationNumber ?? "")[..11] : (itemOrder.ShippingAddress?.IdentificationNumber ?? ""),
                    MusteriId = itemOrder.CustomerId.ToString(),
                    MusteriAdi = itemOrder.CustomerContactName,
                    MusteriEmail = itemOrder.CustomerContactMail,
                    KargoTakipNo = itemOrder.CargoTrackingNumber,
                    KargoTakipUrl = itemOrder.CargoTrackingUrl,
                    KargoGondericiNumarasi = string.Empty,
                    KargoSaglayiciAdi = itemOrder.CargoCompany,
                    SevkiyatPaketDurumu = itemOrder.Status,
                    KargoAdresId = shipmentAddressId,
                    FaturaAdresId = invoiceId,
                    KoliAdeti = 0,
                    Desi = 0,
                    DepoAktarildiEH = Character.H
                };
                PazarYeriKargoAdres pazarYeriKargoAdres = null;
                if (itemOrder.ShippingAddress != null)
                {
                    pazarYeriKargoAdres = new PazarYeriKargoAdres()
                    {
                        Id = qpOrderSeqId,
                        KargoAdresId = shipmentAddressId,
                        AdSoyad = $"{itemOrder.ShippingAddress.FirstName} {itemOrder.ShippingAddress.LastName}",
                        Ad = itemOrder.ShippingAddress.FirstName,
                        Soyad = itemOrder.ShippingAddress.LastName,
                        Adres1 = itemOrder.ShippingAddress.Address1,
                        Sehir = itemOrder.ShippingAddress.City.Length > 20 ? itemOrder.ShippingAddress.City[..20] : itemOrder.ShippingAddress.City,
                        SehirKod = itemOrder.ShippingAddress.CityPlate,
                        PostaKod = string.IsNullOrEmpty(itemOrder.ShippingAddress?.PostalCode) ? "" : itemOrder.ShippingAddress?.PostalCode.Length > 15 ? itemOrder.ShippingAddress.PostalCode[..15] : itemOrder.ShippingAddress.PostalCode,
                        Semt = itemOrder.ShippingAddress.County,
                        SemtId = itemOrder.ShippingAddress.CountyId,
                        UlkeKod = itemOrder.ShippingAddress.CountryCode,
                        TamAdres = itemOrder.ShippingAddress.FullAddress

                    };
                }
                PazarYeriFaturaAdres pazarYeriFaturaAdres = null;
                if (itemOrder.InvoiceAddress != null)
                {
                    pazarYeriFaturaAdres = new PazarYeriFaturaAdres()
                    {
                        Id = qpOrderSeqId,
                        FaturaAdresId = invoiceId,
                        AdSoyad = $"{itemOrder.InvoiceAddress.FirstName} {itemOrder.InvoiceAddress.LastName}",
                        Adi = itemOrder.InvoiceAddress.FirstName,
                        Soyadi = itemOrder.InvoiceAddress.LastName,
                        Adres1 = itemOrder.InvoiceAddress.Address1,
                        Sehir = itemOrder.InvoiceAddress.City.Length > 20 ? itemOrder.InvoiceAddress.City[..20] : itemOrder.InvoiceAddress.City,
                        PostaKod = string.IsNullOrEmpty(itemOrder.InvoiceAddress?.PostalCode) ? "" : itemOrder.InvoiceAddress?.PostalCode.Length > 15 ? itemOrder.InvoiceAddress.PostalCode[..15] : itemOrder.InvoiceAddress.PostalCode,
                        Semt = itemOrder.ShippingAddress.County,
                        UlkeKod = itemOrder.InvoiceAddress.CountryCode,
                        TamAdres = itemOrder.InvoiceAddress.FullAddress,
                        Firma = string.Empty
                    };
                }

                SachetProduct[] sachetProducts = _apiDefinition.SachetProducts;

                //poşet yok
                //PazarYeriSiparisEkBilgi packagingInfo = new()
                //{
                //	PosetSayisi = 1,
                //	PosetTutari = sachetProducts.Any() ? sachetProducts[0].Price : decimal.Parse("0.5"),
                //	PySiparisNo = itemOrder.OrderNumber,
                //	ObaseSiparisId = qpOrderSeqId,
                //	GonderimUcreti = 0
                //};


                List<PazarYeriSiparisDetay> pazarYeriSiparisDetays = new();
                List<PazarYeriSiparisDetay> pazarYeriSiparisDetayList = pazarYeriSiparisDetays;
                PazarYeriSiparisDetay pazarYeriSiparisDetay;

                List<PazarYeriSiparisUrun> pazarYeriSiparisUrunList = new();
                PazarYeriSiparisUrun pazarYeriSiparisUrun;

                if (itemOrder.Items.Any())
                {
                    string urlSeperator = _appSetting.Value.ImageSize.UrlSeperator;
                    string imageSize = $"{_appSetting.Value.ImageSize.Width}/{_appSetting.Value.ImageSize.Length}/";
                    string resizePathParameter = _appSetting.Value.ImageSize.ResizePathParameter;

                    foreach (var lineItem in itemOrder.Items)
                    {
                        var transferProduct = transferProducts.FirstOrDefault(x => (x.PazarYeriMalNo == lineItem?.Barcode?.Trim() && x.PazarYeriBirimNo == storeId));
                        var product = transferProduct != null ? products.FirstOrDefault(x => x.MalNo.Trim() == transferProduct?.MalNo?.Trim()) : null;


                        if (lineItem.Image.Contains("{size}"))
                        {
                            lineItem.Image = lineItem.Image.Replace("{size}", imageSize);
                        }

                        var itemCount = 1;//idefix de quantiy degeri yok sabit bütün ürünler 1 adet geliyor
                        pazarYeriSiparisUrun = new PazarYeriSiparisUrun()
                        {
                            ObaseSiparisId = qpOrderSeqId,
                            PySiparisNo = itemOrder.OrderNumber,
                            PazarYeriBirimId = storeId,
                            ObaseMalNo = product?.MalNo ?? (lineItem.MerchantSku ?? ""),
                            AltUrunObaseMalNo = string.Empty,
                            PazarYeriMalNo = lineItem.Barcode,
                            AltUrunPazarYeriMalNo = string.Empty,
                            Miktar = itemCount,
                            GuncelMiktar = itemCount,
                            IsAlternativeEH = Character.H,
                            ImageUrl = lineItem.Image
                        };
                        pazarYeriSiparisUrunList.Add(pazarYeriSiparisUrun);

                        pazarYeriSiparisDetay = new PazarYeriSiparisDetay()
                        {
                            Id = qpOrderSeqId,
                            LineItemId = lineItem.Id.ToString(),
                            PaketItemId = lineItem.Id.ToString(),
                            PazarYeriBirimId = storeId,
                            ObaseMalNo = product?.MalNo ?? (lineItem.MerchantSku ?? ""),
                            PazarYeriMalNo = lineItem.MerchantSku,
                            PazarYeriUrunKodu = string.Empty,
                            PazarYeriMalAdi = lineItem.ProductName,
                            AlternatifUrunEH = Character.H,
                            Barkod = lineItem.Barcode?.ToString() ?? string.Empty,
                            Miktar = itemCount,
                            NetTutar = lineItem.Price,
                            IndirimTutar = lineItem.DiscountedTotalPrice,
                            BrutTutar = lineItem.DiscountedTotalPrice,
                            KdvOran = lineItem.VatRate,
                            ParaBirimiKodu = string.Empty,
                            SatisKampanyaId = string.Empty,
                            UrunBoyutu = string.Empty,
                            UrunRengi = string.Empty,
                            SiparisUrunDurumAdi = string.Empty,
                            IsCancelledEH = Character.H,
                            IsAlternativeEH = Character.H,
                            IsCollectedEH = Character.H,
                        };

                        pazarYeriSiparisDetayList.Add(pazarYeriSiparisDetay);
                    }

                    //poşet yok
                    //pazarYeriSiparisUrun = new PazarYeriSiparisUrun()
                    //{
                    //	ObaseSiparisId = qpOrderSeqId,
                    //	PySiparisNo = itemOrder.OrderNumber,
                    //	PazarYeriBirimId = storeId,
                    //	ObaseMalNo = sachetProducts.Any() ? sachetProducts[0].ProductCode : "",
                    //	AltUrunObaseMalNo = string.Empty,
                    //	PazarYeriMalNo = sachetProducts.Any() ? sachetProducts[0].ProductCode : "",
                    //	AltUrunPazarYeriMalNo = string.Empty,
                    //	Miktar = 1,
                    //	GuncelMiktar = 1,
                    //	IsAlternativeEH = Character.H,
                    //	ImageUrl = "",
                    //
                    //};
                    //pazarYeriSiparisUrunList.Add(pazarYeriSiparisUrun);

                }
                #region Db Save
                await _transactionDalService.BeginTransactionAsync();

                await _pazarYeriSiparisDalService.AddOrderAsync(pazarYeriSiparis);
                await _pazarYeriSiparisDetayDalService.AddOrderDetailsAsync(pazarYeriSiparisDetayList);
                if (pazarYeriSiparisUrunList is not null)
                {

                    //var urunList = pazarYeriSiparisUrunList.DistinctBy(x => new
                    //{
                    //    x.ObaseSiparisId,
                    //    x.ObaseMalNo,
                    //    x.PazarYeriMalNo,
                    //    x.PySiparisNo
                    //}).ToList();


                    var urunList = pazarYeriSiparisUrunList
                              .GroupBy(x => new
                              {
                                  x.ObaseSiparisId,
                                  x.ObaseMalNo,
                                  x.PazarYeriMalNo,
                                  x.PySiparisNo
                              })
                              .Select(g =>
                              {
                                  var first = g.First();
                                  return new PazarYeriSiparisUrun
                                  {
                                      ObaseSiparisId = g.Key.ObaseSiparisId,
                                      ObaseMalNo = g.Key.ObaseMalNo,
                                      PazarYeriMalNo = g.Key.PazarYeriMalNo,
                                      PySiparisNo = g.Key.PySiparisNo,
                                      Miktar = g.Sum(x => x.Miktar ?? 0),
                                      PazarYeriBirimId = first.PazarYeriBirimId,
                                      AltUrunPazarYeriMalNo = first.AltUrunPazarYeriMalNo,
                                      GuncelMiktar = first.GuncelMiktar,
                                      IsCancelledEH = first.IsCancelledEH,
                                      IsAlternativeEH = first.IsAlternativeEH,
                                      IsCollectedEH = first.IsCollectedEH,
                                      ImageUrl = first.ImageUrl,
                                      AltUrunMiktar = first.AltUrunMiktar,
                                      MinMiktar = first.MinMiktar,
                                      MaxMiktar = first.MaxMiktar,
                                      AltUrunObaseMalNo = first.AltUrunObaseMalNo
                                  };
                              })
                              .ToList();

                    await _pazarYeriSiparisUrunDalService.AddProductsAsync(urunList);
                }

                //poşet yok
                //await _pazarYeriSiparisEkBilgiDalService.AddAdditionalDataAsync(packagingInfo);
                if (pazarYeriFaturaAdres is not null)
                    await _pazarYeriFaturaAdresDalService.AddInvoiceAddressAsync(pazarYeriFaturaAdres);
                if (pazarYeriKargoAdres is not null)
                    await _pazarYeriKargoAdresDalService.AddShipmentAddressAsync(pazarYeriKargoAdres);
                await _transactionDalService.CommitTransactionAsync();
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                await SendFailedOrderMailFormattedAsync("Idefix Servisinde Hata", $"Hata! Sipariş Db'ye kaydedilemedi.", itemOrder, ex);
                Logger.Error("SaveOrderDb Exception {exception} for Order: {order}", fileName: _logFolderName, ex, itemOrder.OrderNumber);
                await _transactionDalService.RollbackTransactionAsync();
                return false;
            }
        }

        private async Task<bool> SendOrderToQp(PazarYeriBirimTanim pyBirimTanim, IEnumerable<PazarYeriMalTanim> pyProductList, IEnumerable<PazarYeriAktarim> pyTransferredProductList, long qpOrderSeqId, SachetProduct[] sachetProduct, IdefixOrderDto orderDto, StringBuilder errorMessages = null, string merchantNo = "")
        {
            bool isSendToQp = false;
            try
            {
                orderDto.CargoKey = _appSetting?.Value?.CargoProductCode ?? string.Empty;
                var qpModel = _orderConvertService.ToQpOrder(orderDto, pyBirimTanim, pyProductList, pyTransferredProductList, qpOrderSeqId, merchantNo: PazarYeri.Idefix, sachetProduct: _apiDefinition.SachetProducts);

                QPService.DeliveryRequest consumeDeliveryRequest = new QPService.DeliveryRequest() { DeliveryAction = qpModel };
                Logger.Information("Request sent to QP: {@request} ", fileName: _logFolderName, qpModel);
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
                         Logger.Warning("Idefix - SendOrderToQp - Retry attempt {Attempt} will be made after {Delay}s. Reason: {Message}", fileName: _logFolderName, args.AttemptNumber.ToString(), args.RetryDelay.TotalSeconds.ToString("F1"), args.Outcome.Exception?.Message ?? "Unknown error");
                         return ValueTask.CompletedTask;
                     }
                 }).Build();

                await pipeline.ExecuteAsync(async cancellationToken =>
                {

                    try
                    {
                        var resp = await _qpClient.ConsumeDeliveryAsync(consumeDeliveryRequest);
                        string consumeDeliveryResult = resp.ConsumeDeliveryResult.Trim();
                        Logger.Information("Reponse ConsumeDeliveryAsync to QP:{qpResponse} ", fileName: _logFolderName, consumeDeliveryResult);


                        if (string.Equals(consumeDeliveryResult, "1") || consumeDeliveryResult.Contains("BURST REQUEST DETECTED!", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Information("QP Success With Id: {qpOrderSeqId}.", fileName: _logFolderName, qpOrderSeqId);
                            var orderItem = await _pazarYeriSiparisDalService.GetOrderByIdAsync(qpOrderSeqId);
                            isSendToQp = true;
                            return;
                        }

                        string errorMessage = $"Sipariş Quickpick'e aktarılırken bir hata oluştu.Sipariş No: {orderDto.OrderNumber}. Hata:{consumeDeliveryResult}";
                        if (errorMessages != null && !errorMessages.ToString().Contains(errorMessage))
                            errorMessages.AppendLine(errorMessage);
                        throw new InvalidOperationException($"Idefix - SendOrderToQp - Sipariş Quickpick'e aktarılırken bir hata oluştu ve tekrar gönderim sağlanacak. QP Response :{errorMessage} ");

                    }
                    catch (Exception ex)
                    {
                        Logger.Error("QP Error With Id: {qpOrderSeqId} For Order Number: {orderNumber}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, orderDto.OrderNumber, ex);
                        throw new InvalidOperationException($"Idefix - SendOrderToQp - Sipariş Quickpick'e aktarılırken bir hata oluştu ve tekrar gönderim sağlanacak. Error :{ex.Message + " " + ex.InnerException?.Message ?? ""} ");
                    }
                });



            }
            catch (Exception ex)
            {
                string errorMessage = $"Sipariş Quickpick'e aktarılırken veya Veritabanında yazılırken bir hata oluştu.Sipariş No: {orderDto.OrderNumber}. Hata:{ex}";
                if (errorMessages != null)
                    errorMessages.AppendLine($"Sipariş Quickpick'e aktarılırken hata alındı. Sipariş No: {orderDto.OrderNumber}. Hata: {ex.Message} Detay: {ex.InnerException?.Message ?? ""}");
                Logger.Error("QP Error With Id: {qpOrderSeqId} for Order Number: {orderNumber}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, orderDto.OrderNumber, ex);

            }

            return isSendToQp;
        }


        private async Task SendFailedOrderMailFormattedAsync(string subject, string message, IdefixOrderDto? orderDto = null, Exception? ex = null)
        {
            try
            {
                string body = "<table>";

                body += $"<tr><td>Tarih:</td><td>{DateTime.Now}</td></tr>";

                if (orderDto is not null)
                {
                    body += $"<tr><td>Order Id:</td><td>{orderDto.Id}</td></tr>";
                    body += $"<tr><td>Order OrderNumber:</td><td>{orderDto.OrderNumber}</td></tr>";
                }

                body += $"<tr><td>Hata:</td><td>{message}</td></tr>";

                if (ex is not null)
                {
                    body += $"<tr><td>Exception Message:</td><td>{ex.Message}</td></tr>";
                    body += $"<tr><td>Inner Exception:</td><td>{ex.InnerException?.Message}</td></tr>";
                    body += $"<tr><td>Stack Trace:</td><td>{ex.StackTrace}</td></tr>";
                }

                body += "</table>";

                Logger.Information($"failed order mail contents -> subject:{subject} - body: {body}", fileName: _logFolderName);
                await SendFailedOrderMail(subject, body);
            }
            catch (Exception exc)
            {
                Logger.Error("An exception occurred while sending failed order email: {exception}", fileName: _logFolderName, exc);
            }
        }

        private string ParseFaultResponseMessageUserFriendly(string response , bool isSuccessStatusCode)
        {
            try
            {
                if(isSuccessStatusCode)
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
                    IdefixErrorResponse? faultWrapperDto = Newtonsoft.Json.JsonConvert.DeserializeObject<IdefixErrorResponse>(response);

                    if (faultWrapperDto == null)
                        return $"OBASE Parser: Bozuk json içerik! (Idefix) <br /> Response: {response}";

                    return $"Kod: {faultWrapperDto.Code} - Hata Kodu: {faultWrapperDto.ErrorCode} <br /> Hata Mesajı: {faultWrapperDto.Message} <br /> ApiResponse: {response}";
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