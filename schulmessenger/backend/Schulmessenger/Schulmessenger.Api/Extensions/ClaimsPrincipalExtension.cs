using System.Security.Claims;

namespace Schulmessenger.Api.Extensions;

public static class ClaimsPrincipalExtension
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        return Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}