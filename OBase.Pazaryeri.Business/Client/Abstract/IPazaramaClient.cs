using OBase.Pazaryeri.Domain.Dtos.Pazarama;
using OBase.Pazaryeri.Domain.Dtos.Pazarama.PushPrice;
using RestEase;

namespace OBase.Pazaryeri.Business.Client.Abstract
{
    public interface IPazaramaClient : IDisposable
    {
        #region General
        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/connect/token")]
        Task<Response<PazaramaResponse<GetPazaramaTokenResponseDto.Data>>> GetToken([Header("Authorization")] string authorization, [Header("Accept")] string accept, [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> data);
        #endregion

        #region Price / Stock

        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/product/updatePrice-v2")]
        Task<Response<PazaramaResponse<string>>> ProductPriceUpdate([Body] PostPazaramaProductStockAndPriceUpdateRequestDto.Root dto);


        [Header("User-Agent", "Mozilla /5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0")]
        [Header("Cache-Control", "no-cache")]
        [AllowAnyStatusCode]
        [Post("/product/updateStock-v2")]
        Task<Response<PazaramaResponse<string>>> ProductStockUpdate([Body] PostPazaramaProductStockAndPriceUpdateRequestDto.Root dto);


        #endregion
    }
}
