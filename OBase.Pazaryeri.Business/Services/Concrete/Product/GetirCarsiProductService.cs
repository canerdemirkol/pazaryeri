#region General

#endregion

#region Project
using OBase.Pazaryeri.Business.Client.Abstract;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Business.Services.Abstract.Product;
using OBase.Pazaryeri.DataAccess.Services.Abstract.Generic;
using OBase.Pazaryeri.Domain.Constants;
using OBase.Pazaryeri.Domain.Dtos.Getir.Product;
using OBase.Pazaryeri.Domain.Entities;
using OBase.Pazaryeri.Domain.Enums;
using static OBase.Pazaryeri.Domain.Constants.Constants;
#endregion

namespace OBase.Pazaryeri.Business.Services.Concrete.Product
{
	public class GetirCarsiProductService : IGetirCarsiProductService
	{
		#region Private
		private readonly IGetirCarsiClient _getirCarsiClient;
		private readonly IGetDalService _getDalService;
		private readonly ICreateDalService _createDalService;
		private readonly IDeleteDalService _deleteDalService;
		private readonly string _logFolderName = nameof(CommonEnums.PazarYerleri.GetirCarsi);
		#endregion

		#region Const
		public GetirCarsiProductService(IGetirCarsiClient getirCarsiClient,
			IGetDalService getDalService, ICreateDalService createDalService, IDeleteDalService deleteDalService)
		{
			_getirCarsiClient = getirCarsiClient;
			_getDalService = getDalService;
			_createDalService = createDalService;
			_deleteDalService = deleteDalService;
		}
		#endregion

		#region Metot
		public async Task GetGetirProductInfos(Dictionary<string, string> properties)
		{
			var shopList = await _getDalService.GetListAsync<PazarYeriBirimTanim>(x => x.PazarYeriNo == Constants.PazarYeri.GetirCarsi && x.AktifPasif == CommonConstants.Aktif);
			var shopIdList = shopList.Select(x => x.PazarYeriBirimNo).ToList();
			await _deleteDalService.TruncateTable(Constants.Db.Table.PazaryeriAlternatifGonderim.Name);
			foreach (string shopId in shopIdList)
			{
				try
				{
					var sizeResponse = await _getirCarsiClient.Products(shopId, 1, 1);
					if (sizeResponse.ResponseMessage.IsSuccessStatusCode)
					{
						int totalCountsize = sizeResponse?.GetContent()?.Data?.TotalCount ?? 1;
						int page = 0;
						do
						{
							page++;
							var allProductInfosForShopResponse = await _getirCarsiClient.Products(shopId, page, 1000);
							var allProductInfosForShop = allProductInfosForShopResponse?.GetContent()?.Data?.Data ?? null;

							if (allProductInfosForShopResponse.ResponseMessage.IsSuccessStatusCode && allProductInfosForShop is not null && allProductInfosForShop.Count > 0)
							{
								List<PazaryeriAlternatifGonderim> getirCarsiMalTanimList = new();
								foreach (GetirProductData product in allProductInfosForShop)
								{
									if (!string.IsNullOrEmpty(product?.VendorId))
									{
										try
										{
											var productOption = product.MenuOptions.MaxBy(x => x.Amount);
											getirCarsiMalTanimList.Add(new PazaryeriAlternatifGonderim
											{
												PazarYeriMalNo = product.VendorId,
												CatalogProductId = product.CatalogProductId,
												MenuProductId = product.MenuProductId,
												OptionId = productOption.OptionId,
												PazarYeriBirimNo = shopId,
												OptionAmount = productOption.Amount,
												OptionPrice = productOption.Price,
												ProductImage = product.Images.Any() ? product.Images.FirstOrDefault() : string.Empty,
											});
										}
										catch (Exception ex)
										{
											Logger.Error("GetGetirProductInfos > {shopId} birimi için {@product} kaydedilirken bir hata oluştu: {exception}", _logFolderName, shopId, product ?? null, ex);
											continue;
										}
									}
								}
								await _createDalService.AddRangeAsync(getirCarsiMalTanimList);
							}
							else
							{
								Logger.Warning("GetGetirProductInfos > {shopId} birimi için Getir'den ürün listesi boş döndü.", _logFolderName, shopId);
							}
						} while ((page * 1000) < totalCountsize);

					}
					else
					{
						Logger.Warning("GetGetirProductInfos > {shopId} birimi için ürün sorgulaması yapılırken Getir'den başarılı response alınamadı.", _logFolderName, shopId);
					}
				}
				catch (Exception ex)
				{
					Logger.Error("GetGetirProductInfos > {shopId} birimi için ürünler alınırken bir hata oluştu: {exception}", _logFolderName, shopId, ex);
					continue;
				}
			}
		}
		#endregion
	}
}