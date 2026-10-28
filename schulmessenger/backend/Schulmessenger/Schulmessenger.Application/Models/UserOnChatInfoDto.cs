using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Models;

public record UserOnChatInfoDto(Guid UserId, string FirstName, string LastName, UserRole Role, bool IsAdmin, string? ClassName);