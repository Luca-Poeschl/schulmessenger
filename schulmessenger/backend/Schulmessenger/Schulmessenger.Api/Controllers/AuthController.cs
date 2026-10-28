using Microsoft.AspNetCore.Mvc;
using Schulmessenger.Application.Interfaces.Services.Auth;
using Schulmessenger.Application.Models.Auth;

namespace Schulmessenger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto loginRequestDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(loginRequestDto.Username) || string.IsNullOrEmpty(loginRequestDto.Password))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: "Benutzername und Passwort sind erforderlich!");

        var loginResponse = await authService.Login(loginRequestDto, cancellationToken);

        return Ok(loginResponse);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequestDto refreshRequestDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshRequestDto.RefreshToken))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: "RefreshToken darf nicht leer sein!");


        var result = await authService.RefreshTokens(refreshRequestDto.RefreshToken, cancellationToken);
        if (result is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized,
                detail: "Ungültiger RefreshToken!");

        return Ok(result);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshRequestDto refreshRequestDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshRequestDto.RefreshToken))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: "RefreshToken darf nicht leer sein!");

        var success = await authService.RevokeToken(refreshRequestDto.RefreshToken, cancellationToken);
        if (!success)
            return Problem(statusCode: StatusCodes.Status404NotFound,
                detail: "Token wurde nicht gefunden!");

        return NoContent();
    }
}