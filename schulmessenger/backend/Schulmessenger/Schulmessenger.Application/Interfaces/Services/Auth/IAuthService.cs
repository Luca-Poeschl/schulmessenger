using Schulmessenger.Application.Models.Auth;

namespace Schulmessenger.Application.Interfaces.Services.Auth;

public interface IAuthService
{
    Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto, CancellationToken cancellationToken = default);
    Task<TokenResultDto?> RefreshTokens(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> RevokeToken(string refreshToken, CancellationToken cancellationToken = default);
}