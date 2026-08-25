using System.IdentityModel.Tokens.Jwt;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServisBinjaket.Application.Interfaces;

namespace ServisBinjaket.Infrastructure.Auth;

public class JwtService : IJwtService
{
    public const string IssuedAtTicksClaim = "sb_iat_ticks";

    private readonly string _secret;

    public JwtService(IConfiguration config)
    {
        _secret = config["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is not configured");
        if (Encoding.UTF8.GetByteCount(_secret) < 32)
            throw new InvalidOperationException("JWT_SECRET must be at least 32 bytes");
    }

    public string GenerateToken(int adminId, string email)
    {
        var now = DateTime.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(now).ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            new Claim(
                IssuedAtTicksClaim,
                now.Ticks.ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: now,
            expires: now.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
