using Microsoft.AspNetCore.Mvc;
using OBase.Pazaryeri.Api.Attributes;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Domain.Dtos;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using System.Net;

namespace OBase.Pazaryeri.Api.Controllers
{
    [Route("api/v1/tokens")]
    [ApiController]
    [SwaggerEndpointGroup("Token")]
    public class TokenController : BaseController
    {
        #region Private
        private readonly ITokenService _tokenService;

        #endregion

        #region Const
        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }
        #endregion
        #region Methods 

        [Route("login")]
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
                {
                    return BadRequest(ServiceResponse<LoginResponseDto>.Error("Kullanıcı adı ve şifre gereklidir.", HttpStatusCode.BadRequest));
                }

                var response = _tokenService.Authenticate(loginRequest);
                return Ok(ServiceResponse<LoginResponseDto>.Success(new LoginResponseDto() { ExpiresAt=response.ExpiresAt,SupplierId=response.SupplierId,Token=response.Token,Username=response.Username}));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ServiceResponse<LoginResponseDto>.Error(ex.Message, HttpStatusCode.Unauthorized));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ServiceResponse<LoginResponseDto>.Error($"An error occurred: {ex.Message}", HttpStatusCode.InternalServerError));
            }
        }
        #endregion

    }
}