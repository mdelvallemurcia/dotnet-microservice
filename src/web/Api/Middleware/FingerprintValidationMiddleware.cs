using Api.Features.Services.Auth;
using api.AuthenticationHandlers;

namespace api.Middleware;

internal class FingerprintValidationMiddleware
{
    private readonly RequestDelegate _next;

    public FingerprintValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IAuthFacade authFacade)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Fingerprint binding is a browser/JWT concern. Service-to-service callers authenticated via
        // HMAC carry no cookie, so enforcing it would wrongly 401 them.
        if (context.User.Identity.AuthenticationType == HmacAuthenticationHandler.SchemeName)
        {
            await _next(context);
            return;
        }

        // The JWT carries the fingerprint HASH; the client holds the RAW value in the 'fp' cookie.
        // Possession of the cookie that hashes to the token's claim proves the token isn't sidejacked.
        var tokenFingerprint = context.User.FindFirst("fp")?.Value;
        var cookieFingerprint = context.Request.Cookies["fp"];

        if (string.IsNullOrEmpty(tokenFingerprint)
            || string.IsNullOrEmpty(cookieFingerprint)
            || tokenFingerprint != authFacade.GenerateHash(cookieFingerprint))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Invalid fingerprint",
                status = StatusCodes.Status401Unauthorized
            });
            return;
        }

        await _next(context);
    }
}
