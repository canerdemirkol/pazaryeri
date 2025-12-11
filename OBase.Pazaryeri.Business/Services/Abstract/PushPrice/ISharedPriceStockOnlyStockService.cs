namespace OBase.Pazaryeri.Business.Services.Abstract.PushPrice
{
    public interface ISharedPriceStockOnlyStockService
    {
        Task SendPushPriceStockOnlyStocks(Dictionary<string, string> properties);
    }
}
