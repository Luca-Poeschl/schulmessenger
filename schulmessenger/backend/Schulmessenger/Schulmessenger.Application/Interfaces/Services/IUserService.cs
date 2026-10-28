using Schulmessenger.Application.Models;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Interfaces.Services;

public interface IUserService
{
    public Task<IReadOnlyList<UserDto>> GetUsers(CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<UserDto>> GetFilteredUsers(Guid senderId, Guid? excludedChatId, CancellationToken cancellationToken = default);
    public Task<UserDto> GetUser(Guid userId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<UserDto>> GetUsers(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
}