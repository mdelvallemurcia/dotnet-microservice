using Microsoft.AspNetCore.Http;

namespace Api.Features.Services.Auth;

public interface IAuthFacade
{
    string GenerateAccessToken(string userId, string fingerprintHash);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiresAt();
    string GenerateHash(string token);
    string GenerateFingerprint();
    void AddSecureCookie(HttpContext context, string key, string value);
    void AddSecureCookie(HttpContext context, CookieKeyEnum keyEnum, string value);
    string? GetSecureCookie(HttpContext context, string key);
    string? GetSecureCookie(HttpContext context, CookieKeyEnum keyEnum);
    void RemoveSecureCookie(HttpContext context, string key);
    void RemoveSecureCookie(HttpContext context, CookieKeyEnum keyEnum);
}
