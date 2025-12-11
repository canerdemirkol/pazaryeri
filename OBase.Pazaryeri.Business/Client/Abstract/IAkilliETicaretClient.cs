using OBase.Pazaryeri.Domain.Dtos.AkilliETicaret;
using RestEase;

namespace OBase.Pazaryeri.Business.Client.Abstract
{
    public interface IAkilliETicaretClient
    {
        #region Authentication
        /// <summary>
        /// Admin SSO Login for Client's
        /// </summary>
        [Header("Cache-Control", "no-cache")]
        [Header("accept", "application/json")]
        [AllowAnyStatusCode]
        [Post("/ad/auth/login")]
        Task<Response<AkilliETicaretResponse<AkilliETicaretLoginResponseDto>>> LoginAsync([Body] AkilliETicaretLoginRequestDto requestDto);

        /// <summary>
        /// Refresh Token For Admin
        /// </summary>
        [Header("Cache-Control", "no-cache")]
        [Header("accept", "application/json")]
        [AllowAnyStatusCode]
        [Post("/ad/auth/refresh_login")]
        Task<Response<AkilliETicaretResponse<AkilliETicaretLoginResponseDto>>> RefreshLoginAsync([Body] AkilliETicaretRefreshLoginRequestDto requestDto);
        #endregion

        #region Product
        /// <summary>
        /// Batch Product Upsert
        /// </summary>
        [Header("Cache-Control", "no-cache")]
        [Header("accept", "application/json")]
        [AllowAnyStatusCode]
        [Post("/ad/product/batch")]
        Task<Response<AkilliETicaretResponse<AkilliETicaretBatchProductResponseDto>>> BatchProductAsync([Body] List<AkilliETicaretProductMasterUpsertDto> products);
        #endregion
    }
}
