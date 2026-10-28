using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Models;

public record UserDto(Guid UserId, string FirstName, string LastName, UserRole Role, string? ClassName);