using Hangfire.Server;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Helper;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using OBase.Pazaryeri.Business.Services.Abstract.Return;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using OBase.Pazaryeri.Domain.Dtos.Getir;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using Polly;
using Polly.Retry;
using RestEase;
using System.Collections.Concurrent;
using System.Net;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Constants.Constants.GetirConstants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using static OBase.Pazaryeri.Domain.Helper.CommonHelper;

namespace OBase.Pazaryeri.Business.Services.Concrete.Order
{
	public class GetirCarsiOrderService : BaseService, IGetirCarsiOrderService
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
		private readonly IGetirCarsiClient _getirCarsiClient;
		private readonly ITransactionDalService _transactionDalService;
		private readonly IGetirCarsiReturnService _getirReturnService;
		private readonly IGetDalService _getDalService;
		private readonly ApiDefinitions _apiDefinition;
		private readonly QPService.OrderDeliveryServiceSoapClient _qpClient;
		private readonly string _logFolderName = nameof(PazarYerleri.GetirCarsi);

        private readonly IOrderConvertService _orderConvertService;


        #endregion

        #region Ctor

        public GetirCarsiOrderService(IGetirCarsiClient getirCarsiClient, IPazarYeriBirimTanimDalService pazarYeriBirimTanimDalService,
			IPazarYeriMalTanimDalService pazarYeriMalTanimDalService, IPazarYeriSiparisDalService pazarYeriSiparisDalService,
			IPazarYeriAktarimDalService pazarYeriAktarimDalService, ITransactionDalService transactionDalService, IPazarYeriFaturaAdresDalService pazarYeriFaturaAdresDalService,
			IPazarYeriKargoAdresDalService pazarYeriKargoAdresDalService, IPazarYeriSiparisEkBilgiDalService pazarYeriSiparisEkBilgiDalService,
			IPazarYeriSiparisUrunDalService pazarYeriSiparisUrunDalService,
			IPazarYeriSiparisDetayDalService pazarYeriSiparisDetayDalService, IOptions<AppSettings> options, IGetirCarsiReturnService getirReturnService, IGetDalService getDalService, IMailService mailService, IOrderConvertService orderConvertService) : base(pazarYeriSiparisDalService, pazarYeriSiparisDetayDalService, options, mailService)
		{
			_getirCarsiClient = getirCarsiClient;
			_pazarYeriBirimTanimDalService = pazarYeriBirimTanimDalService;
			_pazarYeriMalTanimDalService = pazarYeriMalTanimDalService;
			_pazarYeriAktarimDalService = pazarYeriAktarimDalService;
			_transactionDalService = transactionDalService;
			_pazarYeriFaturaAdresDalService = pazarYeriFaturaAdresDalService;
			_pazarYeriKargoAdresDalService = pazarYeriKargoAdresDalService;
			_pazarYeriSiparisEkBilgiDalService = pazarYeriSiparisEkBilgiDalService;
			_pazarYeriSiparisUrunDalService = pazarYeriSiparisUrunDalService;
			_appSetting = options;
			_apiDefinition = _appSetting.Value.ApiDefinitions.FirstOrDefault(x => x.Merchantno == PazarYeri.GetirCarsi);
			if (_appSetting is not null)
			{
				_qpClient = new QPService.OrderDeliveryServiceSoapClient(QPService.OrderDeliveryServiceSoapClient.EndpointConfiguration.OrderDeliveryServiceSoap12, remoteAddress: _appSetting.Value.WareHouseUrl);
			}

			_getirReturnService = getirReturnService;
			_getDalService = getDalService;
			_orderConvertService = orderConvertService;
		}

		#endregion

		#region Methods

		public async Task<ServiceResponse<CommonResponseDto>> SaveOrderOnQp(GetirOrderDto order)
		{
			if (order == null)
			{
                await SendFailedOrderMail("GetirÇarşı Siparişi Kaydedilirken Hata", "Gönderilen sipariş nesnesi boş olamaz.");
                return ServiceResponse<CommonResponseDto>.Error("Gönderilen sipariş nesnesi boş olamaz.");
			}
			if (!order.products.Any())
			{
                await SendFailedOrderMail("GetirÇarşı Siparişi Kaydedilirken Hata", "Ürün listesi boş olamaz.");
                return ServiceResponse<CommonResponseDto>.Error("Ürün listesi boş olamaz.");
			}
			try
			{
				Logger.Information("GetirCarsiOrderService SaveOrderOnQp Request : {@request} ", fileName: _logFolderName, order);

				long qpOrderSeqId = 0;
				bool sendToQp = _apiDefinition.OrderSendToQp;
				string merchantNo = PazarYeri.GetirCarsi;
				var pyBirimTanimDto = await _pazarYeriBirimTanimDalService.GetStoreDetailAsync(merchantNo, order.shopId);
				var pyProductList = await _pazarYeriMalTanimDalService.GetPyProductsAsync(merchantNo);
				var pyTransferredProductList = await _pazarYeriAktarimDalService.GetPyTransferredProductsAsync(merchantNo, order.shopId);
				var dbSaveResult = false;
				var hasOrder = await _pazarYeriSiparisDalService.OrderExistAsync(order.orderId, merchantNo);
				if (order.orderType != GetirConstants.Unapproved && !hasOrder)
				{
					string errorMessage = "The canceled order from Getir could not be found in the db for Order Id: {orderId}, Order Number: {orderNo}";
                    await SendFailedOrderMail("GetirÇarşı Siparişi Kaydedilirken Hata", $"The canceled order from Getir could not be found in the db for Order Id: {order.orderId}, Order Number: {order.orderNumber}");
                    Logger.Warning(errorMessage, fileName: _logFolderName, order.orderId, order.orderNumber);
					return ServiceResponse<CommonResponseDto>.Error($"The canceled order from Getir could not be found in the db for Order Id: {order.orderId}, Order Number: {order.orderNumber}");
				}
				else if (order.orderType == GetirConstants.Unapproved && !hasOrder)
				{
					qpOrderSeqId = await _pazarYeriSiparisDalService.GetSeqId();
					dbSaveResult = await SaveOrderDb(order, qpOrderSeqId, pyProductList, pyTransferredProductList);
				}
				else if (order.orderType.Contains(GetirConstants.Cancelled) && hasOrder)
				{
					var orderDb = await _pazarYeriSiparisDalService.GetOrderWithOrderIdAsync(order.orderId, merchantNo);
					if (!orderDb.SevkiyatPaketDurumu.Contains(GetirConstants.Cancelled))
					{
						orderDb.SevkiyatPaketDurumu = order.orderType;
						if (orderDb.DepoAktarildiEH == Character.E && sendToQp)
						{
							var response = await _qpClient.CancelOrderAsync(orderDb.Id.ToString(), "1");
							if (response.Response)
							{
                                Logger.Information("Order is cancelled. QP Success With ID: {qpOrderId}", fileName: _logFolderName, orderDb.Id);
							}
							else
							{
                                await SendFailedOrderMail("GetirÇarşı Siparişi Kaydedilirken Hata", $"Order is not cancelled. QP Error With ID: {orderDb.Id} response:{response}");
                                Logger.Error("Order is not cancelled. QP Error With ID: {qpOrderId} response:{response}", fileName: _logFolderName, orderDb.Id, response.Message);
							}
						}
						await _pazarYeriSiparisDalService.UpdateOrderAsync(orderDb);
					}
                    dbSaveResult = true;
                }
				else
				{
					qpOrderSeqId = await _pazarYeriSiparisDalService.GetOrderIdByIdAsync(order.orderId, merchantNo);
					dbSaveResult = true;
				}
				long orderCountTransferredToQP = await _pazarYeriSiparisDalService.GetOrderWareHouseTransferredCountAsync(order.orderNumber.ToString(), merchantNo);
				bool _sendQp = sendToQp && orderCountTransferredToQP < 1;
				if (!dbSaveResult)
				{
                    await SendFailedOrderMail("GetirÇarşı Siparişi Kaydedilirken Hata", $"Sipariş kaydedilirken bir hata oluştu. Lütfen tekrar deneyiniz. HttpStatusCode:{HttpStatusCode.InternalServerError}");
                    return ServiceResponse<CommonResponseDto>.Error("Sipariş kaydedilirken bir hata oluştu. Lütfen tekrar deneyiniz.", httpStatusCode: HttpStatusCode.InternalServerError);
				}
				else if (_sendQp)
				{

                    var sendQpResult = await SendOrderToQp(pyBirimTanimDto, pyProductList, pyTransferredProductList, qpOrderSeqId, order,sachetProducts: _apiDefinition.SachetProducts);
					if (sendQpResult)
					{
                        var orderItem = await _pazarYeriSiparisDalService.GetOrderByIdAsync(qpOrderSeqId);
                        if (orderItem is not null)
                        {
                            orderItem.DepoAktarildiEH = Character.E;
                            await _pazarYeriSiparisDalService.UpdateOrderAsync(orderItem);
                        }
                        return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = "Sipariş Kaydedildi", Success = true });

                    }

                    else
                    {
                        return ServiceResponse<CommonResponseDto>.Error("An error occurred while saving the order. Please try again.", httpStatusCode: HttpStatusCode.InternalServerError);
                    }
				}
				else
				{
                    Logger.Information("Sipariş işlemleri tamamlandı. Siparis Numarası: {orderNumber}", _logFolderName, order.orderNumber);
					return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = "Sipariş işlemleri tamamlandı.", Success = true });
				}
			}
			catch (Exception ex)
			{
                await SendFailedOrderMail("GetirÇarşı Siparişi Kaydedilirken Hata", $"Hata! Sipariş Db'ye kaydedilemedi. Sipariş No: {order?.orderNumber ?? 0} \nException: {ex.Message} - {ex.InnerException?.Message ?? ""} ");
                Logger.Error("GetirApiController > CreateOrderFromWebHook Hata {exception} ", fileName: _logFolderName, ex);
				return ServiceResponse<CommonResponseDto>.Error(ex.Message, httpStatusCode: HttpStatusCode.InternalServerError);
			}
		}
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
				IEnumerable<PazarYeriSiparisDetay> marketPlaceOrderItems = new List<PazarYeriSiparisDetay>();
				IEnumerable<PazarYeriSiparisUrun> mpOrderProducts = new List<PazarYeriSiparisUrun>();
				string shopId = orderEntity.PazarYeriSiparisDetails.FirstOrDefault().PazarYeriBirimId;
				string pyOrderID = orderEntity.SiparisId;

				switch (status)
				{
					case StatusEnums.Picking:
						{
							if (enableMarketPlaceServices)
							{
								Logger.Information("OrderUpdatePackageStatus => Verify Request for ShopId: {shopId} and OrderId: {pyOrderID}", fileName: _logFolderName, shopId, pyOrderID);
								var getirResponse = await _getirCarsiClient.Verify(pyOrderID, shopId);
								Logger.Information("OrderUpdatePackageStatus => Verify Response StatusCode : {statusCode}, Response : {@response}", fileName: _logFolderName, getirResponse.ResponseMessage.StatusCode, getirResponse?.StringContent ?? "");

								await UpdateOrderWithOrderDetails(nameof(StatusEnums.Verified), getirResponse.ResponseMessage.IsSuccessStatusCode, null, orderDto.OrderId, getirResponse.StringContent);

								return OrderHelper.ReturnQPResponseV2(getirResponse.ResponseMessage.StatusCode, _logFolderName, CalcSuccessFromHTTPStatus(getirResponse.ResponseMessage.StatusCode), getirResponse.GetContent()?.Meta?.returnMessage ?? "");
							}
							else
							{
								await UpdateOrderWithOrderDetails(nameof(StatusEnums.Verified), true, null, orderDto.OrderId, string.Empty);
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için Getir üzerinde işlem yapılmamıştır.");
							}
						}
					case StatusEnums.Collected:
						{
							SachetProduct[] sachetProducts = _apiDefinition.SachetProducts;
							Dictionary<string, string> productNoMap = new Dictionary<string, string>();
							int bagCount = 0;
							decimal bagTotalPrice = 0;

							foreach (ProductQuantity productQuantity in orderDto.ProductQuantities)
							{
								if (sachetProducts.Any(sp => sp.ProductCode == productQuantity.ProductId))
								{
									SachetProduct sachetProduct = sachetProducts.FirstOrDefault(sp => sp.ProductCode == productQuantity.ProductId);
									decimal sachetPrice = 0;
									if (sachetProduct != null) { sachetPrice = sachetProduct.Price; }
									int bagQuantity = (int)Math.Round(productQuantity.Quantity);
									bagCount += bagQuantity;
									bagTotalPrice += bagQuantity * sachetPrice;
									continue;
								}
								string strProductNo = productQuantity.ProductId.Trim();
								string productMarketPlaceId = await _pazarYeriMalTanimDalService.GetProductMarketPlaceIdByObaseProductIdAsync(strProductNo, _apiDefinition.Merchantno);
								productNoMap.Add(strProductNo, productMarketPlaceId);
							}
							PrepareDto getirPrepareRequest = new PrepareDto();
							IEnumerable<PazarYeriMalTanim> productsWithKGUnit = await _pazarYeriMalTanimDalService.GetProductSalesValueByUnitAsync(CommonConstants.KG, _apiDefinition.Merchantno);
							marketPlaceOrderItems = orderEntity.PazarYeriSiparisDetails;
							mpOrderProducts = await _pazarYeriSiparisUrunDalService.GetOrderProductsByIdAsync(orderDto.OrderId);
							Dictionary<string, TGProductAdditionalDataDto> mpOrderProductAdditionalDataDict = new Dictionary<string, TGProductAdditionalDataDto>();
							decimal totalAmount = 0;
							foreach (var mpOrderProduct in mpOrderProducts)
							{
								ProductQuantity qpProduct = orderDto.ProductQuantities.FirstOrDefault(qpX => qpX.ProductId == mpOrderProduct.ObaseMalNo);
								var selectedProductDetail = orderEntity.PazarYeriSiparisDetails.Where(x => x.ObaseMalNo == qpProduct.ProductId).MaxBy(x => x.Miktar);
								ProductQuantity qpAltProduct = orderDto.ProductQuantities.FirstOrDefault(qpX => qpX.AltProductId == mpOrderProduct.ObaseMalNo) ?? null;
								TGProductAdditionalDataDto mpOrderProductAdditionalData = new TGProductAdditionalDataDto();
								bool hasAlternativeGivenForThisProduct = false;
								if (qpAltProduct is not null && qpAltProduct.Quantity > 0)
								{
									hasAlternativeGivenForThisProduct = true;
									mpOrderProductAdditionalData.AltProductId = qpAltProduct.ProductId;
									mpOrderProductAdditionalData.altSuppliedQuantity = qpAltProduct.Quantity;
								}
								mpOrderProductAdditionalData.orderedQuantity = mpOrderProduct.Miktar ?? 0;
								mpOrderProductAdditionalData.isWeightedItem = productsWithKGUnit.Any(x => x.MalNo == mpOrderProduct.ObaseMalNo);
								var pyProductUnitValue = productsWithKGUnit.FirstOrDefault(x => x.MalNo == mpOrderProduct.ObaseMalNo)?.PyUrunSatisDeger ?? 1;
								mpOrderProductAdditionalData.tyQuantityCoefficient = (mpOrderProductAdditionalData.isWeightedItem) ? pyProductUnitValue : 1;
								decimal suppliedQuantityTemp = 0;
								decimal productMPUnitPrice = (selectedProductDetail.NetTutar / selectedProductDetail.Miktar);

								if (hasAlternativeGivenForThisProduct)
								{
									try
									{
										PazaryeriAlternatifGonderim getirAltProductEntity = null;
										getirAltProductEntity = await _getDalService.GetAsync<PazaryeriAlternatifGonderim>(x => x.PazarYeriBirimNo == shopId && x.PazarYeriMalNo == productNoMap[qpAltProduct.ProductId]);
										if (getirAltProductEntity is null)
										{
											var productGeneralResponse = await _getirCarsiClient.Products(shopId);
											int size = productGeneralResponse?.GetContent()?.Data?.TotalCount ?? 1;
											var allProductsForShopResponse = await _getirCarsiClient.Products(shopId, 1, size);
											var allProductsForShop = allProductsForShopResponse?.GetContent()?.Data.Data;
											var getirProduct = allProductsForShop.FirstOrDefault(x => x.VendorId == productNoMap[qpAltProduct.ProductId]);

											if (getirProduct is not null)
											{
												var productOption = getirProduct.MenuOptions.MaxBy(x => x.Amount);
												getirAltProductEntity = new PazaryeriAlternatifGonderim
												{
													OptionId = productOption.OptionId,
													CatalogProductId = getirProduct.CatalogProductId,
													MenuProductId = getirProduct.MenuProductId,
													OptionPrice = productOption.Price,
													OptionAmount = productOption.Amount
												};
											}
										}
										if (getirAltProductEntity is null)
										{
											return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "Alternatif olarak verilen ürün Getir'de tanımlı değil.");
										}
										int newCount = 0;
										getirPrepareRequest.AlternativeProducts ??= new List<AlternativeProduct>();


										newCount = mpOrderProductAdditionalData.isWeightedItem ? Convert.ToInt32(Math.Ceiling(qpAltProduct.Quantity * 1000 / getirAltProductEntity.OptionAmount)) : Convert.ToInt32(qpAltProduct.Quantity);

										int? newWeight = mpOrderProductAdditionalData.isWeightedItem ? Convert.ToInt32(qpAltProduct.Quantity * 1000) : null;

										decimal optionUnitPrice = getirAltProductEntity.OptionPrice / getirAltProductEntity.OptionAmount;
										getirPrepareRequest.AlternativeProducts.Add(new AlternativeProduct
										{
											SourceId = selectedProductDetail.LineItemId,
											ProductId = getirAltProductEntity.MenuProductId,
											SelectedAmount = new SelectedAmount
											{
												Id = getirAltProductEntity.OptionId,
												Price = getirAltProductEntity.OptionPrice,
												NewPrice = optionUnitPrice * newWeight is not null ? decimal.Round((optionUnitPrice * newWeight) ?? 0, 2, MidpointRounding.AwayFromZero) : null,
												NewCount = newCount,
												NewWeight = newWeight
											}
										});
										suppliedQuantityTemp = mpOrderProductAdditionalData.altSuppliedQuantity + qpProduct.Quantity;
										mpOrderProductAdditionalData.suppliedQuantity = suppliedQuantityTemp;
										mpOrderProductAdditionalData.isAlternateProductGiven = true;
									}
									catch (Exception ex)
									{
										Logger.Error("Alternatif ürün eklenirken bir hata oluştu:{exception}", _logFolderName, ex);
										return OrderHelper.ReturnQPResponseV2(HttpStatusCode.InternalServerError, _logFolderName, false, "Alternatif ürün eklenirken bir hata oluştu: " + ex.Message);
									}
								}
								else
								{
									suppliedQuantityTemp = qpProduct.Quantity;
									mpOrderProductAdditionalData.suppliedQuantity = suppliedQuantityTemp;
								}
								decimal unSuppliedQuantityTemp = (mpOrderProduct.Miktar ?? 0) - suppliedQuantityTemp;
								mpOrderProductAdditionalData.unSuppliedQuantity = (unSuppliedQuantityTemp < 0) ? 0 : unSuppliedQuantityTemp;
								mpOrderProductAdditionalData.isUnSupplied = mpOrderProductAdditionalData.unSuppliedQuantity > 0;
								mpOrderProductAdditionalData.isFullyUnSupplied = suppliedQuantityTemp <= 0;
								mpOrderProductAdditionalDataDict.Add(mpOrderProduct.ObaseMalNo, mpOrderProductAdditionalData);
								if (mpOrderProductAdditionalData.isUnSupplied || mpOrderProductAdditionalData.isAlternateProductGiven || mpOrderProductAdditionalData.suppliedQuantity > mpOrderProductAdditionalData.orderedQuantity)
								{
									getirPrepareRequest.UpdatedProducts.Add(new UpdatedProduct
									{
										Id = selectedProductDetail.LineItemId,
										NewCount = mpOrderProductAdditionalData.isWeightedItem ? null : Convert.ToInt32(qpProduct.Quantity),
										NewPrice = mpOrderProductAdditionalData.isWeightedItem ? decimal.Round(productMPUnitPrice * (qpProduct.Quantity), 2, MidpointRounding.AwayFromZero) : null,
										NewTotalWeight = mpOrderProductAdditionalData.isWeightedItem ? Convert.ToInt32(qpProduct.Quantity * 1000) : null
									});
								}
								if (hasAlternativeGivenForThisProduct)
								{
									try
									{
										decimal altProductUnitPrice = await _pazarYeriSiparisUrunDalService.GetSubProductPriceInfoAsync(qpAltProduct.ProductId, PazarYeri.GetirCarsi);
										totalAmount = totalAmount + (productMPUnitPrice * (mpOrderProductAdditionalData.suppliedQuantity - mpOrderProductAdditionalData.altSuppliedQuantity)) +
											(altProductUnitPrice * mpOrderProductAdditionalData.altSuppliedQuantity);
									}
									catch (Exception ex)
									{
										Logger.Error("OrderUpdatePackageStatus> hasAlternativeGivenForThisProduct Hata:{exception}", _logFolderName, ex);
										return OrderHelper.ReturnQPResponseV2(HttpStatusCode.InternalServerError, _logFolderName, false, "Alternatif ürünün fiyatı alınırken bir hata oluştu: " + ex.Message);
									}
								}
								else
								{
									totalAmount += productMPUnitPrice * mpOrderProductAdditionalData.suppliedQuantity;
								}
							}
							totalAmount += bagTotalPrice;
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
								foreach (var mpOrderProduct in mpOrderProducts)
								{
									TGProductAdditionalDataDto productAdditionalData = mpOrderProductAdditionalDataDict[mpOrderProduct.ObaseMalNo];
									var product = await _pazarYeriSiparisUrunDalService.GetOrderProductByIdAsync(orderDto.OrderId, mpOrderProduct.ObaseMalNo);
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
								getirPrepareRequest.PackagingUpdate = new PackagingUpdate
								{
									NewBagCount = bagCount,
									NewTotalPackagingPrice = Convert.ToDecimal(bagTotalPrice)
								};

								Logger.Information("OrderUpdatePackageStatus => Prepare Request: {@request}", _logFolderName, getirPrepareRequest);

								var getirResponse = await _getirCarsiClient.Prepare(orderEntity.SiparisId, shopId, getirPrepareRequest);

								Logger.Information("OrderUpdatePackageStatus => Prepare Response StatusCode: {statusCode}, Response: {response}", _logFolderName, getirResponse.ResponseMessage.StatusCode, getirResponse?.StringContent ?? string.Empty);


								await UpdateOrderWithOrderDetails(nameof(StatusEnums.Prepared), getirResponse.ResponseMessage.IsSuccessStatusCode, null, orderDto.OrderId, getirResponse.StringContent);

								string qpMessage = getirResponse?.GetContent()?.Meta?.returnMessage ?? "";
								string returnCode = getirResponse?.GetContent()?.Meta?.returnCode ?? "";
								if (returnCode == "115"
									|| returnCode == "113"
									|| qpMessage.Contains("calculated new"))
								{
									qpMessage = $"Sipariş Min:{orderEntity.MinTutar?.ToString("0.00")} - Maks {orderEntity.MaksTutar?.ToString("0.00")} TL aralığında toplanabilir.";
								}

								return OrderHelper.ReturnQPResponseV2(getirResponse.ResponseMessage.StatusCode, _logFolderName, CalcSuccessFromHTTPStatus(getirResponse.ResponseMessage.StatusCode), qpMessage);
							}
							else
							{
								await UpdateOrderWithOrderDetails(nameof(StatusEnums.Prepared), true, null, orderDto.OrderId, string.Empty);
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için Getir üzerinde işlem yapılmamıştır.");
							}
						}
					case StatusEnums.Cancelled:
						{
							if (enableMarketPlaceServices)
							{
								var cancelDto = new CancelDto
								{
									cancelNote = orderDto.CancelNote,
									cancelReasonId = orderDto.ReasonId,
									products = orderEntity.PazarYeriSiparisDetails.Select(s => s.LineItemId).ToArray()
								};
								Logger.Information("OrderUpdatePackageStatus => Cancel Request: {@request}", _logFolderName, cancelDto);
								var cancelledResult = await _getirCarsiClient.Cancel(pyOrderID, shopId, cancelDto);
								Logger.Information("OrderUpdatePackageStatus => Cancel Response StatusCode: {statusCode}, Response: {@response}", _logFolderName, cancelledResult.ResponseMessage.StatusCode, cancelledResult?.StringContent ?? "");

								await UpdateOrderWithOrderDetails(nameof(StatusEnums.Cancelled), cancelledResult.ResponseMessage.IsSuccessStatusCode, orderDto.ReasonId, orderDto.OrderId, cancelledResult.StringContent, null);


								return OrderHelper.ReturnQPResponseV2(cancelledResult.ResponseMessage.StatusCode, _logFolderName, CalcSuccessFromHTTPStatus(cancelledResult.ResponseMessage.StatusCode), cancelledResult?.GetContent()?.Meta?.returnMessage ?? "");
							}
							else
							{
								await UpdateOrderWithOrderDetails(nameof(StatusEnums.Cancelled), true, null, orderDto.OrderId, string.Empty, null);
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için Getir üzerinde işlem yapılmamıştır.");
							}
						}
					case StatusEnums.InTransit:
						{
							if (enableMarketPlaceServices)
							{
								if (orderEntity.KargoSaglayiciAdi == DeliveryTypeCode.GetirGetirsin)
								{
									Response<GetirResponse> getirResponse;
									Logger.Information("OrderUpdatePackageStatus => Handover - Getir Getirsin Request for ShopId: {shopId} and OrderId: {pyOrderID}", fileName: _logFolderName, shopId, pyOrderID);
									getirResponse = await _getirCarsiClient.Handover(pyOrderID, shopId);

									Logger.Information("OrderUpdatePackageStatus => Handover Response StatusCode : {statusCode}, Response : {response}", fileName: _logFolderName, getirResponse.ResponseMessage.StatusCode, getirResponse?.StringContent ?? "");

									await UpdateOrderWithOrderDetails(nameof(StatusEnums.InTransit), getirResponse.ResponseMessage.IsSuccessStatusCode, null, orderDto.OrderId, getirResponse.StringContent);

									return OrderHelper.ReturnQPResponseV2(getirResponse.ResponseMessage.StatusCode, _logFolderName, CalcSuccessFromHTTPStatus(getirResponse.ResponseMessage.StatusCode), getirResponse?.GetContent().Meta?.returnMessage ?? "");
								}
								else
								{
									await UpdateOrderWithOrderDetails(nameof(StatusEnums.InTransit), true, null, orderDto.OrderId, string.Empty);

									return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, CalcSuccessFromHTTPStatus(HttpStatusCode.OK), "Siparişin durumu yolda olarak güncellenmiştir.");
								}
							}
							else
							{
								await UpdateOrderWithOrderDetails(nameof(StatusEnums.InTransit), true, null, orderDto.OrderId, string.Empty);
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için Getir üzerinde işlem yapılmamıştır.");
							}
						}
					case StatusEnums.Delivered:
						{
							if (enableMarketPlaceServices)
							{
								if (orderEntity.KargoSaglayiciAdi == DeliveryTypeCode.GetirGetirsin)
								{
									return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "Getir (Getir kuryesi ile teslimat) siparişlerinde bu statüye geçiş yapılamaz.");
								}
								else
								{
									Response<GetirResponse> getirResponse;
									Logger.Information("OrderUpdatePackageStatus => Delivered(Deliver) - İşletme Getirsin Request for ShopId: {shopId} and OrderId: {pyOrderID}", fileName: _logFolderName, shopId, pyOrderID);
									getirResponse = await _getirCarsiClient.Deliver(pyOrderID, shopId);
									Logger.Information("OrderUpdatePackageStatus => Delivered Response StatusCode : {statusCode}, Response : {response}", fileName: _logFolderName, getirResponse.ResponseMessage.StatusCode, getirResponse?.StringContent ?? "");

									await UpdateOrderWithOrderDetails(nameof(StatusEnums.Delivered), getirResponse.ResponseMessage.IsSuccessStatusCode, null, orderDto.OrderId, getirResponse.StringContent);

									return OrderHelper.ReturnQPResponseV2(getirResponse.ResponseMessage.StatusCode, _logFolderName, CalcSuccessFromHTTPStatus(getirResponse.ResponseMessage.StatusCode), getirResponse?.GetContent().Meta?.returnMessage ?? "");
								}
							}
							else
							{
								await UpdateOrderWithOrderDetails(nameof(StatusEnums.Delivered), true, null, orderDto.OrderId, string.Empty);
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için Getir üzerinde işlem yapılmamıştır.");
							}
						}
					case StatusEnums.Completed:
						{
							if (orderEntity.KargoSaglayiciAdi == DeliveryTypeCode.IsletmeGetirsin)
							{
								await UpdateOrderWithOrderDetails(nameof(StatusEnums.Completed), true, null, orderDto.OrderId, string.Empty);
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Completed işlemi tamamlandı.");
							}
							return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "Getir (Getir kuryesi ile teslimat) siparişlerinde bu statüye geçiş yapılamaz.");
						}

					default:
						{
							if (orderEntity.KargoSaglayiciAdi == DeliveryTypeCode.IsletmeGetirsin)
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "Getir (Mağaza kuryesi ile teslimat) siparişlerinde bu statüye geçiş yapılamaz.");
							else
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "Getir (Getir kuryesi ile teslimat) siparişlerinde bu statüye geçiş yapılamaz.");
						}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("OrderUpdatePackageStatus exception: {exception}", _logFolderName, ex);
				return OrderHelper.ReturnQPResponseV2(HttpStatusCode.InternalServerError, _logFolderName, false, ex.Message);
			}
		}
		public async Task<CommonResponseDto> ClaimStatuUpdate(PostProductReturnRequestDto claimDto)
		{
			var siparisDetay = await _pazarYeriSiparisDalService.GetOrderByIdWithDetailsAsync(claimDto.OrderId);
			if (siparisDetay == null)
				return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "Siparis bulunamadı");

			var res = await _getirCarsiClient.PatchReceiveReturn(siparisDetay.PazarYeriSiparisDetails.FirstOrDefault().PazarYeriBirimId, claimDto.ReturnId);
			return claimDto.Result switch
			{
				TyGoConstants.Accepted => await _getirReturnService.AcceptClaimAsync(claimDto),
				TyGoConstants.Rejected => await _getirReturnService.RejectClaimAsync(claimDto),
				_ => new CommonResponseDto() { Message = "ReasonCode sadece ACCEPTED veya REJECTED olabilir." },
			};
		}

        #endregion

        #region Private Methods
        public async Task<bool> SaveOrderDb(GetirOrderDto order, long qpOrderSeqId,
            IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts)
        {
            try
            {
                PazarYeriSiparis pazarYeriSiparis;
                string invoiceId = Guid.NewGuid().ToString();
                string shipmentAddressId = Guid.NewGuid().ToString();
                var merchantNo = PazarYeri.GetirCarsi;
                var shopId = order.shopId;

                pazarYeriSiparis = new PazarYeriSiparis()
                {
                    PazarYeriNo = merchantNo,
                    Id = qpOrderSeqId,
                    PaketId = order.orderId,
                    SiparisId = order.orderId,
                    SiparisNo = order.orderNumber.ToString(),
                    SiparisTarih = order.checkoutDate,
                    ToplamTutar = order.totalPrice,
                    TcKimlikNo = order.invoiceAddress.identityNumber,
                    MusteriId = order.customer.id,
                    MusteriAdi = order.customer.name,
                    MusteriSoyadi = string.Empty,
                    MusteriEmail = order.customer.email,
                    KargoTakipNo = order.confirmationId,
                    KargoGondericiNumarasi = string.Empty,
                    KargoSaglayiciAdi = (order?.deliveryType ?? 1).ToString(),
                    SevkiyatPaketDurumu = order.orderType,
                    KargoAdresId = shipmentAddressId,
                    FaturaAdresId = invoiceId,
                    KoliAdeti = 0,
                    Desi = 0,
                    MaksTutar = order.maxPossibleAmountMerchantCanCharge,
                    MinTutar = order.minPossibleAmountMerchantCanCharge

                };
                PazarYeriKargoAdres pazarYeriKargoAdres = new()
                {
                    Id = qpOrderSeqId,
                    KargoAdresId = shipmentAddressId,
                    AdSoyad = order.customer.name,
                    Ad = order.customer.name,
                    Adres1 = order.deliveryInfo.address,
                    Adres2 = order.deliveryInfo.address,
                    Sehir = order.deliveryInfo.city,
                };
                PazarYeriFaturaAdres pazarYeriFaturaAdres = new()
                {
                    Id = qpOrderSeqId,
                    FaturaAdresId = invoiceId,
                    AdSoyad = order.customer.name,
                    Adi = order.customer.name,
                    Adres1 = order.invoiceAddress.address,
                    Adres2 = order.invoiceAddress.address,
                    Sehir = order.invoiceAddress.city,
                    Semt = order.invoiceAddress.district,
                    UlkeKod = order.invoiceAddress.countryCode,
                    Firma = string.Empty,
                };
                PazarYeriSiparisEkBilgi packagingInfo = new()
                {
                    PosetSayisi = order.packagingInfo.bagCount ?? 0,
                    PosetTutari = order.packagingInfo.totalPackagingPrice ?? 0,
                    PySiparisNo = order.orderNumber.ToString(),
                    ObaseSiparisId = qpOrderSeqId
                };

                List<PazarYeriSiparisDetay> pazarYeriSiparisDetayList = new();
                List<PazarYeriSiparisUrun> pazarYeriSiparisUrunList = new();

                if (order.products.Any())
                {
                    foreach (var itemProduct in order.products)
                    {
                        var imageResult = await _getDalService.GetAsync<PazaryeriAlternatifGonderim>(x => x.PazarYeriMalNo == itemProduct.vendorId && x.PazarYeriBirimNo == shopId);
                        string imageUrl = imageResult is not null ? imageResult.ProductImage ?? "" : "";
                        var transferProduct = transferProducts.FirstOrDefault(x => itemProduct.vendorId == x.PazarYeriMalNo && x.PazarYeriBirimNo == order.shopId);
                        var product = transferProduct is not null ? products.FirstOrDefault(x => x.MalNo.Trim() == transferProduct?.MalNo?.Trim()) : null;

                        if (pazarYeriSiparisUrunList.Exists(x => x.PazarYeriMalNo == transferProduct?.PazarYeriMalNo))
                        {
                            pazarYeriSiparisUrunList.First(x => x.PazarYeriMalNo == transferProduct?.PazarYeriMalNo).Miktar += (itemProduct.type == ProductType.Gr ? (decimal)(itemProduct.totalWeight) / 1000 : itemProduct.count);
                        }
                        else
                        {
                            pazarYeriSiparisUrunList.Add(new PazarYeriSiparisUrun()
                            {
                                ObaseSiparisId = qpOrderSeqId,
                                PySiparisNo = order.orderNumber.ToString(),
                                ObaseMalNo = product?.MalNo ?? transferProduct?.MalNo,
                                PazarYeriMalNo = transferProduct?.PazarYeriMalNo,
                                AltUrunObaseMalNo = string.Empty,
                                AltUrunPazarYeriMalNo = string.Empty,
                                Miktar = itemProduct.type == YemekSepetiConstants.ProductType.KG ? (decimal)itemProduct.totalWeight / 1000 : itemProduct.count,
                                IsAlternativeEH = Character.H,
                                ImageUrl = imageUrl,
                                PazarYeriBirimId = order.shopId
                            });
                        }

                        pazarYeriSiparisDetayList.Add(new PazarYeriSiparisDetay()
                        {
                            Id = qpOrderSeqId,
                            LineItemId = itemProduct.Id,
                            PaketItemId = itemProduct.Id,
                            PazarYeriBirimId = order.shopId,
                            ObaseMalNo = product?.MalNo ?? transferProduct?.MalNo,
                            PazarYeriMalNo = transferProduct?.PazarYeriMalNo,
                            PazarYeriUrunKodu = string.Empty,
                            PazarYeriMalAdi = itemProduct.name.tr,
                            AlternatifUrunEH = Character.H,
                            Barkod = itemProduct.hasBarcode ? string.Join(',', itemProduct.barcodes) : itemProduct.barcode,
                            Miktar = itemProduct.type == ProductType.Gr ? (decimal)itemProduct.totalWeight / 1000 : (itemProduct.count ?? 1),
                            NetTutar = (itemProduct.totalPrice ?? 0),
                            BrutTutar = (itemProduct.totalPrice ?? 0),
                            KdvOran = itemProduct.vatRate,
                            SatisKampanyaId = string.Empty,
                            UrunBoyutu = string.Empty,
                            UrunRengi = string.Empty,
                            SiparisUrunDurumAdi = string.Empty,
                            IsCancelledEH = Character.H,
                            IsAlternativeEH = Character.H,
                            IsCollectedEH = Character.H
                        });
                    }
                }
                else
                {
                    return false;
                }
                #region Db Save
                await _transactionDalService.BeginTransactionAsync();
                await _pazarYeriSiparisDalService.AddOrderAsync(pazarYeriSiparis);
                await _pazarYeriSiparisDetayDalService.AddOrderDetailsAsync(pazarYeriSiparisDetayList);
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
				await SendFailedOrderMail("GetirÇarşı Siparişi Kaydedilirken Hata", $"Hata! Sipariş Db'ye kaydedilemedi. Sipariş No: {order?.orderNumber ?? 0} \nException: {ex.Message} - {ex.InnerException?.Message ?? ""} ");
                Logger.Error("SaveOrderDb Exception {exception} for Order: {order}", fileName: _logFolderName, ex, order.orderNumber);
                await _transactionDalService.RollbackTransactionAsync();
                return false;
            }
        }
        private async Task<bool> SendOrderToQp(PazarYeriBirimTanim pyBirimTanim, IEnumerable<PazarYeriMalTanim> pyProductList, IEnumerable<PazarYeriAktarim> pyTransferredProductList, long qpOrderSeqId, GetirOrderDto orderDto, SachetProduct[] sachetProducts=null)
        {
            bool isSendToQp = false;     
			try
			{

                if (sachetProducts?.Any() ?? false)
                {
                    orderDto.packagingInfo.bagNumber = sachetProducts.FirstOrDefault().ProductCode;
                    orderDto.packagingInfo.BagUnitPrice = Convert.ToDecimal(sachetProducts.FirstOrDefault().Price);
                }

                var qpModel = _orderConvertService.ToQpOrder(orderDto, pyBirimTanim, pyProductList, pyTransferredProductList, qpOrderSeqId, merchantNo: PazarYeri.GetirCarsi, sachetProduct: _apiDefinition.SachetProducts);
                // devam eden işlemler

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
                          Logger.Warning("GetirCarsi - SendOrderToQp - Retry attempt {Attempt} will be made after {Delay}s. Reason: {Message}",
							  fileName: _logFolderName,
							  args.AttemptNumber.ToString(),
                              args.RetryDelay.TotalSeconds.ToString("F1"),
                              args.Outcome.Exception?.Message ?? "Unknown error");
                          return ValueTask.CompletedTask;
                      }
                  })
                  .Build();

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

                        string errorMessage = $"Sipariş Quickpick'e aktarılırken bir hata oluştu.Sipariş No: {orderDto.orderNumber}. Hata:{consumeDeliveryResult}";
                        Logger.Error("QP Error With Id: {qpOrderSeqId}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, errorMessage);
                        throw new InvalidOperationException($"GetirCarsi - SendOrderToQp - Sipariş Quickpick'e aktarılırken bir hata oluştu ve tekrar gönderim sağlanacak. QP Response :{errorMessage}");
                    }
                    catch (Exception ex)
                    {
                        if ((_appSetting.Value.MailSettings?.MailEnabled ?? false))
                        {
                            await _mailService.SendMailAsync(_logFolderName + $" Hata! Sipariş Quickpick'e iletilemedi. Sipariş No: {orderDto?.orderNumber ?? 0}", ex.Message + " " + ex.InnerException?.Message ?? "");
                        }
                        Logger.Error("QP Error With Id: {qpOrderSeqId} For Order Number: {orderNumber}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, orderDto.orderNumber, ex);
                        throw new InvalidOperationException($"GetirCarsi - SendOrderToQp - Sipariş Quickpick'e aktarılırken bir hata oluştu ve tekrar gönderim sağlanacak. Error :{ex.Message + " " + ex.InnerException?.Message ?? ""} ");

                    }
                });
            }
			catch (Exception ex)
			{

                if ((_appSetting.Value.MailSettings?.MailEnabled ?? false))
                {
                    await _mailService.SendMailAsync(_logFolderName + $" Hata! Sipariş Kaydedilemedi. Sipariş No: {orderDto?.orderNumber ?? 0}", ex.Message + " " + ex.InnerException?.Message ?? "");
                }
                Logger.Error("GetirApiController > CreateOrderFromWebHook Hata {exception} ", fileName: _logFolderName, ex);
            }
            return isSendToQp;
        }
        #endregion



    }
}