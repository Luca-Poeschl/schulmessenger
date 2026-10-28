namespace Schulmessenger.Application.Models;

public record MessageSendDto(
    Guid ChatId,
    Guid MessageId,
    string Content,
    Guid? SenderId,
    string? SenderName,
    DateTimeOffset SentAt,
    bool IsSystemMessage,
    bool IsDeleted,
    string TempId);