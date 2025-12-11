using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.General;
using OBase.Pazaryeri.Business.Services.Abstract.Return;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Order;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.Getir;
using OBase.Pazaryeri.Domain.Dtos.Getir.Return;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using QPService;
using System;
using System.Globalization;
using System.Text;
using static OBase.Pazaryeri.Core.Utility.Helper;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Services.Concrete.Return
{
    public class GetirCarsiReturnService : IGetirCarsiReturnService
	{
		#region Variables

		private readonly IIadeDalService _iadeDalService;
		private readonly IGetirCarsiClient _getirClient;
		private readonly OrderDeliveryServiceSoapClient _qpClient;
		private readonly AppSettings _appSettings;
		private readonly IGetDalService _getDalService;
		private readonly IMailService _mailService;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.GetirCarsi);

		#endregion

		#region Ctor
		public GetirCarsiReturnService(IIadeDalService iadeDalService, IGetirCarsiClient getirOrderClient, IOptionsSnapshot<AppSettings> options, IGetDalService getDalService, IMailService mailService)
		{
			_iadeDalService = iadeDalService;
			_getirClient = getirOrderClient;
			if (options.Value.WareHouseUrl is not null)
			{
				_qpClient = new OrderDeliveryServiceSoapClient(OrderDeliveryServiceSoapClient.EndpointConfiguration.OrderDeliveryServiceSoap12, remoteAddress: options.Value.WareHouseUrl);
			}
			_getDalService = getDalService;
			_appSettings = options.Value;
			_mailService = mailService;
        }

        public void Dispose()
        {

        }


        #endregion

        #region Methods
        public async Task<CommonResponseDto> AcceptClaimAsync(PostProductReturnRequestDto request)
        {
            var lst = await _getDalService.GetListAsync<PazarYeriSiparisIadeDetay>(x => x.ClaimId == request.ReturnId);
            if (!lst.Any())
                return new CommonResponseDto { Message = "Claim bulunamadı. Claim Id:" + request.ReturnId + "Order Id: " + request.OrderId, Success = true };

            List<GetirPostReturnReqProducts> products = new();
            foreach (var item in lst)
            {
                for (var i = 0; i < item.Sayisi; i++)
                {
                    products.Add(new()
                    {
                        id = item.ClaimDetayId,
                        responses = new() {
                        new() {
                            status = (int)CommonEnums.GetirReturnProductStatus.SHOP_APPROVE
                        }
                    }
                    });
                }
            }

            var result = await _getirClient.PostReturn(new() { returnId = request.ReturnId, products = products });
            if (result.ResponseMessage.IsSuccessStatusCode)
            {
                await _iadeDalService.UpdateReturnOrderClaimAsAcceptedAsync(request.ReturnId, GetirConstants.ShopAccepted);
                return new() { Message = "Onay işlemi başarılı. " + result?.GetContent()?.Meta.returnMessage ?? "", Success = true };
            }
            else
            {
                return new() { Message = "Onay işlemi başarısız. " + JsonConvert.DeserializeObject<Meta>(result.StringContent)?.returnMessage ?? "", Success = false };
            }
        }

        public async Task<List<GetirReturnsItemDtoWithShopId>> GetGetirClaimsAsync(string status, ReturnReqBody startend)
        {
            StringBuilder errorMessages = new StringBuilder("");
            List<GetirReturnsItemDtoWithShopId> res = new();
            var birimLst = await _getDalService.GetListAsync<PazarYeriBirimTanim>(x => x.PazarYeriNo == PazarYeri.GetirCarsi && x.AktifPasif == CommonConstants.Aktif);
            foreach (var item in birimLst)
            {
                List<GetirReturnsItemDto> returns = new();
                int page = 0;
                for (var pageDone = false; pageDone == false; page++)
                {
                    var response = (await _getirClient.GetReturnedPackagesAsync(item.PazarYeriBirimNo, status, page, 10, startend));
                    if (response.ResponseMessage.IsSuccessStatusCode)
                    {
                        var resp = response.GetContent();
                        if (resp?.Data.Returns.Count > 0)
                            returns = returns.Concat(resp?.Data.Returns).ToList();
                        if (resp?.Data.Returns.Count < 10)
                            pageDone = true;
                    }
                    else
                    {
                        pageDone = true;
                        errorMessages.AppendLine("<br/> " + _logFolderName + $" {item} birimi için iadeler çekilirken bir hata oluştur. Hata: {response.StringContent}");
                        Logger.Error("ReturnService => GetGetirClaimsAsync - GetReturnedPackagesAsync - ShopId: {ShopId} Error: {response}", fileName: _logFolderName, item.PazarYeriBirimNo, response.StringContent);
                    }
                }
                if (returns.Count > 0)
                    res.Add(new() { ShopId = item.PazarYeriBirimNo, Returns = returns });
            }
            foreach (var item in res)
            {
                foreach (var itemIn in item.Returns)
                {
                    var response = (await _getirClient.GetReturn(itemIn.Id.ToString()));
                    if (response.ResponseMessage.IsSuccessStatusCode)
                        itemIn.Detail = response.GetContent()?.Data;
                    else
                    {
                        Logger.Error("ReturnService => GetGetirClaimsAsync - GetReturn Error: {response}", fileName: _logFolderName, response.StringContent);
                        errorMessages.AppendLine("<br/> " + _logFolderName + $"{itemIn.Id} iadesi için iade detayı çekilirken bir hata oluştur. Hata: {response.StringContent}");
                    }
                }
            }
            if ((_appSettings.MailSettings?.MailEnabled ?? false) && errorMessages.Length > 0)
            {
                await _mailService.SendMailAsync(_logFolderName + $" Iadeler işlenirken Hata!.", errorMessages.ToString());
            }
            return res;
        }

        public async Task<CommonResponseDto> RejectClaimAsync(PostProductReturnRequestDto request)
        {
            GetirRejectReasonIDs myEnum = (GetirRejectReasonIDs)Enum.Parse(typeof(GetirRejectReasonIDs), request.ReasonCode.ToUpper());
            var lst = await _getDalService.GetListAsync<PazarYeriSiparisIadeDetay>(x => x.ClaimId == request.ReturnId);
            if (!lst.Any())
            {
                return new CommonResponseDto { Message = "Claim bulunamadı. Claim Id:" + request.ReturnId + "Order Id: " + request.OrderId, Success = true };
            }

            List<GetirPostReturnReqProducts> products = new();
            foreach (var item in lst)
            {
                for (var i = 0; i < item.Sayisi; i++)
                {
                    products.Add(new()
                    {
                        id = item.ClaimDetayId,
                        responses = new List<GetirPostReturnReqProductResponse>() {
                        new GetirPostReturnReqProductResponse() {
                            status = (int)CommonEnums.GetirReturnProductStatus.SHOP_REJECT,
                            rejectReasonId = (int)myEnum
                        }
                    }
                    });
                }
            }

            var result = await _getirClient.PostReturn(new() { returnId = request.ReturnId, products = products });
            if (result.ResponseMessage.IsSuccessStatusCode)
            {
                await _iadeDalService.UpdateReturnOrderClaimAsRejectedAsync(request.ReturnId, request.ReasonDescription, GetirConstants.ShopRejected);
                return new CommonResponseDto { Message = "Reddetme işlemi başarılı." + result?.GetContent()?.Meta.returnMessage ?? "", Success = true };
            }
            else
            {
                return new CommonResponseDto { Message = "Reddetme işlemi başarısız." + JsonConvert.DeserializeObject<Meta>(result.StringContent)?.returnMessage ?? "", Success = false };
            }
        }

        public async Task<bool> SaveClaimToDbAsync(GetirReturnsItemDtoWithShopId claim)
        {
            if (claim is not null && claim.Returns.Any())
            {
                foreach (var item in claim.Returns)
                {
                    if (!await _iadeDalService.ClaimExistsAsync(item.Id.ToString()))
                    {
                        PazarYeriSiparisIade iade = new()
                        {
                            ClaimId = item.Id.ToString(),
                            ClaimStatus = item.StatusMessage,
                            ClaimTarih = item.RequestedDate != null ? DateTime.ParseExact(item.RequestedDate, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) : null,
                            MusteriAd = item.CustomerName,
                            DepoAktarildiEH = Character.H,
                            SiparisTarih = item.ShopResponseDate != null ? DateTime.ParseExact(item.ShopResponseDate, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) : null,
                            SiparisNo = item.Detail.OrderInfo.OrderId,
                            PazarYeriNo = PazarYeri.GetirCarsi,
                            DepoAktarimDenemeSayisi = 0,
                            PazarYeriBirimNo = claim.ShopId,
                            Id = await _iadeDalService.GetIdByOrderIdAsync(item.Detail.OrderInfo.OrderId)
                        };
                        if (iade.Id != 0)
                        {
                            var groupList = item.Detail.Products.GroupBy(x => x.Id).ToList();
                            List<PazarYeriSiparisIadeDetay> iadeDetaylar = new();
                            foreach (var claimItem in groupList)
                            {
                                var pazarYeriSiparisDetayObj = await _getDalService.GetAsync<PazarYeriSiparisDetay>(x => x.LineItemId == claimItem.Key);
                                PazarYeriSiparisIadeDetay detay = new()
                                {
                                    PyIadeSebepKod = claimItem.FirstOrDefault().RejectReasonId.ToString(),
                                    PyIadeSebepAd = claimItem.FirstOrDefault().RejectReasonDescription,
                                    ClaimId = iade.Id.ToString(),
                                    ClaimDetayId = claimItem.FirstOrDefault().Id,
                                    ClaimItemStatus = claimItem.FirstOrDefault().Status.ToString(),
                                    MusteriIadeSebepKod = claimItem.FirstOrDefault().ReturnReasonDescription,
                                    MusteriIadeSebepAd = claimItem.FirstOrDefault().ReturnReasonId,
                                    Miktar = pazarYeriSiparisDetayObj.Miktar,
                                    Sayisi = claimItem.Count(),
                                    ClaimImageUrls = claimItem.FirstOrDefault().ImageUrl
                                };
                                iadeDetaylar.Add(detay);
                            }
                            iade.PazarYeriSiparisIadeDetay = iadeDetaylar;
                            await _iadeDalService.InsertReturnOrderClaimAsync(iade);
                        }
                        else
                        {
                            Logger.Error("ReturnService => SaveClaimToDbAsync Error: {orderNumber} numaralı sipariş kayıtlarda bulunamadı. Insert başarısız.", fileName: _logFolderName, item.ReturnCode);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public async Task SendClaimToQPAsync(GetirReturnsItemDtoWithShopId claim)
        {
            foreach (var claimItem in claim.Returns)
            {
                if (await _iadeDalService.CheckIfClaimSentToQpAsync(claimItem.Id.ToString()))
                {
                    var returnEntity = await _getDalService.GetAsync<PazarYeriSiparisIade>(x => x.ClaimId == claimItem.Id.ToString());
                    if (returnEntity == null)
                    {
                        Logger.Warning("GetirReturnService => SendClaimToQPAsync =>  {claimId} ClaimId si PazarYeriSiparisIade tablosunda bulunamadı.", fileName: _logFolderName, claimItem.Id);
                        continue;
                    }

                    ProductReturnRequestModel qpReturnModel = new ProductReturnRequestModel();
                    var idResult = await _iadeDalService.GetIdByOrderIdAsync(claimItem.Detail.OrderInfo.OrderId);
                    qpReturnModel.OrderId = (int)idResult;
                    qpReturnModel.ReturnDate = DateTime.ParseExact(claimItem.RequestedDate, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                    qpReturnModel.ReturnId = claimItem.Id.ToString();
                    List<ProductReturnItemRequestModel> itemList = new();
                    var returnProducts = await _getDalService.GetListAsync<PazarYeriSiparisDetay>(x => x.Id == idResult);

                    foreach (var item in returnProducts)
                    {
                        ProductReturnItemRequestModel productReturnItemModel = new();
                        var claimItemForProduct = claimItem.Detail.Products.FirstOrDefault(x => x.Id.ToString() == item.LineItemId);
                        if (claimItemForProduct == null) continue;
                        productReturnItemModel.CustomerNote = claimItemForProduct.StatusDescription;
                        productReturnItemModel.CustomerReason = new()
                        {
                            Code = claimItemForProduct.ReturnReasonId,
                            Id = claimItemForProduct.ReturnReasonId,
                            Name = claimItemForProduct.ReturnReasonDescription
                        };
                        productReturnItemModel.SaleChannelReason = new()
                        {
                            Code = claimItemForProduct.RejectReasonId.ToString(),
                            Id = claimItemForProduct.RejectReasonId.ToString(),
                            Name = claimItemForProduct.RejectReasonDescription
                        };
                        productReturnItemModel.Note = claimItemForProduct.StatusDescription;

                        productReturnItemModel.ProductId = item.ObaseMalNo;
                        productReturnItemModel.ProductName = item.PazarYeriMalAdi;
                        productReturnItemModel.Quantity = item.Miktar;
                        if (!String.IsNullOrEmpty(claimItemForProduct.ImageUrl))
                            productReturnItemModel.Images = new string[] { claimItemForProduct.ImageUrl };
                        itemList.Add(productReturnItemModel);
                    }
                    qpReturnModel.Items = itemList.ToArray();

                    if (returnEntity.DepoAktarimDenemeSayisi >= 3)
                    {
                        Logger.Warning("GetirReturnService => SendClaimToQPAsync =>  {claimId} aktarım sayısı 3 den fazla.", fileName: _logFolderName, claimItem.Id);
                        continue;
                    }
                    await _iadeDalService.UpdateClaimsTryCountAsync(returnEntity);

                    var result = await _qpClient.ProductReturnAsync(qpReturnModel);
                    if (result.ProductReturnResult.Success)
                    {
                        await _iadeDalService.UpdateClaimAsSentAsync(claimItem.Id.ToString());
                        Logger.Information("GetirReturnService => SendClaimToQPAsync =>  {claimId} QP'ye aktarım başarılı.", fileName: _logFolderName, claimItem.Id);
                    }
                    else
                    {
                        Logger.Error("GetirReturnService => SendClaimToQPAsync =>  {claimId} QP'ye aktarım başarısız. \r\nRequest: {@request} \r\nResponse: {@response}", fileName: _logFolderName, claimItem.Id, qpReturnModel, result);
                    }
                }
            }
        }

        #endregion


    }
}
