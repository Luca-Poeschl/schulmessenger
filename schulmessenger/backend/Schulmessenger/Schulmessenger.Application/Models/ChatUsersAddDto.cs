namespace Schulmessenger.Application.Models;

public record ChatUsersAddDto(IReadOnlyCollection<Guid> UserIds);