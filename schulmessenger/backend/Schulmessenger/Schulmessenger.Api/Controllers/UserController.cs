using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schulmessenger.Api.Extensions;
using Schulmessenger.Application.Interfaces.Services;

namespace Schulmessenger.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("filtered")]
    public async Task<IActionResult> GetUsers(Guid? excludedChatId, CancellationToken cancellationToken)
    {
        var senderId = User.GetUserId();
        var users = await userService.GetFilteredUsers(senderId, excludedChatId, cancellationToken);
        return Ok(users);
    }
}