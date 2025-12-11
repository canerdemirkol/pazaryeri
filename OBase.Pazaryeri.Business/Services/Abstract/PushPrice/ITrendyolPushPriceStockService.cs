using OBase.Pazaryeri.Domain.Dtos;
using static OBase.Pazaryeri.Domain.Enums.CommonEnums;

namespace OBase.Pazaryeri.Business.Services.Abstract.PushPrice
{
    public interface ITrendyolPushPriceStockService
    {
        Task PushPriceStockAsync(Dictionary<string, string> properties, JobType executionType);
        Task VerifyPriceStockAsync(Dictionary<string, string> properties);
        Task<List<CustomResult>> PushPriceStockOnlyStockAsync(Dictionary<string, string> properties, long refId);
    }
}
