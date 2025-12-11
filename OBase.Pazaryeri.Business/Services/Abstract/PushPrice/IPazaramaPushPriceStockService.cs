using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Enums;

namespace OBase.Pazaryeri.Business.Services.Abstract.PushPrice
{
    public interface IPazaramaPushPriceStockService
    {
        Task PushPriceStockAsync(Dictionary<string, string> properties, CommonEnums.JobType ExecutionType);
        Task<List<CustomResult>> PushPriceStockOnlyStockAsync(Dictionary<string, string> properties, long refId);
    }
}
