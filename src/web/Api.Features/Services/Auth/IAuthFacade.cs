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
}
