using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.V1;

/// <summary>
/// Minimal versioned endpoints demonstrating the /api/v1 routing convention plus
/// role-based and policy-based authorization. Real feature controllers are added in later phases.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "HelpDesk.Api",
            version = "1.0",
            timestampUtc = DateTime.UtcNow
        });
    }

    /// <summary>Demonstrates role-based authorization: only the Admin role may call this.</summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAdmin()
    {
        return Ok(new { status = "ok", message = "Hello, Admin." });
    }

    /// <summary>Demonstrates policy-based authorization: Admin or Manager, via a named policy.</summary>
    [HttpGet("management")]
    [Authorize(Policy = "RequireManagerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetManagement()
    {
        return Ok(new { status = "ok", message = "Hello, management." });
    }
}
