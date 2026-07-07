using Asp.Versioning;
using HelpDesk.Application.Features.Lookups.Dtos;
using HelpDesk.Application.Features.Lookups.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.V1;

/// <summary>
/// Read-only reference data for populating dropdowns (ticket create/edit forms, filters).
/// Full CRUD management of lookup values is an administration-phase concern, not this one.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public LookupsController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetCategories(CancellationToken cancellationToken)
        => Ok(await _lookupService.GetCategoriesAsync(cancellationToken));

    [HttpGet("priorities")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetPriorities(CancellationToken cancellationToken)
        => Ok(await _lookupService.GetPrioritiesAsync(cancellationToken));

    [HttpGet("statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetStatuses(CancellationToken cancellationToken)
        => Ok(await _lookupService.GetStatusesAsync(cancellationToken));

    [HttpGet("agents")]
    [Authorize(Policy = "RequireAgentOrAbove")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> GetAgents(CancellationToken cancellationToken)
        => Ok(await _lookupService.GetAssignableAgentsAsync(cancellationToken));
}
