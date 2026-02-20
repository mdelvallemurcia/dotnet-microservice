using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;

namespace Api.Features.Shared.Auth;

public class BearerTokenGenerator : IBearerTokenGenerator
{
    private readonly IOptionsMonitor<BearerTokenOptions> _optionsMonitor;

    public BearerTokenGenerator(IOptionsMonitor<BearerTokenOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public string CreateToken(string userId)
    {
        var options = _optionsMonitor.CurrentValue;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(ClaimTypes.Role, "Reader")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(options.ExpirationInMinutes),
            Issuer = options.Issuer,
            Audience = options.Audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}

public interface IBearerTokenGenerator
{
       string CreateToken(string userId);
}