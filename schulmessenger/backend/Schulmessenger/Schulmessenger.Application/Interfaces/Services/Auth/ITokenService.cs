using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Interfaces.Services.Auth;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}