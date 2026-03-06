using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Features.Shared.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Features.Services.Auth;

public class AuthFacade : IAuthFacade
{
    private readonly IOptionsMonitor<BearerTokenOptions> _optionsMonitor;

    public AuthFacade(IOptionsMonitor<BearerTokenOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public string GenerateAccessToken(string userId, string fingerprint)
    {
        var options = _optionsMonitor.CurrentValue;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(ClaimTypes.Role, "reader"),
            new(ClaimTypes.Role, "writer"),
            new("fp", fingerprint),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(options.ExpirationInMinutes),
            Issuer = options.Issuer,
            Audience = options.Audience,
            SigningCredentials = creds,
            NotBefore = DateTime.UtcNow
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken(string userId)
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public string GenerateHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    public string GenerateFingerprint(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var ip = context.Connection.RemoteIpAddress?.ToString();

        var raw = $"{userAgent}:{ip}";

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));

        return Convert.ToBase64String(hash);
    }

    public void AddSecureCookie(HttpContext context, CookieKeyEnum keyEnum, string value)
    {
        var key = keyEnum switch
        {
            CookieKeyEnum.Fingerprint => "fp",
            CookieKeyEnum.RefreshToken => "refresh_token",
            _ => nameof(keyEnum)
        };
        AddSecureCookie(context, key, value);
    }

    public void AddSecureCookie(HttpContext context, string key, string value)
    {
        context.Response.Cookies.Append(
            key,
            value,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
        //TODO different configuration for localhost! -------------------------
        //context.Response.Cookies.Append(
        //    key,
        //    value,
        //    new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Secure = true,
        //        SameSite = SameSiteMode.None
        //    });
    }

}
