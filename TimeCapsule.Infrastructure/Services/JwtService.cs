using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Entities;

namespace TimeCapsule.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var secret = _config["JWT_SECRET"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // Add roles using standard ClaimTypes.Role
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            // Also keep "roles" for backward compatibility or client-side use
            claims.Add(new Claim("roles", role));
        }

        var token = new JwtSecurityToken(
            issuer: "timecapsule-auth",
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(
                int.Parse(_config["JWT_ACCESS_TTL"] ?? "900")),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLower();
    }
}
