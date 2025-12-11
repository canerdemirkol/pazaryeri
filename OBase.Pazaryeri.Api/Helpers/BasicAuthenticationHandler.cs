#region General
using System.Text;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
#endregion

#region Project
using OBase.Pazaryeri.Domain.ConfigurationOptions;
#endregion

namespace OBase.Pazaryeri.Api.Helpers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        #region Private
        private readonly IOptions<AppSettings> _appSettings;
        #endregion

        #region Const
        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IOptions<AppSettings> appSettings)
         : base(options, logger, encoder)
        {
            _appSettings = appSettings;
        }
        #endregion

        #region Metot
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            bool isSuccess = false;

            if (Request.Path.ToString().ToLower().Contains("serverjobs"))
                isSuccess = true;

            if (!isSuccess && !Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

            if(!isSuccess)
            {
                try
                {
                    var authHeader = Request.Headers["Authorization"].ToString();

                    if (authHeader != null && authHeader.Contains("Basic"))
                    {
                        var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);
                        var credentialBytes = Convert.FromBase64String(authHeaderVal.Parameter ?? "");
                        var credentials = Encoding.UTF8.GetString(credentialBytes);
                        int seperator = credentials.IndexOf(":");
                        var username = credentials.Substring(0, seperator);
                        var password = credentials.Substring(seperator + 1);

                        if (username == _appSettings.Value.ApiUser.Username && password == _appSettings.Value.ApiUser.Password)
                            isSuccess = true;

                    }
                    else
                        return Task.FromResult(AuthenticateResult.Fail("BasicAuthenticationHandler > Authentication failed, missing Authorization Header"));
                }
                catch
                {
                    return Task.FromResult(AuthenticateResult.Fail("BasicAuthenticationHanler > Error occured. Authorization failed."));
                }
            }

            if (!isSuccess)
                return Task.FromResult(AuthenticateResult.Fail("BasicAuthenticationHandler > Authentication failed. Invalid Username or Password"));

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, ""),
                new Claim(ClaimTypes.Name, ""),
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        #endregion

    }
}