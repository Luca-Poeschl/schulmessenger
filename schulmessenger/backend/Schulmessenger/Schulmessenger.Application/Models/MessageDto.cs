namespace Schulmessenger.Application.Models;

public record MessageDto(Guid ChatId, Guid MessageId, string Content, Guid? SenderId, string? SenderName, DateTimeOffset SentAt, bool IsSystemMessage, bool isDeleted);