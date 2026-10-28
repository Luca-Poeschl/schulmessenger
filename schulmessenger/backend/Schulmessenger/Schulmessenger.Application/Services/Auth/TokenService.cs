using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Schulmessenger.Application.Interfaces.Services.Auth;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Services.Auth;

public class TokenService : ITokenService
{
    private readonly string _secret = Environment.GetEnvironmentVariable("JWT_SECRET") ??
                                      throw new InvalidOperationException("JWT_SECRET ist nicht gesetzt!");

    public string GenerateAccessToken(User user)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Student?.Class?.ClassName))
        {
            claims.Add(new Claim("ClassName", user.Student.Class.ClassName ?? string.Empty));
        }

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}