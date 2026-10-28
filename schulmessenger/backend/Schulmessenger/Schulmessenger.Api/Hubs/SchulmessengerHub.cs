using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Schulmessenger.Api.Extensions;
using Schulmessenger.Application.Interfaces.Db;
using Schulmessenger.Application.Models;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Api.Hubs;

[Authorize]
public class SchulmessengerHub(ISchulmessengerDbContext db) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.GetUserId();

        var chatIds = await db.User
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.UserChats.Select(c => c.ChatId))
            .ToListAsync();

        foreach (var chatId in chatIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        await base.OnConnectedAsync();
    }

    public async Task
        SendMessage(Guid chatId, string content,
            string tempId)
    {
        var senderId = Context.User!.GetUserId();
        var isMember = await db.UserChat
            .AnyAsync(uc => uc.UserId == senderId && uc.ChatId == chatId);

        var sender = await db.User
            .AsNoTracking()
            .Where(u => u.UserId == senderId)
            .FirstOrDefaultAsync(); // Später dann per Auth

        if (sender is null)
            throw new HubException("User nicht gefunden!");

        if (!isMember)
            throw new HubException("Nicht Mitglied des Chats!");

        var trimmedContent = content.Trim();
        if (string.IsNullOrEmpty(trimmedContent))
            throw new HubException("Die Eingabe darf nicht leer sein!");

        if (trimmedContent.Length > 4000)
            throw new HubException("Die Eingabe darf maximal 4000 Zeichen betragen!");

        var newMessage = new Message()
        {
            ChatId = chatId,
            SenderId = senderId,
            Content = trimmedContent,
            SentAt = DateTimeOffset.UtcNow
        };

        var chat = await db.Chat.FindAsync(chatId);
        if (chat is null)
            throw new HubException("Chats nicht gefunden!");
        
        chat.LastMessage = newMessage;

        await db.Message.AddAsync(newMessage);
        var changedLines = await db.SaveChangesAsync();

        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Speichern der Nachricht!");

        await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage",
            new MessageSendDto(newMessage.ChatId, newMessage.MessageId, newMessage.Content,
                sender.UserId, $"{sender.FirstName} {sender.LastName}", newMessage.SentAt,
                newMessage.IsSystemMessage, newMessage.IsDeleted, tempId));
    }

    public async Task JoinChat(Guid chatId)
    {
        var senderId = Context.User!.GetUserId();
        var isMember = await db.UserChat
            .AnyAsync(uc => uc.ChatId == chatId && uc.UserId == senderId);

        if (!isMember)
            throw new HubException("User ist nicht Mitglied der Gruppe!");

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }
    
    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    }
}