using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace OBase.Pazaryeri.Api.Helpers;

/// <summary>
/// 
/// </summary>
public static class SecurityKeyHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="securityKey"></param>
    /// <returns></returns>
    public static SecurityKey CreateSecurityKey(string securityKey)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
    }
}

