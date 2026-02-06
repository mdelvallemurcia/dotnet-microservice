namespace Api.Features.Shared.Auth;

public class BearerTokenOptions
{
    public const string Section = "BearerToken";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; }
}
