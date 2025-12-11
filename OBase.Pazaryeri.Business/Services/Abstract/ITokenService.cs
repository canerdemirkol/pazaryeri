using OBase.Pazaryeri.Domain.Dtos.CommonDtos;

namespace OBase.Pazaryeri.Business.Services.Abstract
{
    public interface ITokenService
    {
        string GenerateToken(string customerName);
        TokenDto Authenticate(LoginRequestDto loginRequest);
    }
}
