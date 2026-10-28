using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schulmessenger.Application.Interfaces.Services;

namespace Schulmessenger.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClassController(IClassService classService) : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> GetClasses(CancellationToken cancellationToken)
    {
        var classDtos = await classService.GetClasses(cancellationToken);
        return Ok(classDtos);
    }
}