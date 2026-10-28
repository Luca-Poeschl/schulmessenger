using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Models.Auth;

public record LoginResponseDto(string AccessToken, string RefreshToken, Guid UserId, string FirstName, string LastName, UserRole Role, string? ClassName);