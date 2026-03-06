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
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var tokenFingerprint = context.User.FindFirst("fp")?.Value;
        var cookieFingerprint = context.Request.Cookies["fp"];

        if (string.IsNullOrEmpty(tokenFingerprint) || tokenFingerprint != cookieFingerprint)
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
