using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Schulmessenger.Api.Extensions;
using Schulmessenger.Api.Hubs;
using Schulmessenger.Application.Interfaces.Services;
using Schulmessenger.Application.Models;
using Schulmessenger.Domain.Entities;

namespace Schulmessenger.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController(
    IChatService chatService,
    IHubContext<SchulmessengerHub> schulmessengerHub) : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> GetChats(CancellationToken cancellationToken)
    {
        return Ok(await chatService.GetChats(User.GetUserId(), cancellationToken));
    }

    [HttpGet("{chatId}/users")]
    public async Task<IActionResult> GetChatUsers(Guid chatId, CancellationToken cancellationToken)
    {
        return Ok(await chatService.GetChatUsers(chatId, User.GetUserId(), cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult>
        CreateChat(ChatCreateDto chatCreateDto, CancellationToken cancellationToken)
    {
        var senderId = User.GetUserId();
        var createdChat = await chatService.CreateChat(chatCreateDto, senderId, cancellationToken);

        if (createdChat is null)
            throw new ArgumentException("Fehler beim Erstellen des Chats!");

        var userIds = chatCreateDto.Users.Select(u => u.UserId.ToString()).ToList();

        if (createdChat.ChatType == ChatType.Groupchat)
        {
            userIds.Add(senderId.ToString());

            await schulmessengerHub.Clients.Users(userIds)
                .SendAsync("ReceiveNewChat", createdChat, senderId, cancellationToken);
        }
        else if (createdChat.ChatType == ChatType.Direct)
        {
            await schulmessengerHub.Clients.User(senderId.ToString())
                .SendAsync("ReceiveNewChat", createdChat, senderId, cancellationToken);

            var otherUserId = chatCreateDto.Users.First().UserId;
            var otherUserChat = createdChat with
            {
                Name = $"{User.FindFirst("FirstName")?.Value} {User.FindFirst("LastName")?.Value}"
            };

            await schulmessengerHub.Clients.User(otherUserId.ToString())
                .SendAsync("ReceiveNewChat", otherUserChat, senderId, cancellationToken);
        }
        else
        {
            throw new ArgumentException("Ungültiger Chattyp übergeben!");
        }

        return Ok(createdChat);
    }

    [HttpDelete("{chatId}/leave")]
    public async Task<IActionResult> LeaveChat(Guid chatId, CancellationToken cancellationToken)
    {
        var senderId = User.GetUserId();
        var connectionId = Request.GetConnectionId();

        var result = await chatService.LeaveChat(chatId, senderId, cancellationToken);

        await schulmessengerHub.Groups.RemoveFromGroupAsync(connectionId, chatId.ToString(), cancellationToken);

        if (result.LeaveMessage is not null)
        {
            await schulmessengerHub.Clients.Group(chatId.ToString())
                .SendAsync("ReceiveUserLeftChat", chatId, senderId, cancellationToken);

            await schulmessengerHub.Clients.Group(chatId.ToString())
                .SendAsync("ReceiveMessage", result.LeaveMessage, cancellationToken);

            if (result.PromotionMessage is not null)
            {
                await schulmessengerHub.Clients.Group(chatId.ToString())
                    .SendAsync("ReceiveMessage", result.PromotionMessage, cancellationToken);

                await schulmessengerHub.Clients.Group(chatId.ToString())
                    .SendAsync("ReceiveUserPromoted", chatId, result.NewAdminId.ToString(), cancellationToken);
            }
        }

        return NoContent();
    }

    [HttpDelete("{chatId}/remove/{userId}")]
    public async Task<IActionResult>
        RemoveUserFromChat(Guid chatId, Guid userId, CancellationToken cancellationToken)
    {
        var senderId = User.GetUserId();
        var connectionId = Request.GetConnectionId();
        var messageDto = await chatService.RemoveUserFromChat(chatId, userId, senderId, cancellationToken);

        await schulmessengerHub.Clients.GroupExcept(chatId.ToString(), connectionId)
            .SendAsync("ReceiveUserRemovedFromChat", chatId, userId, cancellationToken);

        await schulmessengerHub.Clients.Group(chatId.ToString())
            .SendAsync("ReceiveMessage", messageDto, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{chatId}/delete")]
    public async Task<IActionResult> DeleteChat(Guid chatId, CancellationToken cancellationToken)
    {
        var senderId = User.GetUserId();
        var connectionId = Request.GetConnectionId();
        await chatService.DeleteChat(chatId, senderId, cancellationToken);

        await schulmessengerHub.Clients.GroupExcept(chatId.ToString(), connectionId)
            .SendAsync("ReceiveChatDeleted", chatId, cancellationToken);

        return NoContent();
    }

    [HttpPatch("{chatId}/promote/{userId}")]
    public async Task<IActionResult>
        PromoteUser(Guid chatId, Guid userId, CancellationToken cancellationToken)
    {
        var senderId = User.GetUserId();
        var connectionId = Request.GetConnectionId();
        var messageDto = await chatService.PromoteUser(chatId, userId, senderId, cancellationToken);

        await schulmessengerHub.Clients.GroupExcept(chatId.ToString(), connectionId)
            .SendAsync("ReceiveUserPromoted", chatId, userId, cancellationToken);

        await schulmessengerHub.Clients.Group(chatId.ToString())
            .SendAsync("ReceiveMessage", messageDto, cancellationToken);

        return NoContent();
    }

    [HttpPatch("{chatId}/demote/{userId}")]
    public async Task<IActionResult>
        DemoteUser(Guid chatId, Guid userId, CancellationToken cancellationToken)
    {
        var senderId = User.GetUserId();
        var connectionId = Request.GetConnectionId();
        var messageDto = await chatService.DemoteUser(chatId, userId, senderId, cancellationToken);

        await schulmessengerHub.Clients.GroupExcept(chatId.ToString(), connectionId)
            .SendAsync("ReceiveUserDemoted", chatId, userId, cancellationToken);

        await schulmessengerHub.Clients.Group(chatId.ToString())
            .SendAsync("ReceiveMessage", messageDto, cancellationToken);

        return NoContent();
    }

    [HttpPost("{chatId}/add")]
    public async Task<IActionResult>
        AddUser(Guid chatId, ChatUsersAddDto chatUsersAddDto, CancellationToken cancellationToken)
    {
        var senderId = User.GetUserId();
        var connectionId = Request.GetConnectionId();
        var chatUsersAddResponseDtos = await chatService.AddUsersToChat(chatUsersAddDto, chatId, senderId, cancellationToken);
        var chatDto = await chatService.GetChat(chatId, senderId, cancellationToken);

        foreach (var chatUserAddResponseDto in chatUsersAddResponseDtos)
        {
            await schulmessengerHub.Clients.GroupExcept(chatId.ToString(), connectionId)
                .SendAsync("ReceiveUserAdded", chatId,
                    chatUserAddResponseDto.AddedUser, cancellationToken);

            await schulmessengerHub.Clients.User(chatUserAddResponseDto.AddedUser.UserId.ToString())
                .SendAsync("ReceiveNewChat", chatDto, senderId, cancellationToken);

            await schulmessengerHub.Clients.Group(chatId.ToString())
                .SendAsync("ReceiveMessage", chatUserAddResponseDto.SystemMessage, cancellationToken);
        }

        return Ok(chatUsersAddResponseDtos.Select(c => c.AddedUser));
    }
}