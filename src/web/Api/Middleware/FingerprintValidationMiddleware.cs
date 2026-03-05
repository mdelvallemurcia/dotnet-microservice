namespace api.Middleware;

internal class FingerprintValidationMiddleware
{
    private readonly RequestDelegate _next;

    public FingerprintValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        var tokenFingerprint = context.User.FindFirst("fp")?.Value;
        var cookieFingerprint = context.Request.Cookies["fp"];

        if (tokenFingerprint != cookieFingerprint)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid fingerprint");
            return;
        }

        await _next(context);
    }
}
