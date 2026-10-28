using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Schulmessenger.Api.Extensions;
using Schulmessenger.Api.Hubs;
using Schulmessenger.Application.Interfaces.Services;

namespace Schulmessenger.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController(IMessageService messageService, IHubContext<SchulmessengerHub> schulmessengerHub)
    : ControllerBase
{
    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetMessages(Guid chatId, CancellationToken cancellationToken)
    {
        var senderId = User.GetUserId();
        return Ok(await messageService.GetMessages(chatId, senderId, cancellationToken));
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId, CancellationToken cancellationToken)
    {
        var senderId = User.GetUserId();
        var deletedMessage = await messageService.DeleteMessage(messageId, senderId, cancellationToken);

        await schulmessengerHub.Clients.GroupExcept(deletedMessage.ChatId.ToString(), Request.GetConnectionId())
            .SendAsync("ReceiveDeletedMessage", deletedMessage, cancellationToken);

        return Ok(deletedMessage);
    }
}