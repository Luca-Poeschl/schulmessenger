using Microsoft.EntityFrameworkCore;
using Schulmessenger.Application.Interfaces.Db;
using Schulmessenger.Application.Interfaces.Services;
using Schulmessenger.Application.Models;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Services;

public class MessageService(ISchulmessengerDbContext db) : IMessageService
{
    public async Task<IReadOnlyList<Message>> GetMessages(CancellationToken cancellationToken = default)
    {
        var messages = await db.Message.AsNoTracking().ToListAsync(cancellationToken);
        return messages;
    }

    public async Task<IReadOnlyList<MessageDto>> GetMessages(Guid chatId, Guid senderId,
        CancellationToken cancellationToken = default)
    {
        var userInChat =
            await db.UserChat.AnyAsync(uc => uc.ChatId == chatId && uc.UserId == senderId, cancellationToken);

        if (!userInChat)
            throw new UnauthorizedAccessException("User ist nicht Mitglied des Chats!");

        var messageDtos = await db.Message
            .AsNoTracking()
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto(m.ChatId, m.MessageId, m.Content, m.SenderId,
                m.Sender != null && !m.IsDeleted ? $"{m.Sender.FirstName} {m.Sender.LastName}" : "System", m.SentAt,
                m.IsSystemMessage, m.IsDeleted))
            .ToListAsync(cancellationToken);

        return messageDtos;
    }

    public async Task<MessageDto> CreateSystemMessage(Guid chatId, string content,
        CancellationToken cancellationToken = default)
    {
        var message = new Message()
        {
            MessageId = Guid.NewGuid(),
            ChatId = chatId,
            Content = content,
            SentAt = DateTime.UtcNow,
            IsSystemMessage = true
        };
        await db.Message.AddAsync(message, cancellationToken);

        var chat = db.Chat.Local.FirstOrDefault(c => c.ChatId == chatId);
        chat ??= await db.Chat.FirstOrDefaultAsync(c => c.ChatId == chatId, cancellationToken);
        if (chat is null)
            throw new Exception("Chat wurde nicht gefunden!");

        chat.LastMessage = message;

        return new MessageDto(message.ChatId, message.MessageId, message.Content, message.SenderId,
            "System", message.SentAt, message.IsSystemMessage, message.IsDeleted);
    }

    public async Task<MessageDto> DeleteMessage(Guid messageId, Guid senderId,
        CancellationToken cancellationToken = default)
    {
        var message = await db.Message
            .Include(m => m.Chat)
            .Include(m => m.Chat.UserChats)
            .FirstOrDefaultAsync(m => m.MessageId == messageId, cancellationToken);

        if (message is null)
            throw new KeyNotFoundException("Nachricht wurde nicht gefunden!");

        var userChat = message.Chat.UserChats.FirstOrDefault(uc => uc.UserId == senderId);
        if (userChat is null)
            throw new UnauthorizedAccessException("Benutzer ist nicht Mitglied des Chats!");

        if (message.IsSystemMessage)
            throw new InvalidOperationException("Systemnachrichten können nicht gelöscht werden!");

        if (message.IsDeleted)
            throw new InvalidOperationException("Nachricht wurde bereits gelöscht!");

        if (userChat.Chat.ChatType == ChatType.Direct && message.SenderId != senderId)
            throw new UnauthorizedAccessException("In Direktchats darf man nur eigene Nachrichten löschen!");

        if (userChat.Chat.ChatType == ChatType.Groupchat && !userChat.IsAdmin && message.SenderId != senderId)
            throw new UnauthorizedAccessException("Benutzer ist nicht berechtigt, in diesem Chat Nachrichten zu löschen");

        message.IsDeleted = true;
        message.Content = "Diese Nachricht wurde gelöscht.";

        var changedLines = await db.SaveChangesAsync(cancellationToken);

        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Löschen der Nachricht!");

        return new MessageDto(message.ChatId, message.MessageId, message.Content, message.SenderId, null,
            message.SentAt, message.IsSystemMessage, message.IsDeleted);
    }
}