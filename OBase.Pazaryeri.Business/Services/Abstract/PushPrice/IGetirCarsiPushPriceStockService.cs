using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Enums;

namespace OBase.Pazaryeri.Business.Services.Abstract.PushPrice
{
    public interface IGetirCarsiPushPriceStockService
    {
        Task PushPriceStockAsync(Dictionary<string, string> properties, CommonEnums.JobType ExecutionType);
        Task VerifyPriceStockAsync(Dictionary<string, string> properties);
        Task<List<CustomResult>> PushPriceStockOnlyStockAsync(Dictionary<string, string> properties, long refId);
    }
}
