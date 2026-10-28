using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Models.Auth;

public record LdapUserResultDto(bool Success, string? FirstName, string? LastName, UserRole? Role, string? ClassName, string? ErrorMessage = null);