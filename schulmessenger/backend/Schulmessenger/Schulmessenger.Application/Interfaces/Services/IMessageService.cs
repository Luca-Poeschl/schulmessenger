using Schulmessenger.Application.Models;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Interfaces.Services;

public interface IMessageService
{
    public Task<IReadOnlyList<Message>> GetMessages(CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<MessageDto>> GetMessages(Guid chatId, Guid senderId, CancellationToken cancellationToken = default);
    public Task<MessageDto> CreateSystemMessage(Guid chatId, string content, CancellationToken cancellationToken = default);
    public Task<MessageDto> DeleteMessage(Guid messageId, Guid senderId, CancellationToken cancellationToken = default);
}