using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.YemekSepeti;

namespace OBase.Pazaryeri.Business.Services.Abstract.PushPrice
{
    public interface IYemekSepetiPushPriceStockService
    {
        ServiceResponse<string> SendVerifyUrl(YemekSepetiVerifyRequestDto request);
        Task PushPriceStockAsync(Dictionary<string, string> properties);
        Task<List<CustomResult>> PushPriceStockOnlyStockAsync(Dictionary<string, string> properties, long refId);
        Task VerifyPriceStockAsync(Dictionary<string, string> properties);
    }
}
