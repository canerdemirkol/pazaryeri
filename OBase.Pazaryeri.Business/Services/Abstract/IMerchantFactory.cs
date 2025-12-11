namespace OBase.Pazaryeri.Business.Services.Abstract
{
    public interface IMerchantFactory
    {
        IOrderService GetMerchantOrderService(string merchantNo);
    }
}
