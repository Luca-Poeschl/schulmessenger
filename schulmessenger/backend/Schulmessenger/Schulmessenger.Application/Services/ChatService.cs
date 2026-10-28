using Microsoft.EntityFrameworkCore;
using Schulmessenger.Application.Interfaces.Db;
using Schulmessenger.Application.Interfaces.Services;
using Schulmessenger.Application.Models;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Application.Services;

public class ChatService(ISchulmessengerDbContext db, IMessageService messageService, IUserService userService)
    : IChatService
{
    public async Task<IReadOnlyList<Chat>> GetChats(CancellationToken cancellationToken = default)
    {
        var chats = await db.Chat.AsNoTracking().ToListAsync(cancellationToken);
        return chats;
    }

    public async Task<ChatDto> GetChat(Guid chatId, Guid senderId, CancellationToken cancellationToken = default)
    {
        var chat = await db.Chat
            .AsNoTracking()
            .Where(c => c.ChatId == chatId)
            .Select(c => new ChatDto(c.ChatId,
                c.ChatType == ChatType.Groupchat
                    ? c.Name! // Groupchats haben immer einen Namen!
                    : c.UserChats
                        .Where(uc => uc.UserId != senderId)
                        .Select(uc => $"{uc.User.FirstName} {uc.User.LastName}").FirstOrDefault() ?? "Direktchat",
                c.Description,
                c.ChatType,
                c.LastMessage != null ? c.LastMessage.MessageId : null,
                c.LastMessage != null ? c.LastMessage.Content : null,
                c.LastMessage != null && !c.LastMessage.IsDeleted
                    ? c.LastMessage.Sender != null
                        ? $"{c.LastMessage.Sender.FirstName} {c.LastMessage.Sender.LastName}"
                        : "System"
                    : null,
                c.LastMessage != null ? c.LastMessage.SentAt : null,
                c.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
        return chat ?? throw new ArgumentException("Chat nicht gefunden!");
    }

    public async Task<IReadOnlyList<ChatDto>> GetChats(Guid userId, CancellationToken cancellationToken = default)
    {
        var chatDtos = await db.Chat
            .AsNoTracking()
            .Where(c => c.UserChats.Any(u => u.UserId == userId))
            .OrderByDescending(c => c.LastMessage != null ? c.LastMessage.SentAt : DateTimeOffset.MinValue)
            .Select(c => new ChatDto(c.ChatId,
                c.ChatType == ChatType.Groupchat
                    ? c.Name! // Groupchats haben immer einen Namen!
                    : c.UserChats
                        .Where(uc => uc.UserId != userId)
                        .Select(uc => $"{uc.User.FirstName} {uc.User.LastName}").FirstOrDefault() ?? "Direktchat",
                c.Description,
                c.ChatType,
                c.LastMessage != null ? c.LastMessage.MessageId : null,
                c.LastMessage != null ? c.LastMessage.Content : null,
                c.LastMessage != null && !c.LastMessage.IsDeleted
                    ? c.LastMessage.Sender != null
                        ? $"{c.LastMessage.Sender.FirstName} {c.LastMessage.Sender.LastName}"
                        : "System"
                    : null,
                c.LastMessage != null ? c.LastMessage.SentAt : null,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return chatDtos;
    }

    public async Task<IReadOnlyCollection<UserOnChatInfoDto>> GetChatUsers(Guid chatId, Guid senderId,
        CancellationToken cancellationToken = default)
    {
        var chatUsersDtos = await db.UserChat
            .AsNoTracking()
            .Where(uc => uc.ChatId == chatId && uc.Chat.UserChats.Any(uci => uci.UserId == senderId))
            .OrderByDescending(uc => uc.UserId == senderId)
            .ThenByDescending(uc => uc.IsAdmin)
            .ThenBy(uc => uc.User.Student == null)
            .ThenBy(uc => uc.User.Student!.Class == null)
            .ThenBy(uc => uc.User.Student!.Class!.ClassName)
            .ThenBy(uc => uc.User.LastName)
            .ThenBy(uc => uc.User.FirstName)
            .Select(uc =>
                new UserOnChatInfoDto(uc.UserId, uc.User.FirstName, uc.User.LastName, uc.User.Role, uc.IsAdmin,
                    uc.User.Student != null && uc.User.Student.Class != null ? uc.User.Student.Class.ClassName : ""))
            .ToListAsync(cancellationToken);

        if (chatUsersDtos.Count == 0)
            throw new ArgumentException("Benutzer/Chat nicht gefunden oder nicht Mitglied des Chats!");

        return chatUsersDtos;
    }

    public async Task<ChatDto?>
        CreateChat(ChatCreateDto chatCreateDto, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var creator = await db.User
            .Where(c => c.UserId == creatorId)
            .FirstOrDefaultAsync(cancellationToken);

        if (creator == null)
            throw new ArgumentException("Ungültiger User als Creator übergeben");

        // Überlegung ob Gruppen theoretisch auch nur den Creator haben dürfen
        if (chatCreateDto.Users.Count < 1)
            throw new ArgumentException("Bei der Chat-Erstellung muss mindestens ein anderer User angegeben werden!");

        if (chatCreateDto.Users.Count > 49)
            throw new ArgumentException("Bei der Chat-Erstellung dürfen maximal 49 andere User angegeben werden!");

        var newChat = new Chat
        {
            ChatId = Guid.NewGuid(),
            ChatType = chatCreateDto.ChatType,
            Name = chatCreateDto.Name,
            Description = chatCreateDto.Description
        };

        newChat.UserChats.Add(new UserChat
        {
            User = creator,
            IsAdmin = true
        });

        User? otherUser = null;
        if (chatCreateDto.ChatType == ChatType.Direct)
        {
            if (chatCreateDto.Users.Count != 1)
            {
                throw new ArgumentException(
                    "Es darf bei einem Direktchat nur genau ein anderer User angegeben werden!");
            }

            otherUser = await db.User
                .Where(c => c.UserId == chatCreateDto.Users.First().UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (otherUser == null)
                throw new ArgumentException("Angegebener User wurde nicht gefunden!");

            if (otherUser.UserId == creatorId)
                throw new ArgumentException("Man kann keinen Direktchat mit sich selbst erstellen!");

            newChat.UserChats.Add(new UserChat
            {
                User = otherUser,
                IsAdmin = true
            }); // Im Direktchat sind beide gleichberechtigt

            newChat.Name = null;
            newChat.Description = null;
        }
        else if (chatCreateDto.ChatType == ChatType.Groupchat)
        {
            var trimmedName = chatCreateDto.Name?.Trim();
            var trimmedDescription = chatCreateDto.Description?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new ArgumentException("Bei einer Gruppe muss zwingend ein Gruppenname angegeben werden!");

            if (trimmedName.Length > 50)
                throw new ArgumentException("Der Gruppenname darf maximal 50 Zeichen haben!");

            if (trimmedDescription?.Length > 250)
                throw new ArgumentException("Die Gruppenbeschreibung darf maximal 250 Zeichen haben!");

            newChat.Name = trimmedName;
            newChat.Description = string.IsNullOrEmpty(trimmedDescription) ? null : trimmedDescription;

            var userIds = chatCreateDto.Users.Select(u => u.UserId).ToList();

            if (userIds.Contains(creatorId))
                throw new ArgumentException("Der Ersteller ist bereits automatisch Mitglied der Gruppe!");

            var users = await db.User
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync(cancellationToken);

            if (users.Count != userIds.Count)
                throw new ArgumentException("Es wurden nicht alle angegebenen Benutzer gefunden!");

            foreach (var user in users)
            {
                if (creator.Role == UserRole.Student && user.Role == UserRole.Employee)
                    throw new ArgumentException("Ein Schüler darf keinen Lehrer zu einem Gruppenchat hinzufügen");

                newChat.UserChats.Add(new UserChat
                {
                    User = user,
                    IsAdmin = user.Role == UserRole.Employee
                });
            }
        }
        else
        {
            throw new ArgumentException("Ungültiger Chattyp übergeben!");
        }

        // Damit wenn eine SaveChangesAsync durchläuft, aber die andere nicht, beide nicht gespeichert werden!
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await db.Chat.AddAsync(newChat, cancellationToken);

            var changedLines = await db.SaveChangesAsync(cancellationToken);
            if (changedLines == 0)
                throw new Exception("Unerwarteter Fehler beim Speichern des Chats!");

            var messageDto = await messageService.CreateSystemMessage(newChat.ChatId,
                $"{creator.FirstName} {creator.LastName} hat den Chat erstellt.", cancellationToken);

            newChat.LastMessageId = messageDto.MessageId;

            changedLines = await db.SaveChangesAsync(cancellationToken);

            if (changedLines == 0)
                throw new Exception("Unerwarteter Fehler beim Speichern der Systemnachricht!");

            await transaction.CommitAsync(cancellationToken);

            var displayName = newChat.ChatType == ChatType.Groupchat
                ? newChat.Name!
                : $"{otherUser!.FirstName} {otherUser.LastName}";

            return new ChatDto(newChat.ChatId, displayName, newChat.Description, newChat.ChatType,
                messageDto.MessageId,
                messageDto.Content,
                messageDto.SenderName,
                messageDto.SentAt,
                newChat.CreatedAt);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new Exception("Unerwarteter Fehler beim Speichern des Chats!");
        }
    }

    public async Task<ChatLeaveResultDto> LeaveChat(Guid chatId, Guid senderId,
        CancellationToken cancellationToken = default)
    {
        var userChat = await db.UserChat
            .Include(uc => uc.Chat)
            .Where(uc => uc.UserId == senderId && uc.ChatId == chatId)
            .FirstOrDefaultAsync(cancellationToken);

        if (userChat is null)
            throw new ArgumentException("Benutzer ist nicht Mitglied des Chats!");

        if (userChat.Chat.ChatType != ChatType.Groupchat)
            throw new ArgumentException("Es können nur Gruppenchats verlassen werden!");

        db.UserChat.Remove(userChat);

        int changedLines;
        // Überlegung ob Chat dann noch existieren soll aber ohne Schreibrechte
        if (!await db.UserChat.AnyAsync(uc => uc.ChatId == chatId && uc.UserId != senderId, cancellationToken))
        {
            var chat = await db.Chat.FirstOrDefaultAsync(c => c.ChatId == chatId, cancellationToken);
            if (chat is null)
                throw new ArgumentException("Chat wurde nicht gefunden!");

            db.Chat.Remove(chat);

            changedLines = await db.SaveChangesAsync(cancellationToken);
            if (changedLines == 0)
                throw new Exception("Unerwarteter Fehler beim Verlassen des Chats!");

            return new ChatLeaveResultDto(null);
        }

        var sender = await userService.GetUser(senderId, cancellationToken);
        var messageDto = await messageService.CreateSystemMessage(chatId,
            $"{sender.FirstName} {sender.LastName} hat die Gruppe verlassen.", cancellationToken);

        MessageDto? promotionMessage = null;
        Guid? newAdminId = null;
        if (userChat.IsAdmin)
        {
            var adminLeft =
                await db.UserChat.FirstOrDefaultAsync(uc => uc.ChatId == chatId && uc.IsAdmin && uc.UserId != senderId,
                    cancellationToken);

            if (adminLeft is null)
            {
                var firstUserInChat = await db.UserChat
                    .Include(uc => uc.User)
                    .FirstAsync(uc => uc.ChatId == chatId && uc.UserId != senderId, cancellationToken);
                firstUserInChat.IsAdmin = true;

                promotionMessage = await messageService.CreateSystemMessage(chatId,
                    $"{firstUserInChat.User.FirstName} {firstUserInChat.User.LastName} wurde automatisch befördert.",
                    cancellationToken);
                newAdminId = firstUserInChat.UserId;
            }
        }

        changedLines = await db.SaveChangesAsync(cancellationToken);
        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Verlassen des Chats!");

        return new ChatLeaveResultDto(messageDto, promotionMessage, newAdminId);
    }

    public async Task<MessageDto> RemoveUserFromChat(Guid chatId, Guid userId, Guid senderId,
        CancellationToken cancellationToken = default)
    {
        if (userId == senderId)
            throw new ArgumentException("Man kann sich nicht selbst aus der Gruppe entfernen!");

        var userChats = await db.UserChat
            .Include(uc => uc.Chat)
            .Where(uc => (uc.UserId == senderId || uc.UserId == userId) &&
                         uc.ChatId == chatId)
            .ToListAsync(cancellationToken);

        if (userChats.Count != 2)
            throw new ArgumentException("Benutzer nicht gefunden oder nicht Mitglied des Chats!");

        var senderChat = userChats.First(uc => uc.UserId == senderId);

        if (senderChat.Chat.ChatType == ChatType.Direct)
            throw new ArgumentException("Benutzer können nicht aus Direktchats entfernt werden");

        if (!senderChat.IsAdmin)
            throw new UnauthorizedAccessException("Sender ist kein Admin des Chats!");

        db.UserChat.Remove(userChats.First(uc => uc.UserId == userId));

        var user = await userService.GetUser(userId, cancellationToken);
        var sender = await userService.GetUser(senderId, cancellationToken);
        var messageDto = await messageService.CreateSystemMessage(chatId,
            $"{user.FirstName} {user.LastName} wurde von {sender.FirstName} {sender.LastName} aus der Gruppe entfernt.",
            cancellationToken);

        var changedLines = await db.SaveChangesAsync(cancellationToken);
        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Entfernen des Benutzers aus dem Chat!");

        return messageDto;
    }

    public async Task DeleteChat(Guid chatId, Guid senderId, CancellationToken cancellationToken = default)
    {
        var userChat = await db.UserChat
            .Include(uc => uc.Chat)
            .FirstOrDefaultAsync(uc => uc.UserId == senderId && uc.ChatId == chatId, cancellationToken);

        if (userChat is null)
            throw new ArgumentException("Benutzer ist nicht Mitglied des Chats!");

        if (userChat.Chat.ChatType != ChatType.Groupchat)
            throw new ArgumentException("Es können nur Gruppenchats gelöscht werden!");

        if (!userChat.IsAdmin)
            throw new UnauthorizedAccessException("Sender ist kein Admin des Chats!");

        db.Chat.Remove(userChat.Chat);

        var changedLines = await db.SaveChangesAsync(cancellationToken);
        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Löschen des Chats!");
    }

    public async Task<MessageDto> PromoteUser(Guid chatId, Guid userId, Guid senderId,
        CancellationToken cancellationToken = default)
    {
        if (userId == senderId)
            throw new ArgumentException("Der Sender kann sich nicht selbst zum Admin befördern!");

        var userChats = await db.UserChat
            .Where(uc => (uc.UserId == senderId || uc.UserId == userId) &&
                         uc.ChatId == chatId)
            .ToListAsync(cancellationToken);

        if (userChats.Count != 2)
            throw new ArgumentException("Benutzer nicht gefunden oder nicht Mitglied des Chats!");

        var isGroupChat = await db.Chat.AnyAsync(c => c.ChatId == chatId && c.ChatType == ChatType.Groupchat, cancellationToken);
        if (!isGroupChat)
            throw new ArgumentException("Administratoren können nur in Gruppenchats verwaltet werden!");

        if (!userChats.First(uc => uc.UserId == senderId).IsAdmin)
            throw new UnauthorizedAccessException("Sender ist kein Admin des Chats!");

        var userToPromoteChat = userChats.First(uc => uc.UserId == userId);

        if (userToPromoteChat.IsAdmin)
            throw new ArgumentException("Benutzer ist bereits Administrator!");

        userToPromoteChat.IsAdmin = true;

        var user = await userService.GetUser(userId, cancellationToken);
        var sender = await userService.GetUser(senderId, cancellationToken);
        var messageDto = await messageService.CreateSystemMessage(chatId,
            $"{user.FirstName} {user.LastName} wurde von {sender.FirstName} {sender.LastName} befördert.",
            cancellationToken);


        var changedLines = await db.SaveChangesAsync(cancellationToken);
        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Speichern der Beförderung!");

        return messageDto;
    }

    public async Task<MessageDto> DemoteUser(Guid chatId, Guid userId, Guid senderId,
        CancellationToken cancellationToken = default)
    {
        if (userId == senderId)
            throw new ArgumentException(
                "Der Sender kann sich nicht selbst degradieren!"); // Überlegung, ob er das dürfen sollte?

        var userChats = await db.UserChat
            .Where(uc => (uc.UserId == senderId || uc.UserId == userId) &&
                         uc.ChatId == chatId)
            .ToListAsync(cancellationToken);

        if (userChats.Count != 2)
            throw new ArgumentException("Benutzer nicht gefunden oder nicht Mitglied des Chats!");

        var isGroupChat = await db.Chat.AnyAsync(c => c.ChatId == chatId && c.ChatType == ChatType.Groupchat, cancellationToken);
        if (!isGroupChat)
            throw new ArgumentException("Administratoren können nur in Gruppenchats verwaltet werden!");

        if (!userChats.First(uc => uc.UserId == senderId).IsAdmin)
            throw new UnauthorizedAccessException("Sender ist kein Admin des Chats!");

        var userToPromoteChat = userChats.First(uc => uc.UserId == userId);

        if (!userToPromoteChat.IsAdmin)
            throw new ArgumentException("Benutzer hat bereits keine Administrator-Rechte!");

        userToPromoteChat.IsAdmin = false;

        var user = await userService.GetUser(userId, cancellationToken);
        var sender = await userService.GetUser(senderId, cancellationToken);
        var messageDto = await messageService.CreateSystemMessage(chatId,
            $"{user.FirstName} {user.LastName} wurde von {sender.FirstName} {sender.LastName} degradiert.",
            cancellationToken);

        var changedLines = await db.SaveChangesAsync(cancellationToken);
        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Speichern der Degradierung!");

        return messageDto;
    }

    public async Task<IReadOnlyCollection<ChatUsersAddResponseDto>> AddUsersToChat(ChatUsersAddDto chatUsersAddDto,
        Guid chatId, Guid senderId, CancellationToken cancellationToken = default)
    {
        if (chatUsersAddDto.UserIds.Count == 0)
            throw new ArgumentException("Es muss mindestens ein Benutzer angegeben werden!");

        if (chatUsersAddDto.UserIds.Contains(senderId))
            throw new ArgumentException("Der Sender kann sich nicht selbst zu einem Chat hinzufügen!");

        var userChat = await db.UserChat
            .AsNoTracking()
            .Include(c => c.Chat)
            .Where(uc => uc.UserId == senderId && uc.ChatId == chatId)
            .FirstOrDefaultAsync(cancellationToken);

        if (userChat is null)
            throw new ArgumentException("Benutzer nicht gefunden oder nicht Mitglied des Chats!");

        if (!userChat.IsAdmin)
            throw new UnauthorizedAccessException("Sender ist kein Admin des Chats!");

        if (userChat.Chat.ChatType != ChatType.Groupchat)
            throw new ArgumentException("Benutzer können nur zu Gruppenchats hinzugefügt werden!");

        var users = await db.User
            .Include(u => u.Student)
            .ThenInclude(s => s != null ? s.Class : null)
            .Where(u => chatUsersAddDto.UserIds.Contains(u.UserId))
            .ToListAsync(cancellationToken);

        if (users.Count != chatUsersAddDto.UserIds.Count)
            throw new ArgumentException("Benutzer nicht gefunden!");

        var userIdsAlreadyInChat = await db.UserChat
            .AsNoTracking()
            .Where(uc => uc.ChatId == chatId && chatUsersAddDto.UserIds.Contains(uc.UserId))
            .Select(uc => uc.UserId)
            .ToListAsync(cancellationToken);

        if (userIdsAlreadyInChat.Count > 0)
            throw new ArgumentException("Mindestens ein Benutzer ist schon Mitglied des Chats!");

        var currentMemberCount = await db.UserChat.CountAsync(uc => uc.ChatId == chatId, cancellationToken);
        if (currentMemberCount + users.Count > 50)
            throw new InvalidOperationException("Das Limit von 50 Mitglieder wird überschritten!");

        var sender = await userService.GetUser(senderId, cancellationToken);

        ICollection<ChatUsersAddResponseDto> chatUsersAddResponseDtos = [];

        foreach (var user in users)
        {
            if (sender.Role == UserRole.Student && user.Role == UserRole.Employee)
                throw new ArgumentException("Schüler dürfen keine Lehrer zu Chats hinzufügen!");

            var newUserChat = new UserChat()
            {
                User = user,
                ChatId = chatId,
                IsAdmin = user.Role == UserRole.Employee
            };

            db.UserChat.Add(newUserChat);

            var messageDto = await messageService.CreateSystemMessage(chatId,
                $"{user.FirstName} {user.LastName} wurde von {sender.FirstName} {sender.LastName} hinzugefügt.",
                cancellationToken);

            var chatUserAddResponseDto = new ChatUsersAddResponseDto(
                new UserOnChatInfoDto(user.UserId, user.FirstName, user.LastName, user.Role, newUserChat.IsAdmin,
                    user.Student != null && user.Student.Class != null ? user.Student.Class.ClassName : ""),
                messageDto);

            chatUsersAddResponseDtos.Add(chatUserAddResponseDto);
        }

        var changedLines = await db.SaveChangesAsync(cancellationToken);
        if (changedLines == 0)
            throw new Exception("Unerwarteter Fehler beim Hinzufügen der Benutzer!");

        return chatUsersAddResponseDtos.ToList().AsReadOnly();
    }
}