namespace Api.Features.Shared.Auth;

public class BearerTokenOptions
{
    public const string Section = "BearerToken";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; }

    // Refresh-token lifetime in days. Unifies login/refresh (previously 1h vs 1d). Default 7.
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}
