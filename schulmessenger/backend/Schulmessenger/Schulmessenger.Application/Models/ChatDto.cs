using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Models;

public record ChatDto(
    Guid ChatId,
    string Name,
    string? Description,
    ChatType ChatType,
    Guid? LastMessageId,
    string? LastMessageContent,
    string? LastMessageSenderName,
    DateTimeOffset? LastMessageSentAt,
    DateTimeOffset CreatedAt);