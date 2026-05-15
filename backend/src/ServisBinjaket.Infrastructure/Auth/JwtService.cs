using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServisBinjaket.Application.Interfaces;

namespace ServisBinjaket.Infrastructure.Auth;

public class JwtService : IJwtService
{
    private readonly string _secret;

    public JwtService(IConfiguration config)
    {
        _secret = config["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is not configured");
        if (_secret.Length < 32)
            throw new InvalidOperationException("JWT_SECRET must be at least 32 characters");
    }

    public string GenerateToken(int adminId, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
