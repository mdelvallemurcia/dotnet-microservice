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
        // 1. Definir la clave secreta (debe venir de UserSecrets o KeyVault)
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 2. Definir los Claims (información dentro del token)
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(ClaimTypes.Role, "Reader")
        };

        // 3. Configuración del Token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(options.ExpirationInMinutes),
            Issuer = options.Issuer,
            Audience = options.Audience,
            SigningCredentials = creds
        };

        // 4. Crear el Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}

public interface IBearerTokenGenerator
{
       string CreateToken(string userId);
}