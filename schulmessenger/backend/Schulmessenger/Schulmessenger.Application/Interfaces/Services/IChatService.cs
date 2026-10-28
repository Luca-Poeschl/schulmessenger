using Schulmessenger.Application.Models;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Interfaces.Services;

public interface IChatService
{
    public Task<IReadOnlyList<Chat>> GetChats(CancellationToken cancellationToken = default);
    public Task<ChatDto> GetChat(Guid chatId, Guid senderId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<ChatDto>> GetChats(Guid userId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<UserOnChatInfoDto>> GetChatUsers(Guid chatId, Guid senderId, CancellationToken cancellationToken = default);
    public Task<ChatDto?> CreateChat(ChatCreateDto chatCreateDto, Guid creatorId, CancellationToken cancellationToken = default);
    public Task<ChatLeaveResultDto> LeaveChat(Guid chatId, Guid senderId, CancellationToken cancellationToken = default);
    public Task<MessageDto> RemoveUserFromChat(Guid chatId, Guid userId, Guid senderId, CancellationToken cancellationToken = default);
    public Task DeleteChat(Guid chatId, Guid senderId, CancellationToken cancellationToken = default);
    public Task<MessageDto> PromoteUser(Guid chatId, Guid userId, Guid senderId, CancellationToken cancellationToken = default);
    public Task<MessageDto> DemoteUser(Guid chatId, Guid userId, Guid senderId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyCollection<ChatUsersAddResponseDto>> AddUsersToChat(ChatUsersAddDto chatUsersAddDto, Guid chatId, Guid senderId, CancellationToken cancellationToken = default);
}