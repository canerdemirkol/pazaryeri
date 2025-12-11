using OBase.Pazaryeri.Business.Factories;
using OBase.Pazaryeri.Business.LogHelper;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Constants;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using OBase.Pazaryeri.Domain.Dtos.Getir.Orders;
using OBase.Pazaryeri.Domain.Dtos.HepsiExpress;
using OBase.Pazaryeri.Domain.Dtos.QuickPick;
using OBase.Pazaryeri.Domain.Dtos.TrendyolGo;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;
using OBase.Pazaryeri.Domain.Entities;
using QPService;
using System.Net;
using System.Text;
using static OBase.Pazaryeri.Domain.Constants.Constants;
using static OBase.Pazaryeri.Domain.Helper.CommonHelper;
using Item = QPService.Item;
using Payment = QPService.Payment;
using PazarYeriAktarim = OBase.Pazaryeri.Domain.Entities.PazarYeriAktarim;

namespace OBase.Pazaryeri.Business.Helper
{
    public static partial class OrderHelper
    {
        public static CommonResponseDto ReturnQPResponseV2(HttpStatusCode httpStatusCode, string logFolderName, bool success = false, string message = "")
        {
            CommonResponseDto messageRespDto = new() { Message = $"Pazaryeri | {message}", Success = success, StatusCode = httpStatusCode };
            if (success)
                Logger.Information("ReturnQPResponseV2 : {@response}", logFolderName, messageRespDto);
            else
                Logger.Warning("ReturnQPResponseV2 : {@response}", logFolderName, messageRespDto);

            return messageRespDto;

        }
        public static List<string> GetSuppliedLineItemPackageItemIds(IEnumerable<PazarYeriSiparisUrun> mpOrderProducts, IEnumerable<PazarYeriSiparisDetay> marketPlacesOrderDetails, List<ProductQuantity> qpProductQuantities, IEnumerable<PazarYeriMalTanim> productsWithKGUnit)
        {
            List<string> suppliedPackageItemIds = new();

            foreach (var mpOrderProduct in mpOrderProducts)
            {
                ProductQuantity qpProduct = qpProductQuantities.FirstOrDefault(qpX => qpX.ProductId == mpOrderProduct.ObaseMalNo);
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
        public static List<string> GetUnsuppliedLineItemPackageItemIds(IEnumerable<PazarYeriSiparisUrun> mpOrderProducts, IEnumerable<PazarYeriSiparisDetay> marketPlacesOrderDetails, List<ProductQuantity> qpProductQuantities, IEnumerable<PazarYeriMalTanim> productsWithKGUnit)
        {
            var allOrderItemsLineItemIDs = marketPlacesOrderDetails.ToList().Select(s => s.PaketItemId);
            var SuppliedLineItemItemIds = GetSuppliedLineItemPackageItemIds(mpOrderProducts, marketPlacesOrderDetails, qpProductQuantities, productsWithKGUnit);
            IEnumerable<string> unSuppliedLineItemIds = allOrderItemsLineItemIDs.Except(SuppliedLineItemItemIds);
            return unSuppliedLineItemIds.ToList();
        }
    }
}