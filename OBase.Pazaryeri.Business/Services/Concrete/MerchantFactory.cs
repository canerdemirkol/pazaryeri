#region General
#endregion

#region Project
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Business.Services.Abstract.Order;
using static OBase.Pazaryeri.Domain.Constants.Constants;
#endregion

namespace OBase.Pazaryeri.Business.Services.Concrete
{
    public class MerchantFactory : IMerchantFactory
    {
        #region Private
        private readonly IGetirCarsiOrderService _getirOrderService;
        private readonly ITrendyolGoOrderService _trendyolGoOrderService;
        private readonly IYemekSepetiOrderService _yemekSepetiOrderService;
        private readonly IIdefixOrderService _idefixOrderService;
        #endregion

        #region Const
        public MerchantFactory(IGetirCarsiOrderService getirOrderService, ITrendyolGoOrderService trendyolGoOrderService, IYemekSepetiOrderService yemekSepetiOrderService, IIdefixOrderService idefixOrderService)
        {
            _getirOrderService = getirOrderService;
            _trendyolGoOrderService = trendyolGoOrderService;
            _yemekSepetiOrderService = yemekSepetiOrderService;
            _idefixOrderService = idefixOrderService;
        }

        #endregion

        #region Metot
        public IOrderService GetMerchantOrderService(string merchantNo)
        {
            switch (merchantNo)
            {
                case PazarYeri.GetirCarsi:
                    return _getirOrderService;
                case PazarYeri.TrendyolGo:
                    return _trendyolGoOrderService;
                case PazarYeri.Yemeksepeti:
                    return _yemekSepetiOrderService;
                case PazarYeri.Idefix:
                    return _idefixOrderService;
                default:
                    throw new KeyNotFoundException($"Merchant With {merchantNo} ID, Not Found");
            }
        }
        #endregion

    }
}