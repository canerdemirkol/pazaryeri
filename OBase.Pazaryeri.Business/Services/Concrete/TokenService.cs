using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OBase.Pazaryeri.Business.Services.Abstract;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos.CommonDtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OBase.Pazaryeri.Business.Services.Concrete
{
    public class TokenService : ITokenService
    {
        private readonly IOptions<AppSettings> _options;
        private readonly AuthDefinitions _authDefinitions;
        public TokenService(IOptions<AppSettings> options)
        {
            _options = options;
            _authDefinitions = _options.Value.AuthDefinitions;
        }

        public TokenDto Authenticate(LoginRequestDto loginRequest)
        {
            if (_authDefinitions?.JwtUsers == null || !_authDefinitions.JwtUsers.Any())
            {
                throw new UnauthorizedAccessException("Sistemde yapılandırılmış kullanıcı yok");
            }

            var user = _authDefinitions.JwtUsers.FirstOrDefault(u =>
                u.Username == loginRequest.Username &&
                u.Password == loginRequest.Password);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Kullanıcı adı veya şifre hatalı.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_authDefinitions.JwtSecret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add SupplierId if configured
            if (!string.IsNullOrEmpty(user.SupplierId))
            {
                claims.Add(new Claim("SupplierId", user.SupplierId));
            }

            // Add Roles
            if (user.Roles != null && user.Roles.Any())
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // Add AllowedServices
            if (user.AllowedServices != null && user.AllowedServices.Any())
            {
                foreach (var service in user.AllowedServices)
                {
                    claims.Add(new Claim("AllowedService", service));
                }
            }

            var expiresAt = user.JwtExpiresInDay.HasValue
                ? DateTime.UtcNow.AddDays(user.JwtExpiresInDay.Value)
                : DateTime.UtcNow.AddDays(1);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _authDefinitions.JwtIssuer,
                Audience = _authDefinitions.JwtAudience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new TokenDto
            {
                Token = tokenString,
                Username = user.Username,
                SupplierId = user.SupplierId,
                Roles = user.Roles,
                AllowedServices = user.AllowedServices,
                ExpiresAt = expiresAt
            };
        }

        public string GenerateToken(string customerName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_authDefinitions.JwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, customerName)
            }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = customerName,
                Audience = customerName
            };

            var expiresAt = _authDefinitions.JwtExpiresInDay.HasValue
              ? DateTime.UtcNow.AddDays(_authDefinitions.JwtExpiresInDay.Value)
              : DateTime.UtcNow.AddDays(1);


            tokenDescriptor.Expires = expiresAt;
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}