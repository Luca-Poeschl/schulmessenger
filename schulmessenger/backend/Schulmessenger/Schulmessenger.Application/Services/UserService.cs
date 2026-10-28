using Microsoft.EntityFrameworkCore;
using Schulmessenger.Application.Interfaces.Db;
using Schulmessenger.Application.Interfaces.Services;
using Schulmessenger.Application.Models;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Services;

public class UserService(ISchulmessengerDbContext db) : IUserService
{
    public async Task<IReadOnlyList<UserDto>> GetUsers(CancellationToken cancellationToken = default)
    {
        return await db.User
            .AsNoTracking()
            .Select(u => new UserDto(u.UserId, u.FirstName, u.LastName, u.Role,
                u.Student != null && u.Student.Class != null ? u.Student.Class.ClassName : string.Empty))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserDto>> GetFilteredUsers(Guid senderId, Guid? excludedChatId,
        CancellationToken cancellationToken = default)
    {
        var userChats = excludedChatId is not null
            ? await db.UserChat
                .AsNoTracking()
                .Where(uc => uc.ChatId == excludedChatId)
                .Select(uc => uc.UserId)
                .ToListAsync(cancellationToken)
            : [];

        return await db.User
            .AsNoTracking()
            .Where(u => u.UserId != senderId && !userChats.Contains(u.UserId))
            .OrderBy(u => u.Student == null)
            .ThenBy(u => u.Student!.Class == null)
            .ThenBy(u => u.Student!.Class!.ClassName)
            .ThenBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new UserDto(u.UserId, u.FirstName, u.LastName, u.Role,
                u.Student != null && u.Student.Class != null ? u.Student.Class.ClassName : string.Empty))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto> GetUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await db.User
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        return user is null
            ? throw new ArgumentException("User nicht gefunden!")
            : new UserDto(user.UserId, user.FirstName, user.LastName, user.Role,
                user.Student != null && user.Student.Class != null ? user.Student.Class.ClassName : string.Empty);
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsers(IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var users = await db.User
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserId))
            .Select(u => new UserDto(u.UserId, u.FirstName, u.LastName, u.Role,
                u.Student != null && u.Student.Class != null ? u.Student.Class.ClassName : string.Empty))
            .ToListAsync(cancellationToken);

        return users;
    }
}