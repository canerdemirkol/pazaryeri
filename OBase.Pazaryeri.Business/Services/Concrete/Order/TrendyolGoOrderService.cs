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
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using Polly;
using Polly.Retry;
using RestEase;
using System;
using System.Net;
using System.ServiceModel;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Constants.Constants.GetirConstants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;
using static OBase.Pazaryeri.Domain.Helper.CommonHelper;

namespace OBase.Pazaryeri.Business.Services.Concrete.Order
{
	public class TrendyolGoOrderService : BaseService, ITrendyolGoOrderService
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
		private readonly ITrendyolGoClient _trendyolGoClient;
		private readonly ITrendyolGoReturnService _tyGoReturnService;
		private readonly ITransactionDalService _transactionDalService;
		private readonly ApiDefinitions _apiDefinition;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.TrendyolGo);

		private readonly QPService.OrderDeliveryServiceSoapClient _qpClient;

        private readonly IOrderConvertService _orderConvertService;

        #endregion

        #region Ctor

        public TrendyolGoOrderService(IPazarYeriBirimTanimDalService pazarYeriBirimTanimDalService,
			IPazarYeriMalTanimDalService pazarYeriMalTanimDalService, IPazarYeriSiparisDalService pazarYeriSiparisDalService,
			IPazarYeriAktarimDalService pazarYeriAktarimDalService, ITrendyolGoClient trendyolGoClient, IPazarYeriSiparisDetayDalService pazarYeriSiparisDetayDalService,
			IPazarYeriSiparisUrunDalService pazarYeriSiparisUrunDalService, IPazarYeriSiparisEkBilgiDalService pazarYeriSiparisEkBilgiDalService, IOptions<AppSettings> appSetting,
			ITransactionDalService transactionDalService, IPazarYeriKargoAdresDalService pazarYeriKargoAdresDalService, IPazarYeriFaturaAdresDalService pazarYeriFaturaAdresDalService, ITrendyolGoReturnService tyGoReturnService, IMailService mailService, IOrderConvertService orderConvertService) : base(pazarYeriSiparisDalService, pazarYeriSiparisDetayDalService, appSetting, mailService)
		{
			_pazarYeriBirimTanimDalService = pazarYeriBirimTanimDalService;
			_pazarYeriMalTanimDalService = pazarYeriMalTanimDalService;
			_pazarYeriAktarimDalService = pazarYeriAktarimDalService;
			_trendyolGoClient = trendyolGoClient;
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
			_apiDefinition = _appSetting.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.TrendyolGo);
			_tyGoReturnService = tyGoReturnService;
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
				bool enableTGGOServices = _appSetting.Value.EnableMarketPlaceServices;
				var status = orderDto.Status;
				Response<CommonResponseDto> response;

				if (!orderDto.ProductQuantities.Any())
				{
					return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "En Az Bir Ürün Göndermelisiniz");
				}

				IEnumerable<PazarYeriSiparisDetay> marketPlaceOrderLineItems = new List<PazarYeriSiparisDetay>();

				var packageId = orderDto.PackageId;

				switch (status)
				{
					case StatusEnums.Picking:
						{
							if (enableTGGOServices)
							{
								Logger.Information("OrderUpdatePackageStatus => Picking Request SupplierId :{supplierId} Picking Request PackageId : {packageId}", fileName: _logFolderName, supplierId, packageId);
								response = await _trendyolGoClient.Picked(supplierId, packageId);
								Logger.Information("OrderUpdatePackageStatus => Picking Response Statu Message : {statusCode} Picking Response : {@response}", fileName: _logFolderName, response.ResponseMessage.StatusCode, response?.StringContent ?? "");
								await UpdateOrderWithOrderDetails(CommonEnums.StatusEnums.Picking.ToString(), response.ResponseMessage.IsSuccessStatusCode, null, orderDto.OrderId, response.StringContent);
								return new CommonResponseDto() { StatusCode = response.ResponseMessage.StatusCode, Success = CalcSuccessFromHTTPStatus(response.ResponseMessage.StatusCode), Message = response.StringContent };
							}
							else
							{
								await UpdateOrderWithOrderDetails(CommonEnums.StatusEnums.Picking.ToString(), true, null, orderDto.OrderId, string.Empty);
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için TrendyolGo üzerinde işlem yapılmamıştır.");
							}
						}
					case StatusEnums.Collected:
						{
							SachetProduct[] sachetProducts = _apiDefinition.SachetProducts;

							Dictionary<string, string> productNoMap = new();
							int bagCount = 0;
							decimal bagTotalPrice = 0;
							foreach (ProductQuantity productQuantity in orderDto.ProductQuantities)
							{
								if (sachetProducts?.Any(sp => sp.ProductCode == productQuantity.ProductId) ?? false)
								{
									SachetProduct sachetProduct = Array.Find(sachetProducts, w => w.ProductCode == productQuantity.ProductId);
									decimal sachetPrice = 0;
									if (sachetProduct != null) { sachetPrice = sachetProduct.Price; }
									int bagQuantity = (int)Math.Round(productQuantity.Quantity);
									bagCount += bagQuantity;
									bagTotalPrice += bagQuantity * sachetPrice;
									continue;
								}
								else if (productQuantity.ProductId == _appSetting.Value.CargoProductCode)
								{
									continue;
								}
								string strProductNo = productQuantity.ProductId.Trim();
								string productMarketPlaceId = await _pazarYeriMalTanimDalService.GetProductMarketPlaceIdByObaseProductIdAsync(strProductNo, _apiDefinition.Merchantno);
								productNoMap.Add(strProductNo, productMarketPlaceId);
							}
							IEnumerable<PazarYeriMalTanim> productsWithKGUnit = await _pazarYeriMalTanimDalService.GetProductSalesValueByUnitAsync(CommonConstants.KG, _apiDefinition.Merchantno);
							marketPlaceOrderLineItems = orderEntity.PazarYeriSiparisDetails;
							var mpOrderProducts = await _pazarYeriSiparisUrunDalService.GetOrderProductsByIdAsync(orderDto.OrderId);
							List<string> suppliedLineItemPackageItemIds = GetSuppliedLineItemPackageItemIds(mpOrderProducts, marketPlaceOrderLineItems, orderDto.ProductQuantities, productsWithKGUnit);
							List<string> unsuppliedLineItemPackageItemIds = GetUnsuppliedLineItemPackageItemIds(mpOrderProducts, marketPlaceOrderLineItems, orderDto.ProductQuantities, productsWithKGUnit);
							List<string> altSentPackageItemIds = new();
							Dictionary<string, TGProductAdditionalDataDto> mpOrderProductAdditionalDataDict = new();

							bool areUnSuppliedProductsPresent = false;
							bool areAlternativeProductsPresent = false;
							bool areFullyUnSuppliedProductsPresent = false;
							List<string> fullyUnsuppliedLineItemPackageIds = new();

							decimal totalAmount = 0;
							foreach (var mpOrderProduct in mpOrderProducts)
							{
								if (mpOrderProduct.ObaseMalNo == sachetProducts[0].ProductCode)
								{
									continue;
								}
								ProductQuantity qpProduct = orderDto.ProductQuantities.Find(qpX => qpX.ProductId == mpOrderProduct.ObaseMalNo);

								if (qpProduct is null)
								{
									return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, false, "Lütfen, siparişte bulunan tüm ürünleri gönderiniz.");
								}

								ProductQuantity qpAltProduct = orderDto.ProductQuantities.Find(qpX => qpX.AltProductId == mpOrderProduct.ObaseMalNo) ?? null;
								TGProductAdditionalDataDto mpOrderProductAdditionalData = new();
								bool hasAlternativeGivenForThisProduct = false;
								if (qpAltProduct is not null && qpAltProduct.Quantity > 0)
								{
									hasAlternativeGivenForThisProduct = true;
									mpOrderProductAdditionalData.AltProductId = qpAltProduct.ProductId;
									mpOrderProductAdditionalData.altSuppliedQuantity = qpAltProduct.Quantity;
								}
								mpOrderProductAdditionalData.orderedQuantity = mpOrderProduct.Miktar ?? 0;
								mpOrderProductAdditionalData.isWeightedItem = productsWithKGUnit.Any(x => x.MalNo == mpOrderProduct.ObaseMalNo);
								var pyProductUnitValue = productsWithKGUnit?.FirstOrDefault(x => x.MalNo == mpOrderProduct.ObaseMalNo)?.PyUrunSatisDeger ?? 1;

								mpOrderProductAdditionalData.tyQuantityCoefficient = mpOrderProductAdditionalData.isWeightedItem ? pyProductUnitValue : 1;
								decimal qpQuantity = 0;
								if (hasAlternativeGivenForThisProduct)
								{
									var packageItemIdsForGivenProduct = marketPlaceOrderLineItems.Where(x => x.ObaseMalNo == qpProduct.ProductId).Select(x => x.PaketItemId).ToList();
									var unsuppliedPackageItemIdsForGivenProduct = unsuppliedLineItemPackageItemIds.Intersect(packageItemIdsForGivenProduct).ToList();

									qpQuantity = mpOrderProductAdditionalData.altSuppliedQuantity + qpProduct.Quantity;
									mpOrderProductAdditionalData.suppliedQuantity = qpQuantity;
									mpOrderProductAdditionalData.isAlternateProductGiven = true;
									List<string> altSentPackageItemIdsForCurrProduct = new();
									for (int i = 0; i < mpOrderProductAdditionalData.altSuppliedQuantity; i++)
									{
										string altProPackageID = unsuppliedPackageItemIdsForGivenProduct.FirstOrDefault();
										packageItemIdsForGivenProduct.Remove(altProPackageID);
										unsuppliedPackageItemIdsForGivenProduct.Remove(altProPackageID);
										unsuppliedLineItemPackageItemIds.Remove(altProPackageID);
										altSentPackageItemIdsForCurrProduct.Add(altProPackageID);
									}
									altSentPackageItemIds.AddRange(altSentPackageItemIdsForCurrProduct);
								}
								else
								{
									qpQuantity = qpProduct.Quantity;
									mpOrderProductAdditionalData.suppliedQuantity = qpQuantity;
								}

								if (qpQuantity >= mpOrderProductAdditionalData.tyQuantityCoefficient * mpOrderProductAdditionalData.orderedQuantity)
								{
									mpOrderProductAdditionalData.tySuppliedProductCount = (int)Math.Floor(qpQuantity / mpOrderProductAdditionalData.tyQuantityCoefficient);
								}
								else
								{
									mpOrderProductAdditionalData.tySuppliedProductCount = (int)Math.Ceiling(qpQuantity / mpOrderProductAdditionalData.tyQuantityCoefficient);
								}

								decimal unSuppliedQuantityTemp = (mpOrderProduct.Miktar ?? 0) - qpQuantity;
								mpOrderProductAdditionalData.unSuppliedQuantity = (unSuppliedQuantityTemp < 0) ? 0 : unSuppliedQuantityTemp;
								mpOrderProductAdditionalData.isUnSupplied = mpOrderProductAdditionalData.unSuppliedQuantity > 0;
								mpOrderProductAdditionalData.isFullyUnSupplied = qpQuantity <= 0;

								if (mpOrderProductAdditionalData.isUnSupplied && !hasAlternativeGivenForThisProduct)
								{
									if (mpOrderProductAdditionalData.isWeightedItem)
									{
										areUnSuppliedProductsPresent = areUnSuppliedProductsPresent || (mpOrderProductAdditionalData.tySuppliedProductCount != mpOrderProduct.Miktar);
									}
									else
									{
										areUnSuppliedProductsPresent = true;
									}
								}
								if (mpOrderProductAdditionalData.isFullyUnSupplied && !hasAlternativeGivenForThisProduct)
								{
									areFullyUnSuppliedProductsPresent = true;
									IEnumerable<string> mpOrderProductLineItemPackageIds = marketPlaceOrderLineItems.Where(x => x.ObaseMalNo == mpOrderProduct.ObaseMalNo).Select(x => x.PaketItemId);
									fullyUnsuppliedLineItemPackageIds.AddRange(mpOrderProductLineItemPackageIds);
								}
								mpOrderProductAdditionalDataDict.Add(mpOrderProduct.ObaseMalNo, mpOrderProductAdditionalData);
								IEnumerable<PazarYeriSiparisDetay> mpOrderProductPriceLineItems = marketPlaceOrderLineItems.Where(x => x.ObaseMalNo == qpProduct.ProductId);
								decimal productMPUnitPrice = mpOrderProductPriceLineItems.Select(li => li.NetTutar).Max();
								decimal productUnitPrice = productMPUnitPrice / mpOrderProductAdditionalData.tyQuantityCoefficient;
								if (hasAlternativeGivenForThisProduct)
								{
									try
									{
										areAlternativeProductsPresent = true;
										decimal altProductUnitPrice = await _pazarYeriSiparisUrunDalService.GetSubProductPriceInfoAsync(qpAltProduct.ProductId, PazarYeri.TrendyolGo);

										totalAmount = totalAmount + (productUnitPrice * (mpOrderProductAdditionalData.suppliedQuantity - mpOrderProductAdditionalData.altSuppliedQuantity)) +
											(altProductUnitPrice * mpOrderProductAdditionalData.altSuppliedQuantity);
									}
									catch (Exception ex)
									{
										Logger.Error("OrderUpdatePackageStatus> hasAlternativeGivenForThisProduct Hata:{exception}", fileName: _logFolderName, ex);
									}
								}
								else
								{
									totalAmount = totalAmount + productUnitPrice * mpOrderProductAdditionalData.suppliedQuantity;
								}
							}
							totalAmount = totalAmount + bagTotalPrice;

							try
							{
								PazarYeriSiparisEkBilgi pazarYeriSiparisEkBilgi = await _pazarYeriSiparisEkBilgiDalService.GetAdditionalDataAsync(orderDto.OrderId);
								totalAmount = totalAmount + (pazarYeriSiparisEkBilgi?.GonderimUcreti ?? 0);
								if (pazarYeriSiparisEkBilgi != null)
								{
									pazarYeriSiparisEkBilgi.PosetSayisi = bagCount;
									pazarYeriSiparisEkBilgi.PosetTutari = bagTotalPrice;
									pazarYeriSiparisEkBilgi.GuncelFaturaTutar = totalAmount;
									await _pazarYeriSiparisEkBilgiDalService.UpdateOrderAdditionalDataAsync(pazarYeriSiparisEkBilgi);
								}

								foreach (var mpOrderProductObaseMalNo in mpOrderProducts.Select(s => s.ObaseMalNo))
								{
									if (mpOrderProductObaseMalNo == sachetProducts[0].ProductCode)
									{
										var product = await _pazarYeriSiparisUrunDalService.GetOrderProductByIdAsync(orderDto.OrderId, mpOrderProductObaseMalNo);
										if (product != null)
										{
											product.GuncelMiktar = bagCount;
											await _pazarYeriSiparisUrunDalService.UpdateProductAsync(product);
										}
										continue;
									}
									if (mpOrderProductAdditionalDataDict.ContainsKey(mpOrderProductObaseMalNo))
									{
										TGProductAdditionalDataDto productAdditionalData = mpOrderProductAdditionalDataDict[mpOrderProductObaseMalNo];
										var product = await _pazarYeriSiparisUrunDalService.GetOrderProductByIdAsync(orderDto.OrderId, mpOrderProductObaseMalNo);
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
							}
							catch (Exception ex)
							{
								Logger.Error("OrderUpdatePackageStatus > PazarYeriSiparisEkBilgi Hata:{exception}", _logFolderName, ex);
							}

							if (areUnSuppliedProductsPresent)
							{
								var updatePackageAsUnSuppliedDto = new TGUpdatePackageUnSuppliedRequestDto()
								{
									ItemIdList = unsuppliedLineItemPackageItemIds.ToArray(),
									ReasonId = TranslateQPReasonIdToTrendyolReasonId(orderDto.ReasonId)
								};

								if (areFullyUnSuppliedProductsPresent)
								{
									updatePackageAsUnSuppliedDto.CausedCancelPackageItemIds = fullyUnsuppliedLineItemPackageIds.ToArray<string>();
								}

								if (enableTGGOServices)
								{
									TGMarkAlternativeRequestDto markAltDto = new();
									if (areAlternativeProductsPresent)
									{
										markAltDto.CollectedItemIdList = suppliedLineItemPackageItemIds.Except(altSentPackageItemIds).ToArray<string>();
										markAltDto.AlternativeItemIdList = altSentPackageItemIds.ToArray<string>();
									}
									else
									{
										markAltDto.CollectedItemIdList = suppliedLineItemPackageItemIds.ToArray<string>();
									}
									if (enableTGGOServices)
									{
										Logger.Information("OrderUpdatePackageStatus > Mark-Alternative Request: {@request}", fileName: _logFolderName, markAltDto);
										var responseAlt = _trendyolGoClient.MarkAlternative(supplierId, packageId, markAltDto).Result;
										Thread.Sleep(200);
										Logger.Information("OrderUpdatePackageStatus > Mark-Alternative Status Message: {statusCode} Mark-Alternative Response: {@response}", fileName: _logFolderName, responseAlt.ResponseMessage.StatusCode, responseAlt?.GetContent());
									}

									Logger.Information("OrderUpdatePackageStatus > UnSupplied Request: {@request}", fileName: _logFolderName, updatePackageAsUnSuppliedDto);
									response = await _trendyolGoClient.UpdatePackageAsUnSupplied(supplierId, packageId, updatePackageAsUnSuppliedDto);
									Logger.Information("OrderUpdatePackageStatus > UnSupplied Status Message: {statusCode} UnSupplied Response: {@response}", fileName: _logFolderName, response.ResponseMessage.StatusCode, response?.GetContent());
									await UpdateOrderWithOrderDetails(StatusEnums.UnSupplied.ToString(), response.ResponseMessage.IsSuccessStatusCode, UnSuppliedStatuEnums.TedarikProblemi.ToString(), orderDto.OrderId, response.StringContent, unsuppliedLineItemPackageItemIds);

									return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, response.ResponseMessage.IsSuccessStatusCode, $"{response.ResponseMessage.StatusCode}-{response?.StringContent ?? ""}");
								}
								else
								{
									await UpdateOrderWithOrderDetails(StatusEnums.UnSupplied.ToString(), true, UnSuppliedStatuEnums.TedarikProblemi.ToString(), orderDto.OrderId, string.Empty, unsuppliedLineItemPackageItemIds);
									return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için TrendyolGo üzerinde işlem yapılmamıştır.");
								}
							}
							else
							{
								if (enableTGGOServices)
								{
									TGMarkAlternativeRequestDto markAltDto = new();
									if (areAlternativeProductsPresent)
									{
										markAltDto.CollectedItemIdList = suppliedLineItemPackageItemIds.Except(altSentPackageItemIds).ToArray<string>();
										markAltDto.AlternativeItemIdList = altSentPackageItemIds.ToArray<string>();
									}
									else
									{
										markAltDto.CollectedItemIdList = suppliedLineItemPackageItemIds.ToArray<string>();
									}
									if (enableTGGOServices)
									{
										Logger.Information("OrderUpdatePackageStatus > Mark-Alternative Request: {@request}", fileName: _logFolderName, markAltDto);
										var responseAlt = _trendyolGoClient.MarkAlternative(supplierId, packageId, markAltDto).Result;
										Thread.Sleep(200);
										Logger.Information("OrderUpdatePackageStatus > Mark-Alternative Status Message: {statusCode} Mark-Alternative Response: {@response}", fileName: _logFolderName, responseAlt.ResponseMessage.StatusCode, responseAlt?.GetContent());
									}

									var updatePackageInvoicedDto = new TGUpdatePackageInvoicedRequestDto()
									{
										InvoiceAmount = totalAmount,
										ReceiptLink = string.Empty
									};
									if (bagCount > 0)
									{
										updatePackageInvoicedDto.BagCount = bagCount;
									}
									Logger.Information("OrderUpdatePackageStatus => Invoiced Request: {@request}", fileName: _logFolderName, updatePackageInvoicedDto);
									response = await _trendyolGoClient.UpdatePackageAsInvoiced(supplierId, packageId, updatePackageInvoicedDto);
									Logger.Information("OrderUpdatePackageStatus => Invoiced Statu Message => {statusCode} Invoiced Response => {@response}", fileName: _logFolderName, response.ResponseMessage.StatusCode, response?.GetContent());
									await UpdateOrderWithOrderDetails(nameof(StatusEnums.Invoiced), response.ResponseMessage.IsSuccessStatusCode, null, orderDto.OrderId, response.StringContent);
									CommonResponseDto commonRespDto = new() { StatusCode = response.ResponseMessage.StatusCode, Success = CalcSuccessFromHTTPStatus(response.ResponseMessage.StatusCode), Message = response.StringContent };
									return commonRespDto;
								}
								else
								{
									await UpdateOrderWithOrderDetails(StatusEnums.Invoiced.ToString(), true, null, orderDto.OrderId, string.Empty);
									return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için TrendyolGo üzerinde işlem yapılmamıştır.");
								}
							}
						}
					case StatusEnums.Cancelled:
						{
							if (enableTGGOServices)
							{
								marketPlaceOrderLineItems = orderEntity.PazarYeriSiparisDetails;
								var updatePackageAsUnSuppliedDto = new TGUpdatePackageUnSuppliedRequestDto()
								{
									ItemIdList = marketPlaceOrderLineItems.Select(x => x.PaketItemId).ToArray(),
									ReasonId = TranslateQPReasonIdToTrendyolReasonId(orderDto.ReasonId)
								};
								Logger.Information("OrderUpdatePackageStatus => UnSupplied Request: {@request}", fileName: _logFolderName, updatePackageAsUnSuppliedDto);
								response = await this._trendyolGoClient.UpdatePackageAsUnSupplied(supplierId, packageId, updatePackageAsUnSuppliedDto);
								Logger.Information("OrderUpdatePackageStatus => UnSupplied Statu Message: {statusCode} UnSupplied Response: {@response}", fileName: _logFolderName, response.ResponseMessage.StatusCode, response?.GetContent());
								await UpdateOrderWithOrderDetails(StatusEnums.Cancelled.ToString(), response.ResponseMessage.IsSuccessStatusCode, TranslateQPReasonIdToTrendyolReasonId(orderDto.ReasonId).ToString(), orderDto.OrderId, response.StringContent, marketPlaceOrderLineItems.Select(x => x.PaketItemId).ToList());
								return new CommonResponseDto() { StatusCode = response.ResponseMessage.StatusCode, Success = CalcSuccessFromHTTPStatus(response.ResponseMessage.StatusCode), Message = response.StringContent };
							}
							else
							{
								await UpdateOrderWithOrderDetails(StatusEnums.Cancelled.ToString(), true, TranslateQPReasonIdToTrendyolReasonId(orderDto.ReasonId).ToString(), orderDto.OrderId, string.Empty, marketPlaceOrderLineItems.Select(x => x.PaketItemId).ToList());
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, "Pazaryeri servisleri pasif durumda olduğu için TrendyolGo üzerinde işlem yapılmamıştır.");
							}
						}
					case StatusEnums.Completed or StatusEnums.InTransit or StatusEnums.Delivered or StatusEnums.UnDelivered:
						{
							await UpdateOrderWithOrderDetails(nameof(StatusEnums.Completed), true, null, orderDto.OrderId, string.Empty);
							return OrderHelper.ReturnQPResponseV2(HttpStatusCode.OK, _logFolderName, true, $"Sipariş {StatusEnums.Completed} statüsüne alındı.");
						}
					default:
						{
							if (orderEntity.KargoSaglayiciAdi == TyGoConstants.StoreDelivery)
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "TrendyolGO (Mağaza kuryesi ile teslimat) siparişlerinde bu statüye geçiş yapılamaz.");
							else
								return OrderHelper.ReturnQPResponseV2(HttpStatusCode.BadRequest, _logFolderName, false, "TrendyolGO (Trendyol kuryesi ile teslimat) siparişlerinde bu statüye geçiş yapılamaz.");
						}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("OrderUpdatePackageStatus exception: {exception}", _logFolderName, ex);
				return OrderHelper.ReturnQPResponseV2(HttpStatusCode.InternalServerError, _logFolderName, false, ex.Message);
			}
		}

		public async Task<List<TrendyolGoOrderDto>> GetTrendyolGoOrdersByStoreIdAsync(string orderStatus, string storeId, string merchantNo, string startDate, string endDate)
		{
			List<TrendyolGoOrderDto> orders = new List<TrendyolGoOrderDto>();
			bool loopCondition = true;
			int page = 0;
			try
			{
				while (loopCondition)
				{
					var result = await _trendyolGoClient.GetShipmentPackages(_apiDefinition.SupplierId, orderStatus, storeId, startDate, endDate, page.ToString());
					if (result.ResponseMessage.StatusCode == HttpStatusCode.OK)
					{
						TGShipmentPackageResponseDto root = JsonConvert.DeserializeObject<TGShipmentPackageResponseDto>(result.StringContent);

						orders.AddRange(root.Content);
						page++;
						loopCondition = page < root.TotalPages;
					}
					else
					{
                        await SendFailedOrderMail("TrendyolGO Servisinde Hata", $"Hata! Siparişler çekilemedi. Pazar Yeri Birim No: {storeId ?? ""} \nResult: {result?.StringContent ?? ""}");
                        Logger.Warning("Execution Type: {orderStatus} orders, API Request Error with Http Status Code: {statusCode}, API Response: {response}", fileName: _logFolderName, orderStatus, result.ResponseMessage.StatusCode, result?.StringContent ?? "");
						break;
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Get {orderStatus} orders request failed for {store}. Error: {e}", fileName: _logFolderName, orderStatus, storeId, e);
			}

			return orders;
		}

		public async Task ProcessTyGoCreatedOrdersAsync(Dictionary<string, string> properties)
		{
			string merchantNo = PazarYeri.TrendyolGo;
			bool sendToQp = bool.Parse(properties[TyGoConstants.Parameters.SendToQp]);
			int dayCount = int.Parse(properties[TyGoConstants.Parameters.GetOrdersDayCount]);
			var storeList = await _pazarYeriBirimTanimDalService.GetStoreDetailsListAsync(merchantNo,onlyActive:true);
			var pyProductList = await _pazarYeriMalTanimDalService.GetPyProductsAsync(merchantNo);

			DateTime dateTimeNow = DateTime.Now.ToLocalTime();
			long endDate = new DateTimeOffset(dateTimeNow).ToUnixTimeMilliseconds();
			long startDate = new DateTimeOffset(dateTimeNow.AddDays(0 - dayCount)).ToUnixTimeMilliseconds();
			Logger.Information("Get Orders from: Start Date {startDate} to End Date {endDate}", fileName: _logFolderName, startDate.ToString(), endDate.ToString());
			foreach (var store in storeList)
			{
				var pyTransferredProductList = await _pazarYeriAktarimDalService.GetPyTransferredProductsAsync(merchantNo, store.PazarYeriBirimNo);
				var storeCreatedOrderList = await GetTrendyolGoOrdersByStoreIdAsync(TyGoConstants.Created, store.PazarYeriBirimNo, merchantNo, startDate.ToString(), endDate.ToString());
				if (storeCreatedOrderList is not null && storeCreatedOrderList.Any())
				{
					await SaveTyGoCreatedOrdersAsync(storeCreatedOrderList, sendToQp, store, pyProductList, pyTransferredProductList, merchantNo);
				}
			}
		}

		public async Task ProcessTyGoCancelledOrdersAsync(Dictionary<string, string> properties)
		{
			string merchantNo = PazarYeri.TrendyolGo;
			int dayCount = int.Parse(properties[TyGoConstants.Parameters.GetOrdersDayCount]);
			var storeIdList = await _pazarYeriBirimTanimDalService.GetPyStoreNoListAsync(merchantNo);

			DateTime dateTimeNow = DateTime.Now.ToLocalTime();
			long endDate = new DateTimeOffset(dateTimeNow).ToUnixTimeMilliseconds();
			long startDate = new DateTimeOffset(dateTimeNow.AddDays(0 - dayCount)).ToUnixTimeMilliseconds();
			Logger.Information("Get Orders Request: Start Date {startDate}", fileName: _logFolderName, startDate.ToString());
			Logger.Information("End Date {endDate}", fileName: _logFolderName, endDate.ToString());
			foreach (var storeId in storeIdList)
			{
				string cancelStatus = $"{TyGoConstants.Cancelled},{TyGoConstants.UnSupplied}";
				var storeCancelledOrderList = await GetTrendyolGoOrdersByStoreIdAsync(cancelStatus, storeId, merchantNo, startDate.ToString(), endDate.ToString());
				if (storeCancelledOrderList is not null && storeCancelledOrderList.Any())
				{
					await SaveTyGoCancelledOrdersAsync(storeCancelledOrderList);
				}
			}
		}

		public async Task SaveTyGoCreatedOrdersAsync(List<TrendyolGoOrderDto> storeCreatedOrderList, bool sendToQp, PazarYeriBirimTanim pyBirimTanimDto, IEnumerable<PazarYeriMalTanim> pyProductList, IEnumerable<PazarYeriAktarim> pyTransferredProductList, string merchantNo)
		{
			Logger.Information("SaveTyGoCreatedOrdersAsync  is running.", fileName: _logFolderName);
			using OperationContextScope operationContextScope = new(_qpClient.InnerChannel);
			string supplierId = _apiDefinition.SupplierId;
			StringBuilder errorMessages = new("");
			try
			{
				foreach (var order in storeCreatedOrderList)
				{
					Logger.Information("SaveTyGoCreatedOrdersAsync Store Id:{store} Created Order Id:{orderId} Order: {@response}", fileName: _logFolderName, order.StoreId, order.OrderId, order);
					long qpOrderSeqId;
					bool dbResultVal;
					try
					{
						if (!await _pazarYeriSiparisDalService.OrderExistAsync(order.Id, merchantNo))
						{
							qpOrderSeqId = await _pazarYeriSiparisDalService.GetSeqId();
							dbResultVal = await SaveOrderDb(order, order.StoreId, qpOrderSeqId, pyProductList, pyTransferredProductList);
						}
						else
						{
							qpOrderSeqId = await _pazarYeriSiparisDalService.GetOrderIdByIdAsync(order.Id, merchantNo);
							dbResultVal = true;
						}
					}
					catch (Exception ex)
					{
						errorMessages.Append($"Sipariş No: {order.OrderNumber} için hata alındı. Hata: {ex.Message} Detay: {ex.InnerException?.Message ?? ""}");
						qpOrderSeqId = 0;
						dbResultVal = false;
						Logger.Error("SaveTyGoCreatedOrdersAsync An error occurred while querying or saving the order id{orderId} from the db.Hata: {exception}", fileName: _logFolderName, order.OrderId, ex);
					}

					long orderCountTransferredToQP = await _pazarYeriSiparisDalService.GetOrderWareHouseTransferredCountAsync(order.OrderNumber, merchantNo);
					long ordersCountWithSameOrderID = await _pazarYeriSiparisDalService.GetOrdersCountWithSameOrderIdAsync(order.OrderNumber, merchantNo);
					bool _sendQp = sendToQp && orderCountTransferredToQP < 1;

					if (ordersCountWithSameOrderID > 1)
					{
						var createdOrder = await _pazarYeriSiparisDalService.GetCreatedOrderByOrderNumberAsync(order.OrderNumber, merchantNo);
						var ordersByOrderNumber = await _pazarYeriSiparisUrunDalService.GetListAsync<PazarYeriSiparisUrun>(x => x.PySiparisNo == order.OrderNumber);
						var unSuppliedOrderId = ordersByOrderNumber.OrderBy(x => x.ObaseSiparisId).Select(x => x.ObaseSiparisId).FirstOrDefault();
						var unSuppliedOrder = ordersByOrderNumber.Where(x => x.ObaseSiparisId == unSuppliedOrderId);
						Logger.Information("Existing order! ID: {orderId} Order Number: {orderNumber}", fileName: _logFolderName, createdOrder.Id, createdOrder.SiparisNo);
						if (createdOrder != null && order.Id == createdOrder.PaketId)
						{
							IEnumerable<PazarYeriSiparisEkBilgi> mpOrderAdditionalDatas = await _pazarYeriSiparisEkBilgiDalService.GetAdditionalDatasByOrderNumberAsync(order.OrderNumber);
							List<PazarYeriSiparisDetay> pazarYeriSiparisDetays = new();
							IEnumerable<PazarYeriSiparisDetay> marketPlaceOrderLineItems = pazarYeriSiparisDetays;
							List<string> altSentPackageItemIds = new List<string>();
							bool areAlternativeProductsPresent = false;
							marketPlaceOrderLineItems = await _pazarYeriSiparisDetayDalService.GetOrderDetailsdByIdAsync(createdOrder.Id);
							foreach (PazarYeriSiparisUrun mpOrderProduct in unSuppliedOrder)
							{
								var packageItemIdsForGivenProduct = marketPlaceOrderLineItems.Where(x => x.ObaseMalNo == mpOrderProduct.ObaseMalNo).Select(x => x.PaketItemId).ToList();
								if (mpOrderProduct.IsAlternativeEH == Character.E)
								{
									areAlternativeProductsPresent = true;
									List<string> altSentPackageItemIdsForCurrProduct = new();
									for (int i = 0; i < mpOrderProduct.AltUrunMiktar; i++)
									{
										string altProPackageID = packageItemIdsForGivenProduct.FirstOrDefault();
										packageItemIdsForGivenProduct.Remove(altProPackageID);
										altSentPackageItemIdsForCurrProduct.Add(altProPackageID);
									}
									altSentPackageItemIds.AddRange(altSentPackageItemIdsForCurrProduct);
								}
							}
							Thread.Sleep(500);

							TGMarkAlternativeRequestDto markAltDto = new();
							if (areAlternativeProductsPresent)
							{
								markAltDto.CollectedItemIdList = marketPlaceOrderLineItems.Select(x => x.PaketItemId).Except(altSentPackageItemIds).ToArray<string>();
								markAltDto.AlternativeItemIdList = altSentPackageItemIds.ToArray<string>();
							}
							else
							{
								markAltDto.CollectedItemIdList = marketPlaceOrderLineItems.Select(x => x.PaketItemId).ToArray<string>();
							}
							Logger.Information("SaveTyGoCreatedOrdersAsync Mark-Alternative Request: {@request}", fileName: _logFolderName, markAltDto);
							var responseAlt = await _trendyolGoClient.MarkAlternative(supplierId, createdOrder.PaketId, markAltDto);
							Logger.Information("SaveTyGoCreatedOrdersAsync Mark-Alternative Mark-Alternative Status Message: {statusCode} Response: {@response}", fileName: _logFolderName, responseAlt.ResponseMessage.StatusCode, responseAlt?.GetContent());
							Thread.Sleep(500);
							PazarYeriSiparisEkBilgi mpOrderAdditionalDataRecord = mpOrderAdditionalDatas.FirstOrDefault();
							decimal totalAmount = order.TotalPrice;
							int sachetCount = 0;
							if (mpOrderAdditionalDataRecord != null)
							{
								totalAmount = mpOrderAdditionalDataRecord.GuncelFaturaTutar ?? totalAmount;
								sachetCount = mpOrderAdditionalDataRecord.PosetSayisi;
							}
							var updatePackageAsInvoicedDto = new TGUpdatePackageInvoicedRequestDto()
							{
								InvoiceAmount = totalAmount,
								ReceiptLink = string.Empty
							};
							if (sachetCount > 0)
							{
								updatePackageAsInvoicedDto.BagCount = sachetCount;
							}
							Logger.Information("SaveTyGoCreatedOrdersAsync > UpdatePackageAsInvoiced Request: {@request}", fileName: _logFolderName, updatePackageAsInvoicedDto);
							var response = await _trendyolGoClient.UpdatePackageAsInvoiced(supplierId, createdOrder.PaketId, updatePackageAsInvoicedDto);
							Logger.Information("SaveTyGoCreatedOrdersAsync > UpdatePackageAsInvoiced Status Message: {statusCode} Response: {@response}", fileName: _logFolderName, response.ResponseMessage.StatusCode, response?.GetContent());
							if (response.ResponseMessage.IsSuccessStatusCode)
							{

								createdOrder.SevkiyatPaketDurumu = nameof(StatusEnums.Invoiced);
								createdOrder.Hata = string.Empty;
								createdOrder.HasSent = Character.E;
								await _pazarYeriSiparisDalService.UpdateOrderAsync(createdOrder);
								PazarYeriSiparisEkBilgi mpOrderAdditionalDataRecordToUpdate = mpOrderAdditionalDatas.LastOrDefault();
								mpOrderAdditionalDataRecordToUpdate.PosetTutari = mpOrderAdditionalDataRecord.PosetTutari;
								mpOrderAdditionalDataRecordToUpdate.PosetSayisi = mpOrderAdditionalDataRecord.PosetSayisi;
								mpOrderAdditionalDataRecordToUpdate.GuncelFaturaTutar = mpOrderAdditionalDataRecord.GuncelFaturaTutar;
								await _pazarYeriSiparisEkBilgiDalService.UpdateOrderAdditionalDataAsync(mpOrderAdditionalDataRecordToUpdate);
							}
						}
					}

					if (_sendQp && dbResultVal)
					{
                        var sendQpResult = await SendOrderToQp(pyBirimTanimDto, pyProductList, pyTransferredProductList, qpOrderSeqId, merchantNo: PazarYeri.TrendyolGo, sachetProduct: _apiDefinition.SachetProducts, errorMessages: errorMessages, orderDto: order);
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
			}
			catch (Exception ex)
			{
				errorMessages.AppendLine($"Hata: {ex.Message} Detay: {ex.InnerException?.Message ?? ""}");
				Logger.Error("SaveTyGoCreatedOrdersAsync Hata:{exception}", fileName: _logFolderName, ex);
			}
			finally
			{
				if (errorMessages.Length > 0)
				{
					//await _mailService.SendMailAsync(_logFolderName + $" Hata! Siparişler veritabanına kaydedilemedi. Pazar Yeri Birim No: {storeCreatedOrderList?.FirstOrDefault()?.StoreId ?? ""}", errorMessages.ToString());
                    await SendFailedOrderMail("TrendyolGo Servisinde Hata", $"Hata! Siparişler veritabanına kaydedilemedi. Pazar Yeri Birim No: {storeCreatedOrderList?.FirstOrDefault()?.StoreId ?? ""} - {errorMessages.ToString()}");
                }
            }
		}

		public async Task<ServiceResponse<CommonResponseDto>> SaveTyGoCreatedOrderAsync(TrendyolGoOrderDto order)
		{
			Logger.Information("SaveTyGoCreatedOrdersAsync  is running.", fileName: _logFolderName);
			using OperationContextScope operationContextScope = new(_qpClient.InnerChannel);
			string supplierId = _apiDefinition.SupplierId;
			StringBuilder errorMessages = new("");
			try
			{
				string merchantNo = PazarYeri.TrendyolGo;
				var pyProductList = await _pazarYeriMalTanimDalService.GetPyProductsAsync(merchantNo);
				bool sendToQp = true;
				PazarYeriBirimTanim pyBirimTanimDto =await _pazarYeriBirimTanimDalService.GetStoreDetailAsync(merchantNo, order.StoreId);

                var pyTransferredProductList = await _pazarYeriAktarimDalService.GetPyTransferredProductsAsync(merchantNo, order.StoreId);

				Logger.Information("SaveTyGoCreatedOrdersAsync Store Id:{store} Created Order Id:{orderId} Order: {@response}", fileName: _logFolderName, order.StoreId, order.OrderId, order);
				long qpOrderSeqId;
				bool dbResultVal;
				try
				{
					if (!await _pazarYeriSiparisDalService.OrderExistAsync(order.Id, merchantNo))
					{
						qpOrderSeqId = await _pazarYeriSiparisDalService.GetSeqId();
						dbResultVal = await SaveOrderDb(order, order.StoreId, qpOrderSeqId, pyProductList, pyTransferredProductList);
					}
					else
					{
						qpOrderSeqId = await _pazarYeriSiparisDalService.GetOrderIdByIdAsync(order.Id, merchantNo);
						dbResultVal = true;
					}
				}
				catch (Exception ex)
				{
					errorMessages.Append($"Sipariş No: {order.OrderNumber} için hata alındı. Hata: {ex.Message} Detay: {ex.InnerException?.Message ?? ""}");
					qpOrderSeqId = 0;
					dbResultVal = false;
					Logger.Error("SaveTyGoCreatedOrdersAsync An error occurred while querying or saving the order id{orderId} from the db.Hata: {exception}", fileName: _logFolderName, order.OrderId, ex);
				}

				long orderCountTransferredToQP = await _pazarYeriSiparisDalService.GetOrderWareHouseTransferredCountAsync(order.OrderNumber, merchantNo);
				long ordersCountWithSameOrderID = await _pazarYeriSiparisDalService.GetOrdersCountWithSameOrderIdAsync(order.OrderNumber, merchantNo);
				bool _sendQp = sendToQp && orderCountTransferredToQP < 1;

				if (ordersCountWithSameOrderID > 1)
				{
					var createdOrder = await _pazarYeriSiparisDalService.GetCreatedOrderByOrderNumberAsync(order.OrderNumber, merchantNo);
					var ordersByOrderNumber = await _pazarYeriSiparisUrunDalService.GetListAsync<PazarYeriSiparisUrun>(x => x.PySiparisNo == order.OrderNumber);
					var unSuppliedOrderId = ordersByOrderNumber.OrderBy(x => x.ObaseSiparisId).Select(x => x.ObaseSiparisId).FirstOrDefault();
					var unSuppliedOrder = ordersByOrderNumber.Where(x => x.ObaseSiparisId == unSuppliedOrderId);
					Logger.Information("Existing order! Entity: {@entity}", fileName: _logFolderName, createdOrder);
					if (createdOrder != null && order.Id == createdOrder.PaketId)
					{
						IEnumerable<PazarYeriSiparisEkBilgi> mpOrderAdditionalDatas = await _pazarYeriSiparisEkBilgiDalService.GetAdditionalDatasByOrderNumberAsync(order.OrderNumber);
						List<string> altSentPackageItemIds = new();
						bool areAlternativeProductsPresent = false;
						var marketPlaceOrderLineItems = await _pazarYeriSiparisDetayDalService.GetOrderDetailsdByIdAsync(createdOrder.Id);
						foreach (PazarYeriSiparisUrun mpOrderProduct in unSuppliedOrder)
						{
							var packageItemIdsForGivenProduct = marketPlaceOrderLineItems.Where(x => x.ObaseMalNo == mpOrderProduct.ObaseMalNo).Select(x => x.PaketItemId).ToList();
							if (mpOrderProduct.IsAlternativeEH == Character.E)
							{
								areAlternativeProductsPresent = true;
								List<string> altSentPackageItemIdsForCurrProduct = new();
								for (int i = 0; i < mpOrderProduct.AltUrunMiktar; i++)
								{
									string altProPackageID = packageItemIdsForGivenProduct.FirstOrDefault();
									packageItemIdsForGivenProduct.Remove(altProPackageID);
									altSentPackageItemIdsForCurrProduct.Add(altProPackageID);
								}
								altSentPackageItemIds.AddRange(altSentPackageItemIdsForCurrProduct);
							}
						}
						Thread.Sleep(500);

						TGMarkAlternativeRequestDto markAltDto = new();
						if (areAlternativeProductsPresent)
						{
							markAltDto.CollectedItemIdList = marketPlaceOrderLineItems.Select(x => x.PaketItemId).Except(altSentPackageItemIds).ToArray<string>();
							markAltDto.AlternativeItemIdList = altSentPackageItemIds.ToArray<string>();
						}
						else
						{
							markAltDto.CollectedItemIdList = marketPlaceOrderLineItems.Select(x => x.PaketItemId).ToArray<string>();
						}
						Logger.Information("SaveTyGoCreatedOrdersAsync Mark-Alternative Request: {@request}", fileName: _logFolderName, markAltDto);
						var responseAlt = await _trendyolGoClient.MarkAlternative(supplierId, createdOrder.PaketId, markAltDto);
						Logger.Information("SaveTyGoCreatedOrdersAsync Mark-Alternative Mark-Alternative Status Message: {statusCode} Response: {@response}", fileName: _logFolderName, responseAlt.ResponseMessage.StatusCode, responseAlt?.GetContent());
						Thread.Sleep(500);
						PazarYeriSiparisEkBilgi mpOrderAdditionalDataRecord = mpOrderAdditionalDatas.FirstOrDefault();
						decimal totalAmount = order.TotalPrice;
						int sachetCount = 0;
						if (mpOrderAdditionalDataRecord != null)
						{
							totalAmount = mpOrderAdditionalDataRecord.GuncelFaturaTutar ?? totalAmount;
							sachetCount = mpOrderAdditionalDataRecord.PosetSayisi;
						}
						var updatePackageAsInvoicedDto = new TGUpdatePackageInvoicedRequestDto()
						{
							InvoiceAmount = totalAmount,
							ReceiptLink = string.Empty
						};
						if (sachetCount > 0)
						{
							updatePackageAsInvoicedDto.BagCount = sachetCount;
						}
						Logger.Information("SaveTyGoCreatedOrdersAsync > UpdatePackageAsInvoiced Request: {@request}", fileName: _logFolderName, updatePackageAsInvoicedDto);
						var response = await _trendyolGoClient.UpdatePackageAsInvoiced(supplierId, createdOrder.PaketId, updatePackageAsInvoicedDto);
						Logger.Information("SaveTyGoCreatedOrdersAsync > UpdatePackageAsInvoiced Status Message: {statusCode} Response: {@response}", fileName: _logFolderName, response.ResponseMessage.StatusCode, response?.GetContent());
						if (response.ResponseMessage.IsSuccessStatusCode)
						{
							createdOrder.SevkiyatPaketDurumu = nameof(StatusEnums.Invoiced);
							createdOrder.Hata = string.Empty;
							createdOrder.HasSent = Character.E;
							await _pazarYeriSiparisDalService.UpdateOrderAsync(createdOrder);
							PazarYeriSiparisEkBilgi mpOrderAdditionalDataRecordToUpdate = mpOrderAdditionalDatas.LastOrDefault();
							mpOrderAdditionalDataRecordToUpdate.PosetTutari = mpOrderAdditionalDataRecord.PosetTutari;
							mpOrderAdditionalDataRecordToUpdate.PosetSayisi = mpOrderAdditionalDataRecord.PosetSayisi;
							mpOrderAdditionalDataRecordToUpdate.GuncelFaturaTutar = mpOrderAdditionalDataRecord.GuncelFaturaTutar;
							await _pazarYeriSiparisEkBilgiDalService.UpdateOrderAdditionalDataAsync(mpOrderAdditionalDataRecordToUpdate);
						}
					}
				}

				if (_sendQp && dbResultVal)
				{
                    var sendQpResult = await SendOrderToQp(pyBirimTanimDto, pyProductList, pyTransferredProductList, qpOrderSeqId, merchantNo: PazarYeri.TrendyolGo, sachetProduct: _apiDefinition.SachetProducts, errorMessages: errorMessages, orderDto: order);
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
				Logger.Error("SaveTyGoCreatedOrdersAsync Hata:{exception}", fileName: _logFolderName, ex);
                return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = errorMessages.ToString(), Success = false });
            }
			finally
			{
				if (errorMessages.Length > 0)
				{
					//await _mailService.SendMailAsync(_logFolderName + $" Hata! Siparişler veritabanına kaydedilemedi. Pazar Yeri Birim No: {order?.StoreId ?? ""}", errorMessages.ToString());
                    await SendFailedOrderMail("TrendyolGo Servisinde Hata", $"Hata! Siparişler veritabanına kaydedilemedi. Pazar Yeri Birim No: {order?.StoreId ?? ""} - {errorMessages.ToString()}");
                }
			}
			return ServiceResponse<CommonResponseDto>.Success(data: new CommonResponseDto { Message = "Order Saved", Success = true });
		}

		public async Task SaveTyGoCancelledOrdersAsync(List<TrendyolGoOrderDto> storeCreatedOrderList)
		{
			StringBuilder errorMessages = new StringBuilder("");
			Logger.Information("SaveTyGoCancelledOrdersAsync  is running.", fileName: _logFolderName);
			foreach (var itemOrder in storeCreatedOrderList)
			{
				try
				{
					var order = await _pazarYeriSiparisDalService.GetOrderWithOrderIdAsync(itemOrder.OrderId, PazarYeri.TrendyolGo);
					if (order == null || (order.SevkiyatPaketDurumu == TyGoConstants.Cancelled && order.DepoAktarildiEH == Character.E))
					{
						continue;
					}
					else
					{
						if (itemOrder.PackageStatus == TyGoConstants.UnSupplied && itemOrder.Lines.Exists(w => w.Items.Exists(x => !x.IsCancelled)))
						{
							continue;
						}
						var oldestOrder = await _pazarYeriSiparisDalService.GetOldestOrderWithOrderIdAsync(itemOrder.OrderId, PazarYeri.TrendyolGo);
						var id = order.Id;
						var response = await _qpClient.CancelOrderAsync(oldestOrder.Id.ToString(), "1");
						if (response.Response)
						{
							Logger.Information("Order is cancelled. QP Success With ID: {qpOrderId}", fileName: _logFolderName, oldestOrder.Id);
							if (order.Id != oldestOrder.Id)
							{
								oldestOrder.SevkiyatPaketDurumu = TyGoConstants.Cancelled;
								oldestOrder.DepoAktarildiEH = Character.E;
								await _pazarYeriSiparisDalService.UpdateOrderAsync(oldestOrder);
							}
							order.SevkiyatPaketDurumu = TyGoConstants.Cancelled;
							order.DepoAktarildiEH = Character.E;
							await _pazarYeriSiparisDalService.UpdateOrderAsync(order);
						}
						else
						{
							errorMessages.AppendLine($"İptal sipariş Quickpick'e aktarılırken hata alındı. Sipariş No: {itemOrder.OrderNumber}. Id: {id}");
							Logger.Information("Order is not cancelled. QP Error With ID: {qpOrderId} response:{response}", fileName: _logFolderName, id, response.Message);
						}
					}
				}
				catch (Exception ex)
				{
					errorMessages.AppendLine($"İptal sipariş işlenirken hata alındı. Sipariş No: {itemOrder.OrderNumber}. Hata: {ex.Message} Detay: {ex.InnerException?.Message ?? ""}");
					Logger.Error("SaveTyGoCancelledOrdersAsync Hata: {exception}", fileName: _logFolderName, ex);
				}
				finally
				{
					if (errorMessages.Length > 0)
					{
						//await _mailService.SendMailAsync(_logFolderName + $" Hata! İptal siparişler işlenirken hata alındı.", errorMessages.ToString());
                        await SendFailedOrderMail("TrendyolGo Servisinde Hata", $"Hata! İptal siparişler işlenirken hata alındı. - {errorMessages.ToString()}");
                    }
				}
			}
		}

		public async Task<CommonResponseDto> ClaimStatuUpdate(PostProductReturnRequestDto claimDto)
		{
			Logger.Information("ClaimStatuUpdate Request:{@request}", _logFolderName, claimDto);
			switch (claimDto.Result)
			{
				case TyGoConstants.Accepted:
					return await _tyGoReturnService.AcceptClaimAsync(claimDto);
				case TyGoConstants.Rejected:
					return await _tyGoReturnService.RejectClaimAsync(claimDto);
				default:
					return new CommonResponseDto() { Message = "ReasonCode sadece ACCEPTED veya REJECTED olabilir." };
			}
		}

        #endregion

        #region Private Methods

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

		private List<string> GetUnsuppliedLineItemPackageItemIds(IEnumerable<PazarYeriSiparisUrun> mpOrderProducts, IEnumerable<PazarYeriSiparisDetay> marketPlacesOrderDetails, List<ProductQuantity> qpProductQuantities, IEnumerable<PazarYeriMalTanim> productsWithKGUnit)
        {
            var allOrderItemsLineItemIDs = marketPlacesOrderDetails.ToList().Select(s => s.PaketItemId);
            var SuppliedLineItemItemIds = GetSuppliedLineItemPackageItemIds(mpOrderProducts, marketPlacesOrderDetails, qpProductQuantities, productsWithKGUnit);
            IEnumerable<string> unSuppliedLineItemIds = allOrderItemsLineItemIDs.Except(SuppliedLineItemItemIds);
            return unSuppliedLineItemIds.ToList();
        }

        private async Task<bool> SaveOrderDb(TrendyolGoOrderDto itemOrder, string storeId, long qpOrderSeqId, IEnumerable<PazarYeriMalTanim> products, IEnumerable<PazarYeriAktarim> transferProducts)
		{
			try
			{
				PazarYeriSiparis pazarYeriSiparis;
				string invoiceId = Guid.NewGuid().ToString();
				string shipmentAddressId = Guid.NewGuid().ToString();
				var merchantNo = PazarYeri.TrendyolGo;

				pazarYeriSiparis = new PazarYeriSiparis()
				{
					PazarYeriNo = merchantNo,
					Id = qpOrderSeqId,
					PaketId = itemOrder.Id,
					SiparisId = itemOrder.OrderId,
					SiparisNo = itemOrder.OrderNumber,
					SiparisTarih = UnixTimeStampToDateTime(itemOrder.OrderDate, merchantNo),
					TahminiTeslimBaslangicTarih = UnixTimeStampToDateTime(itemOrder.EstimatedDeliveryStartDate, merchantNo),
					TahminiTeslimBitisTarih = UnixTimeStampToDateTime(itemOrder.EstimatedDeliveryEndDate, merchantNo),
					BrutTutar = itemOrder.GrossAmount,
					ToplamIndirimTutar = itemOrder.TotalDiscount,
					ToplamTutar = itemOrder.TotalPrice,
					ParaBirimiKodu = itemOrder.CurrencyCode,
					TcKimlikNo = (itemOrder.ShipmentAddress?.IdentityNumber ?? "").Length > 11 ? (itemOrder.ShipmentAddress?.IdentityNumber ?? "")[..11] : (itemOrder.ShipmentAddress?.IdentityNumber ?? ""),
					MusteriId = itemOrder.Customer.Id,
					MusteriAdi = itemOrder.Customer.FirstName,
					MusteriSoyadi = itemOrder.Customer.LastName,
					MusteriEmail = itemOrder.Customer.Email,
					KargoTakipNo = string.Empty,
					KargoTakipUrl = itemOrder.CargoInfo != null ? itemOrder.CargoInfo.TrackingLink : string.Empty,
					KargoGondericiNumarasi = string.Empty,
					KargoSaglayiciAdi = !string.IsNullOrEmpty(itemOrder.DeliveryModel) ? itemOrder.DeliveryModel : string.Empty,
					SevkiyatPaketDurumu = itemOrder.PackageStatus,
					KargoAdresId = shipmentAddressId,
					FaturaAdresId = invoiceId,
					KoliAdeti = 0,
					Desi = 0,
					DepoAktarildiEH = Character.H
				};
				PazarYeriKargoAdres pazarYeriKargoAdres = null;
				if (itemOrder.ShipmentAddress != null)
				{
					pazarYeriKargoAdres = new PazarYeriKargoAdres()
					{
						Id = qpOrderSeqId,
						KargoAdresId = shipmentAddressId,
						AdSoyad = $"{itemOrder.ShipmentAddress.FirstName} {itemOrder.ShipmentAddress.LastName}",
						Ad = itemOrder.ShipmentAddress.FirstName,
						Soyad = itemOrder.ShipmentAddress.LastName,
						Adres1 = itemOrder.ShipmentAddress.Address1,
						Adres2 = itemOrder.ShipmentAddress.Address2,
						Sehir = itemOrder.ShipmentAddress.City.Length > 20 ? itemOrder.ShipmentAddress.City[..20] : itemOrder.ShipmentAddress.City,
						SehirKod = itemOrder.ShipmentAddress.CityCode,
						PostaKod = itemOrder.ShipmentAddress.PostalCode.Length > 15 ? itemOrder.ShipmentAddress.PostalCode[..15] : itemOrder.ShipmentAddress.PostalCode,
						Semt = itemOrder.ShipmentAddress.District,
						SemtId = itemOrder.ShipmentAddress.DistrictId,
						UlkeKod = itemOrder.ShipmentAddress.CountryCode,
						TamAdres = itemOrder.ShipmentAddress.AddressDescription
					};
				}
				PazarYeriFaturaAdres pazarYeriFaturaAdres = null;
				if (itemOrder.InvoiceAddress != null)
				{
					pazarYeriFaturaAdres = new PazarYeriFaturaAdres()
					{
						Id = qpOrderSeqId,
						FaturaAdresId = invoiceId,
						AdSoyad = $"{itemOrder.InvoiceAddress.FirstName} {itemOrder.ShipmentAddress.LastName}",
						Adi = itemOrder.InvoiceAddress.FirstName,
						Soyadi = itemOrder.InvoiceAddress.LastName,
						Adres1 = itemOrder.InvoiceAddress.Address1,
						Adres2 = itemOrder.InvoiceAddress.Address2,
						Sehir = itemOrder.InvoiceAddress.City.Length > 20 ? itemOrder.InvoiceAddress.City[..20] : itemOrder.InvoiceAddress.City,
						PostaKod = itemOrder.InvoiceAddress.PostalCode.Length > 15 ? itemOrder.InvoiceAddress.PostalCode[..15] : itemOrder.InvoiceAddress.PostalCode,
						Semt = itemOrder.InvoiceAddress.District,
						UlkeKod = itemOrder.InvoiceAddress.CountryCode,
						TamAdres = itemOrder.InvoiceAddress.AddressDescription,
						Firma = string.Empty,
					};
				}

				SachetProduct[] sachetProducts = _apiDefinition.SachetProducts;

				PazarYeriSiparisEkBilgi packagingInfo = new()
				{
					PosetSayisi = 1,
					PosetTutari = sachetProducts.Any() ? sachetProducts[0].Price : decimal.Parse("0.25"),
					PySiparisNo = itemOrder.OrderNumber,
					ObaseSiparisId = qpOrderSeqId,
					GonderimUcreti = itemOrder?.TotalCargo ?? 0
				};
				List<PazarYeriSiparisDetay> pazarYeriSiparisDetays = new();
				List<PazarYeriSiparisDetay> pazarYeriSiparisDetayList = pazarYeriSiparisDetays;
				PazarYeriSiparisDetay pazarYeriSiparisDetay;

				List<PazarYeriSiparisUrun> pazarYeriSiparisUrunList = new();
				PazarYeriSiparisUrun pazarYeriSiparisUrun;

				if (itemOrder.Lines.Any())
				{
					string urlSeperator = _appSetting.Value.ImageSize.UrlSeperator;
					string imageWidth = $"/{_appSetting.Value.ImageSize.Width}";
					string imageLength = $"/{_appSetting.Value.ImageSize.Length}/";
					string resizePathParameter = _appSetting.Value.ImageSize.ResizePathParameter;

					foreach (var lineItem in itemOrder.Lines)
					{
						StringBuilder imageUrl = new();
						var transferProduct = transferProducts.FirstOrDefault(x => (x.PazarYeriMalNo == lineItem?.Barcode?.Trim() && x.PazarYeriBirimNo == storeId));
						var product = transferProduct != null ? products.FirstOrDefault(x => x.MalNo.Trim() == transferProduct?.MalNo?.Trim()) : null;

						if (lineItem.Product.ImageUrls.Any())
						{
							foreach (string url in lineItem.Product.ImageUrls)
							{
								var urlArray = url.Split(urlSeperator);
								if (urlArray.Length == 2)
								{
									string baseUrl = urlArray[0] + urlSeperator;
									imageUrl.Append($"{baseUrl + resizePathParameter + imageWidth + imageLength + urlArray[1]},");
								}
								else
								{
									imageUrl.Append($"{url},");
								}
							}
						}
						pazarYeriSiparisUrun = new PazarYeriSiparisUrun()
						{
							ObaseSiparisId = qpOrderSeqId,
							PySiparisNo = itemOrder.OrderNumber,
							PazarYeriBirimId = storeId,
							ObaseMalNo = product?.MalNo ?? (lineItem.Sku ?? ""),
							AltUrunObaseMalNo = string.Empty,
							PazarYeriMalNo = lineItem.Barcode,
							AltUrunPazarYeriMalNo = string.Empty,
							Miktar = lineItem.Items.Count,
							GuncelMiktar = lineItem.Items.Count,
							IsAlternativeEH = Character.H,
							ImageUrl = imageUrl.ToString()
						};
						pazarYeriSiparisUrunList.Add(pazarYeriSiparisUrun);

						foreach (var item in lineItem.Items)
						{
							pazarYeriSiparisDetay = new PazarYeriSiparisDetay()
							{
								Id = qpOrderSeqId,
								LineItemId = item.Id,
								PaketItemId = item.PackageItemId,
								PazarYeriBirimId = storeId,
								ObaseMalNo = product?.MalNo ?? (lineItem.Sku ?? ""),
								PazarYeriMalNo = lineItem.MerchantSku,
								PazarYeriUrunKodu = string.Empty,
								PazarYeriMalAdi = lineItem.Product.Name,
								AlternatifUrunEH = item.IsAlternative ? Character.E : Character.H,
								Barkod = lineItem.Barcode?.ToString() ?? string.Empty,
								Miktar = item.Quantity,
								NetTutar = lineItem.Price,
								IndirimTutar = item.Discount,
								KdvTutar = lineItem.VatBaseAmount,
								BrutTutar = lineItem.Amount,
								KdvOran = lineItem.VatRatio,
								ParaBirimiKodu = string.Empty,
								SatisKampanyaId = string.Empty,
								UrunBoyutu = string.Empty,
								UrunRengi = string.Empty,
								SiparisUrunDurumAdi = string.Empty,
								IsCancelledEH = item.IsCancelled ? Character.E : Character.H,
								IsAlternativeEH = item.IsAlternative ? Character.E : Character.H,
								IsCollectedEH = item.IsCollected ? Character.E : Character.H
							};

							pazarYeriSiparisDetayList.Add(pazarYeriSiparisDetay);
						}
					}

					pazarYeriSiparisUrun = new PazarYeriSiparisUrun()
					{
						ObaseSiparisId = qpOrderSeqId,
						PySiparisNo = itemOrder.OrderNumber,
						PazarYeriBirimId = storeId,
						ObaseMalNo = sachetProducts.Any() ? sachetProducts[0].ProductCode : "44406",
						AltUrunObaseMalNo = string.Empty,
						PazarYeriMalNo = sachetProducts.Any() ? sachetProducts[0].ProductCode : "44406",
						AltUrunPazarYeriMalNo = string.Empty,
						Miktar = 1,
						GuncelMiktar = 1,
						IsAlternativeEH = Character.H,
						ImageUrl = ""
					};
					pazarYeriSiparisUrunList.Add(pazarYeriSiparisUrun);
				}
				#region Db Save
				await _transactionDalService.BeginTransactionAsync();

				await _pazarYeriSiparisDalService.AddOrderAsync(pazarYeriSiparis);
				await _pazarYeriSiparisDetayDalService.AddOrderDetailsAsync(pazarYeriSiparisDetayList);
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

				await _pazarYeriSiparisEkBilgiDalService.AddAdditionalDataAsync(packagingInfo);
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
				await SendFailedOrderMail("TrendyolGo Servisinde Hata", $"Hata! Sipariş Db'ye kaydedilemedi. Sipariş No: {itemOrder?.OrderNumber ?? ""}, {ex.Message} {ex.InnerException?.Message ?? ""}");
				Logger.Error("SaveOrderDb Exception {exception} for Order: {order}", fileName: _logFolderName, ex, itemOrder.OrderNumber);
				await _transactionDalService.RollbackTransactionAsync();
				return false;
			}
		}

		private async Task<bool> SendOrderToQp(PazarYeriBirimTanim pyBirimTanim, IEnumerable<PazarYeriMalTanim> pyProductList, IEnumerable<PazarYeriAktarim> pyTransferredProductList, long qpOrderSeqId, SachetProduct[] sachetProduct, TrendyolGoOrderDto orderDto, StringBuilder errorMessages = null, string merchantNo = "")
		{
			bool isSendToQp = false;
			try
			{
				orderDto.CargoProductCode = _appSetting?.Value?.CargoProductCode ?? string.Empty;
				var qpModel = _orderConvertService.ToQpOrder(orderDto, pyBirimTanim, pyProductList, pyTransferredProductList, qpOrderSeqId, merchantNo: PazarYeri.TrendyolGo, sachetProduct: _apiDefinition.SachetProducts);

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
						 Logger.Warning("TrendyolGo - SendOrderToQp - Retry attempt {Attempt} will be made after {Delay}s. Reason: {Message}", fileName: _logFolderName, args.AttemptNumber.ToString(), args.RetryDelay.TotalSeconds.ToString("F1"), args.Outcome.Exception?.Message ?? "Unknown error");
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
						throw new InvalidOperationException($"TrendyolGo - SendOrderToQp - Sipariş Quickpick'e aktarılırken bir hata oluştu ve tekrar gönderim sağlanacak. QP Response :{errorMessage} ");

					}
					catch (Exception ex)
					{
						Logger.Error("QP Error With Id: {qpOrderSeqId} For Order Number: {orderNumber}. Error: {exception}", fileName: _logFolderName, qpOrderSeqId, orderDto.OrderNumber, ex);
						throw new InvalidOperationException($"TrendyolGo - SendOrderToQp - Sipariş Quickpick'e aktarılırken bir hata oluştu ve tekrar gönderim sağlanacak. Error :{ex.Message + " " + ex.InnerException?.Message ?? ""} ");
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
        
		#endregion
    }
}