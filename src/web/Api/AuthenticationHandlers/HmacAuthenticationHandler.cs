using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace api.AuthenticationHandlers;

internal class HmacAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    // Single source of truth for the scheme name: used to register the scheme, to route by
    // Authorization prefix, and to exempt HMAC callers from fingerprint validation.
    public const string SchemeName = "HMAC";

    private readonly TimeProvider _timeProvider;

    public HmacAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TimeProvider timeProvider)
        : base(options, logger, encoder)
    {
        _timeProvider = timeProvider;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();

        if (!authHeader.StartsWith("Ldx ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.NoResult());

        var token = authHeader.Substring("Ldx ".Length);

        if (!ValidateHmac(token))
            return Task.FromResult(AuthenticateResult.Fail("Invalid HMAC"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "ldx-client"),
            new Claim("client_id", "123")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool ValidateHmac(string token)
    {
        var now = _timeProvider.GetUtcNow();

        // Token -> AppId:hash:timestamp

        // puedes usar now para validar timestamps en la firma HMAC

        return true;
    }
}
