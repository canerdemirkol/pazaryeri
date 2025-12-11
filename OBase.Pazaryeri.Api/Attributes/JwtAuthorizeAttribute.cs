using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OBase.Pazaryeri.Domain.ConfigurationOptions;
using OBase.Pazaryeri.Domain.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace OBase.Pazaryeri.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,AllowMultiple =false)]
    public class JwtAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public bool ValidateUser { get; set; } = false;

        /// <summary>
        /// Kullanıcının erişebilmesi gereken zorunlu hizmet adı (örneğin, "SaleInfo", "YemekSepeti")
        /// Boş veya null ise, servis kontrolü atlanır
        /// </summary>
        public string RequiredService { get; set; }

        /// <summary>
        /// Kullanıcının en az birine sahip olması gereken zorunlu roller ("Admin", "Manager" gibi)
        /// Kullanıcının belirtilen rollerden en az BİRİNE sahip olması gerekir
        /// </summary>
        public string[] RequiredRoles { get; set; }


        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtAuthorizeAttribute>>();
            var options = context.HttpContext.RequestServices.GetService<IOptions<AppSettings>>();

            if (options?.Value?.AuthDefinitions == null)
            {
                logger?.LogWarning("Yetkilendirme yapılandırılmamış");
                await SetUnauthorizedResponse(context, "Yetkilendirme yapılandırılmamış");
                return;
            }

            var authDefinitions = options.Value.AuthDefinitions;

            if (!authDefinitions.AuthEnabled || authDefinitions.AuthType != "Bearer")
            {
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                logger?.LogWarning("Authorization header bulunamadı");
                await SetUnauthorizedResponse(context, "Authorization header bulunamadı");
                return;
            }

            var token = authHeader.ToString().Split(" ").Last();

            var validationResult = ValidateToken(token, ValidateUser, RequiredService, RequiredRoles, authDefinitions, logger);
            if (!validationResult.IsValid)
            {
                logger?.LogWarning($"Token validasyon hatası: {validationResult.ErrorMessage}");
                await SetUnauthorizedResponse(context, validationResult.ErrorMessage);
                return;
            }
        }

        private ValidationResult ValidateToken(string token, bool validateUser, string requiredService, string[] requiredRoles, AuthDefinitions authDefinitions, ILogger logger)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = authDefinitions.JwtSecret;

                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = validateUser && !string.IsNullOrEmpty(authDefinitions.JwtIssuer),
                    ValidIssuer = authDefinitions.JwtIssuer,
                    ValidateAudience = validateUser && !string.IsNullOrEmpty(authDefinitions.JwtAudience),
                    ValidAudience = authDefinitions.JwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                if (validatedToken == null)
                {
                    return ValidationResult.Fail("Geçersiz token");
                }

                //sadece validate user true ise
                if (validateUser)
                {
                    var usernameClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                    if (usernameClaim == null)
                    {
                        return ValidationResult.Fail("Token içinde kullanıcı adı bulunamadı");
                    }
                
                    if (authDefinitions.JwtUsers == null || !authDefinitions.JwtUsers.Any())
                    {
                        return ValidationResult.Fail("Sistemde tanımlı kullanıcı bulunamadı");
                    }

                    var userExists = authDefinitions.JwtUsers.Any(u => u.Username == usernameClaim.Value);
                    if (!userExists)
                    {
                        return ValidationResult.Fail($"Kullanıcı '{usernameClaim.Value}' yapılandırmada bulunamadı");
                    }
                }

              
                if (!string.IsNullOrEmpty(requiredService))
                {
                    var supplierIdClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "SupplierId");
                    if (supplierIdClaim == null || string.IsNullOrEmpty(supplierIdClaim.Value))
                    {
                        logger?.LogWarning($"Erişim reddedildi: Token içinde SupplierId bulunamadı (Servis: '{requiredService}')");
                        return ValidationResult.Fail("Erişim reddedildi: Servis erişimi için SupplierId gereklidir");
                    }


                    var allowedServiceClaims = claimsPrincipal.Claims
                        .Where(c => c.Type == "AllowedService")
                        .Select(c => c.Value)
                        .ToList();

                    if (!allowedServiceClaims.Any())
                    {
                        return ValidationResult.Fail("Token içinde servis yetkileri bulunamadı");
                    }

                    // Eğer "*" varsa, tüm servislere erişim izni var demektir
                    if (allowedServiceClaims.Contains("*", StringComparer.OrdinalIgnoreCase))
                    {
                        logger?.LogInformation($"Kullanıcı tüm servislere erişim iznine sahip (*), '{requiredService}' servisine erişim sağlandı");
                    }
                    else if (!allowedServiceClaims.Contains(requiredService, StringComparer.OrdinalIgnoreCase))
                    {
                        var supplierId = supplierIdClaim?.Value ?? "Unknown";
                        logger?.LogWarning($"Erişim reddedildi: Kullanıcının '{requiredService}' servisine erişimi yok. SupplierId: {supplierId}, İzinli servisler: {string.Join(", ", allowedServiceClaims)}");
                        return ValidationResult.Fail("Erişim reddedildi: Bu servise erişim yetkiniz bulunmamaktadır");
                    }
                }

                if (requiredRoles != null && requiredRoles.Any())
                {
                    var userRoleClaims = claimsPrincipal.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToList();

                    if (!userRoleClaims.Any())
                    {
                        return ValidationResult.Fail("Token içinde rol bilgisi bulunamadı");
                    }

                    // Eğer kullanıcı Admin rolüne sahipse veya "*" rolü varsa, tüm kaynaklara erişim izni var
                    if (userRoleClaims.Contains("Admin", StringComparer.OrdinalIgnoreCase) ||
                        userRoleClaims.Contains("*", StringComparer.OrdinalIgnoreCase))
                    {
                        logger?.LogInformation("Kullanıcı Admin rolüne veya tüm rollere (*) sahip, erişim sağlandı");
                    }
                    else
                    {
                        var hasRequiredRole = requiredRoles.Any(requiredRole =>
                            userRoleClaims.Contains(requiredRole, StringComparer.OrdinalIgnoreCase));

                        if (!hasRequiredRole)
                        {
                            var usernameClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                            var username = usernameClaim?.Value ?? "Unknown";
                            logger?.LogWarning($"Erişim reddedildi: Kullanıcı '{username}' gerekli role sahip değil. Gerekli roller: {string.Join(", ", requiredRoles)}, Kullanıcı rolleri: {string.Join(", ", userRoleClaims)}");
                            return ValidationResult.Fail("Erişim reddedildi: Bu kaynağa erişim için gerekli role sahip değilsiniz");
                        }
                    }
                }

                return ValidationResult.Success();
            }
            catch (SecurityTokenExpiredException ex)
            {
                logger?.LogWarning($"Token doğrulama başarısız: token süresi dolmuş - {ex.Message}");
                return ValidationResult.Fail("Token süresi dolmuş");
            }
            catch (SecurityTokenException ex)
            {
                logger?.LogWarning($"Token doğrulama başarısız: {ex.Message}");
                return ValidationResult.Fail("Geçersiz token");
            }
            catch (Exception ex)
            {
                logger?.LogWarning($"Token doğrulama başarısız: {ex.Message}");
                return ValidationResult.Fail("Token doğrulama hatası");
            }
        }


        private Task SetUnauthorizedResponse(AuthorizationFilterContext context, string message)
        {
            var result = ServiceResponse<object>.Error(message, HttpStatusCode.Unauthorized);

            context.Result = new JsonResult(result)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

            return Task.CompletedTask;
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }

            public static ValidationResult Success() => new ValidationResult { IsValid = true };
            public static ValidationResult Fail(string message) => new ValidationResult { IsValid = false, ErrorMessage = message };
        }
    }
}
