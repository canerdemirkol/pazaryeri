using AutoMapper;
using Microsoft.Extensions.Options;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.Helper;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Return;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using OBase.Pazaryeri.Domain.Helper;
using QPService;
using RestEase;
using static OBase.Pazaryeri.Domain.Constants.Constants;

namespace OBase.Pazaryeri.Business.Services.Concrete.Return
{
    public class TyGoReturnService : ITrendyolGoReturnService
	{
        #region Variables

		private readonly IIadeDalService _iadeDalService;
        private readonly ITrendyolGoClient _tgClient;
        private readonly QPService.OrderDeliveryServiceSoapClient _qpClient;
        private readonly IOptions<AppSettings> _options;
        private readonly IMapper _mapper;
        private readonly ApiDefinitions _apiDefinition;
        private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.TrendyolGo);

        #endregion

        #region Ctor

		public TyGoReturnService(IIadeDalService iadeDalService, ITrendyolGoClient tgOrderClient, IOptions<AppSettings> options, IMapper mapper)
        {
            _iadeDalService = iadeDalService;
            _tgClient = tgOrderClient;
            _options = options;
            if (_options.Value.WareHouseUrl is not null)
            {
                _qpClient = new QPService.OrderDeliveryServiceSoapClient(QPService.OrderDeliveryServiceSoapClient.EndpointConfiguration.OrderDeliveryServiceSoap12, remoteAddress: _options.Value.WareHouseUrl);
            }
            _mapper = mapper;
            _apiDefinition = _options.Value.ApiDefinitions?.FirstOrDefault(x => x.Merchantno == PazarYeri.TrendyolGo);
        }

        #endregion

        #region Methods

        public async Task<CommonResponseDto> AcceptClaimAsync(PostProductReturnRequestDto request)
        {
            var claimItemIdList = await _iadeDalService.GetClaimItemIdsByClaimIdAsync(request.ReturnId);
            if (!claimItemIdList.Any())
            {
                return OrderHelper.ReturnQPResponseV2(System.Net.HttpStatusCode.BadRequest, _logFolderName, false, "Claim bulunamadı. Claim Id:" + request.ReturnId + "Order Id: " + request.OrderId);
            }
            TGAcceptClaimRequestDto tgRequest = new TGAcceptClaimRequestDto();
            tgRequest.ClaimId = request.ReturnId;
            tgRequest.ClaimItemIds = claimItemIdList;
            Logger.Information("AcceptClaim Request {@request}:", _logFolderName, tgRequest);
            var result = await _tgClient.AcceptClaimAsync(_apiDefinition.SupplierId, request.ReturnId, tgRequest);
            Logger.Information("AcceptClaim StatusCode:{statuscode} Response {@response}:", _logFolderName, result.ResponseMessage.StatusCode, result?.GetContent());
            if (result.ResponseMessage.IsSuccessStatusCode)
            {
                await _iadeDalService.UpdateReturnOrderClaimAsAcceptedAsync(request.ReturnId, TyGoConstants.Accepted);
                return OrderHelper.ReturnQPResponseV2(System.Net.HttpStatusCode.OK, _logFolderName, true, "Onay işlemi başarılı." + result?.GetContent()?.Message ?? "");
            }
            else
            {
                return OrderHelper.ReturnQPResponseV2(System.Net.HttpStatusCode.BadRequest, _logFolderName, false, "Onay işlemi başarısız." + result?.GetContent()?.Message ?? "");
            }
        }

        public async Task<Response<TGGetReturnedPackagesResponseDto>> GetTyGoClaimsAsync(string startDate, string status, string endDate, string page)
        {
            var response = await _tgClient.GetReturnedPackagesAsync(_apiDefinition.SupplierId, status, startDate, endDate, page);
            return response;
        }

        public async Task<CommonResponseDto> RejectClaimAsync(PostProductReturnRequestDto request)
        {
            var claimItemIdList = await _iadeDalService.GetClaimItemIdsByClaimIdAsync(request.ReturnId);
            if (!claimItemIdList.Any())
            {
                return OrderHelper.ReturnQPResponseV2(System.Net.HttpStatusCode.BadRequest, _logFolderName, false, "Claim bulunamadı. Claim Id:" + request.ReturnId + "Order Id: " + request.OrderId);
            }
            TGRejectClaimRequestDto tgRequest = new TGRejectClaimRequestDto();
            tgRequest.ClaimId = request.ReturnId;
            tgRequest.ClaimItemIds = claimItemIdList;
            tgRequest.Description = request.ReasonDescription ?? "";
            int reasonCodeInt;
            tgRequest.ReasonId = int.TryParse(request.ReasonCode, out reasonCodeInt) ? reasonCodeInt : TyGoConstants.RejectClaimReason;
            Logger.Information("RejectClaim Request {@request}:", _logFolderName, tgRequest);
            var result = await _tgClient.RejectClaimAsync(_apiDefinition.SupplierId, request.ReturnId, tgRequest);
            Logger.Information("RejectClaim StatusCode:{statuscode}, Response {@response}:", _logFolderName, result.ResponseMessage.StatusCode, result?.GetContent());
            if (result.ResponseMessage.IsSuccessStatusCode)
            {
                await _iadeDalService.UpdateReturnOrderClaimAsRejectedAsync(request.ReturnId, tgRequest.Description, TyGoConstants.Rejected);
                return OrderHelper.ReturnQPResponseV2(System.Net.HttpStatusCode.OK, _logFolderName, true, "Reddetme işlemi başarılı." + result?.GetContent()?.Message ?? "");
            }
            else
            {
                return OrderHelper.ReturnQPResponseV2(System.Net.HttpStatusCode.BadRequest, _logFolderName, false, "Reddetme işlemi başarısız." + result?.GetContent()?.Message ?? "");
            }
        }

        public async Task<bool> SaveClaimToDbAsync(ClaimContent claim)
        {
            if (claim is not null)
            {
                if (!await _iadeDalService.ClaimExistsAsync(claim.Id))
                {
                    Logger.Information("SaveClaimToDbAsync > Claim response: {@response}", _logFolderName, claim);
                    var iade = _mapper.Map<PazarYeriSiparisIade>(claim);
                    iade.PazarYeriNo = PazarYeri.TrendyolGo;
                    iade.Id = await _iadeDalService.GetIdByOrderNumberAscAsync(claim.OrderNumber);
                    if (iade.Id != 0)
                    {
                        List<PazarYeriSiparisIadeDetay> iadeDetaylar = new List<PazarYeriSiparisIadeDetay> { };
                        foreach (var claimItem in claim.ClaimItems)
                        {
                            var imageSettings = _options.Value.ImageSize;
                            var detay = _mapper.Map<PazarYeriSiparisIadeDetay>(claimItem);
                            detay = _mapper.Map(claimItem.TrendyolClaimItemReason, detay);
                            detay = _mapper.Map(claimItem.CustomerClaimItemReason, detay);
                            detay = _mapper.Map(claimItem.ClaimItemStatus, detay);
                            detay.ClaimId = iade.ClaimId.ToString();
                            detay.Miktar = 1;
                            detay.Sayisi = 1;
                            detay.ClaimImageUrls = claimItem.ClaimImageUrls.ResizeTyGoImageUrl(imageSettings.Width, imageSettings.Length, imageSettings.UrlSeperator, imageSettings.ResizePathParameter);
                            iadeDetaylar.Add(detay);
                        }
                        iade.PazarYeriSiparisIadeDetay = iadeDetaylar;
                        iade.DepoAktarimDenemeSayisi++;
                        await _iadeDalService.InsertReturnOrderClaimAsync(iade);
                        return true;
                    }
                    else
                    {
                        Logger.Error("TGReturnService => SaveClaimToDbAsync Error: {orderNumber} numaralı sipariş kayıtlarda bulunamadı. Insert başarısız.", fileName: _logFolderName, claim.OrderNumber);
                        return false;
                    }
                }
            }
            return false;
        }

        public async Task SendClaimToQPAsync(ClaimContent claim)
        {
            if (await _iadeDalService.CheckIfClaimSentToQpAsync(claim.Id))
            {
                ProductReturnRequestModel qpReturnModel = new();
                var idResult = await _iadeDalService.GetIdByOrderNumberDescAsync(claim.OrderNumber);
                qpReturnModel.OrderId = ((int)idResult);
                qpReturnModel.ReturnDate = DateTimeOffset.FromUnixTimeMilliseconds(claim.ClaimDate).DateTime;
                qpReturnModel.ReturnId = claim.Id;
                var imageSettings = _options.Value.ImageSize;
                List<ProductReturnItemRequestModel> itemList = new();
                List<string> lineItemsList = new();
                foreach (ClaimItem item in claim.ClaimItems)
                {
                    lineItemsList.Add(item.OrderLineItemId.ToString());
                }
                var returnProducts = _iadeDalService.GetClaimProductInfos(lineItemsList);
                var productNos = returnProducts.Select(x => x.ObaseMalNo).Distinct().ToList();

                foreach (var item in productNos)
                {
                    ProductReturnItemRequestModel productReturnItemModel = new ProductReturnItemRequestModel();
                    var productInfoForClaim = returnProducts.FirstOrDefault(x => x.ObaseMalNo == item);
                    var claimItemForProduct = claim.ClaimItems.FirstOrDefault(x => x.OrderLineItemId.ToString() == productInfoForClaim.LineItemId);
                    productReturnItemModel.CustomerNote = claimItemForProduct.CustomerNote;
                    var InfosForClaimProduct = returnProducts.Where(x => x.ObaseMalNo == item);
                    List<string> imageUrlsNotSized = new();
                    List<string> imageUrlsReSized = new();
                    foreach (var productInfo in InfosForClaimProduct)
                    {
                        var imagesForLineItem = (claim.ClaimItems.FirstOrDefault(x => x.OrderLineItemId == productInfo.LineItemId)?.ClaimImageUrls ?? null);
                        if (imagesForLineItem is not null && imagesForLineItem.Any())
                        {
                            imageUrlsNotSized.AddRange(imagesForLineItem);
                        }
                    }
                    foreach (var image in imageUrlsNotSized)
                    {
                        imageUrlsReSized.Add(image.ResizeTyGoImageUrl(imageSettings.Width, imageSettings.Length, imageSettings.UrlSeperator, imageSettings.ResizePathParameter));//ToDo: will send to QP
                    }
                    productReturnItemModel.CustomerReason = new ProductReturnReasonRequestModel
                    {
                        Code = claimItemForProduct.CustomerClaimItemReason.Code,
                        Id = claimItemForProduct.CustomerClaimItemReason.Id.ToString(),
                        Name = claimItemForProduct.CustomerClaimItemReason.Name
                    };
                    productReturnItemModel.SaleChannelReason = new ProductReturnReasonRequestModel
                    {
                        Code = claimItemForProduct.TrendyolClaimItemReason.Code,
                        Id = claimItemForProduct.TrendyolClaimItemReason.Id.ToString(),
                        Name = claimItemForProduct.TrendyolClaimItemReason.Name
                    };
                    productReturnItemModel.Note = claimItemForProduct.Note;
                    productReturnItemModel.Images = imageUrlsReSized.ToArray();
                    productReturnItemModel.ProductId = productInfoForClaim.ObaseMalNo;
                    productReturnItemModel.ProductName = productInfoForClaim.PazarYeriMalAdi;
                    productReturnItemModel.Quantity = returnProducts.Count(x => x.ObaseMalNo == item);
                    itemList.Add(productReturnItemModel);
                }
                qpReturnModel.Items = itemList.ToArray();
                Logger.Information("SendClaimToQPAsync => QuickPick Claim Request: {@request}", _logFolderName, qpReturnModel);
                var result = await _qpClient.ProductReturnAsync(qpReturnModel);
                if (result.ProductReturnResult.Success)
                {
                    await _iadeDalService.UpdateClaimAsSentAsync(claim.Id);
                    Logger.Information("TGReturnService => SendClaimToQPAsync =>  {claimId} QP'ye aktarım başarılı.", fileName: _logFolderName, claim.Id);
                }
                else
                {
                    Logger.Error("TGReturnService => SendClaimToQPAsync =>  {claimId} QP'ye aktarım başarısız. Request: {@request} Response: {@response}", fileName: _logFolderName, claim.Id, qpReturnModel, result);
                }
            }
        }

        public async Task<List<PazarYeriSiparisIade>> GetClaimsToSendQpAsync(string merchantNo)
        {
            return await _iadeDalService.GetClaimsToSendQpAsync(merchantNo);
        }

        public async Task SendIadeToQPAsync(PazarYeriSiparisIade iade)
        {
            ProductReturnRequestModel qpReturnModel = new();
            qpReturnModel.OrderId = Convert.ToInt32(iade.Id);
            qpReturnModel.ReturnDate = iade.ClaimTarih ?? DateTime.MinValue;
            qpReturnModel.ReturnId = iade.ClaimId;
            List<ProductReturnItemRequestModel> itemList = new();
            List<string> lineItemsList = new();
            foreach (PazarYeriSiparisIadeDetay item in iade.PazarYeriSiparisIadeDetay)
            {
                lineItemsList.Add(item.OrderLineItemId.ToString());
            }
            var returnProducts = _iadeDalService.GetClaimProductInfos(lineItemsList);
            var productNos = returnProducts.Select(x => x.ObaseMalNo).Distinct().ToList();

            foreach (var item in productNos)
            {
                ProductReturnItemRequestModel productReturnItemModel = new ProductReturnItemRequestModel();
                var productInfoForClaim = returnProducts.FirstOrDefault(x => x.ObaseMalNo == item);
                var claimItemForProduct = iade.PazarYeriSiparisIadeDetay.FirstOrDefault(x => x.OrderLineItemId == productInfoForClaim.LineItemId);
                productReturnItemModel.CustomerNote = claimItemForProduct.MusteriNot;
                productReturnItemModel.CustomerReason = new ProductReturnReasonRequestModel
                {
                    Code = claimItemForProduct.MusteriIadeSebepKod,
                    Id = claimItemForProduct.MusteriIadeSebepId.ToString(),
                    Name = claimItemForProduct.MusteriIadeSebepAd
                };
                productReturnItemModel.SaleChannelReason = new ProductReturnReasonRequestModel
                {
                    Code = claimItemForProduct.PyIadeSebepKod,
                    Id = claimItemForProduct.PyIadeSebepId.ToString(),
                    Name = claimItemForProduct.PyIadeSebepAd
                };
                string claimItemImages = claimItemForProduct?.ClaimImageUrls ?? "";
                if (!string.IsNullOrEmpty(claimItemImages))
                {
                    var imagesToSend = claimItemImages.Split(',');
                    productReturnItemModel.Images = imagesToSend.Take(imagesToSend.Length - 1).ToArray();
                }

                productReturnItemModel.Note = claimItemForProduct.Aciklama;
                productReturnItemModel.ProductId = productInfoForClaim.ObaseMalNo;
                productReturnItemModel.ProductName = productInfoForClaim.PazarYeriMalAdi;
                productReturnItemModel.Quantity = returnProducts.Count(x => x.ObaseMalNo == item);
                itemList.Add(productReturnItemModel);
            }
            qpReturnModel.Items = itemList.ToArray();
            Logger.Information("SendIadeToQPAsync => QuickPick Return Request: {@request}", _logFolderName, qpReturnModel);
            var result = await _qpClient.ProductReturnAsync(qpReturnModel);
            if (result.ProductReturnResult.Success)
            {
                await _iadeDalService.UpdateClaimAsSentAsync(iade.ClaimId);
                Logger.Information("TGReturnService => SendIadeToQPAsync =>  {claimId} QP'ye aktarım başarılı.", fileName: _logFolderName, iade.ClaimId);

            }
            else
            {
                Logger.Error("TGReturnService => SendIadeToQPAsync =>  {claimId} QP'ye aktarım başarısız. Request: {@request} Response: {@response}", fileName: _logFolderName, iade.ClaimId, qpReturnModel, result);
            }
        }

        public async Task UpdateClaimsTryCountAsync(PazarYeriSiparisIade iade)
        {
            await _iadeDalService.UpdateClaimsTryCountAsync(iade);
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
