using Microsoft.AspNetCore.Http;

namespace Api.Features.Services.Auth;

public interface IAuthFacade
{
    string GenerateAccessToken(string userId, string fingerprint);
    string GenerateRefreshToken(string userId);
    string GenerateHash(string token);
    string GenerateFingerprint(HttpContext context);
    void AddSecureCookie(HttpContext context, string key, string value);
    void AddSecureCookie(HttpContext context, CookieKeyEnum keyEnum, string value);
    string? GetSecureCookie(HttpContext context, string key);
    string? GetSecureCookie(HttpContext context, CookieKeyEnum keyEnum);
    void RemoveSecureCookie(HttpContext context, string key);
    void RemoveSecureCookie(HttpContext context, CookieKeyEnum keyEnum);
}
