using Asp.Versioning;
using HelpDesk.Application.Features.Admin.Lookups.Dtos;
using HelpDesk.Application.Features.Admin.Lookups.Interfaces;
using HelpDesk.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.V1;

/// <summary>
/// Full CRUD for the three lookup tables — deferred from Phase 3 (which only needed read-only
/// dropdowns) to this administration phase. One controller, three near-identical action groups,
/// each delegating to the generic <see cref="IAdminLookupService{TEntity}"/> for its entity type.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireAdmin")]
[Route("api/v{version:apiVersion}/admin")]
public class AdminLookupsController : ControllerBase
{
    private readonly IAdminLookupService<Category> _categoryService;
    private readonly IAdminLookupService<Priority> _priorityService;
    private readonly IAdminLookupService<Status> _statusService;

    public AdminLookupsController(
        IAdminLookupService<Category> categoryService,
        IAdminLookupService<Priority> priorityService,
        IAdminLookupService<Status> statusService)
    {
        _categoryService = categoryService;
        _priorityService = priorityService;
        _statusService = statusService;
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminLookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminLookupItemDto>>> GetCategories(CancellationToken cancellationToken)
        => Ok(await _categoryService.GetAllAsync(cancellationToken));

    [HttpPost("categories")]
    [ProducesResponseType(typeof(AdminLookupItemDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AdminLookupItemDto>> CreateCategory(LookupUpsertRequest request, CancellationToken cancellationToken)
    {
        var created = await _categoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCategories), new { version = "1.0" }, created);
    }

    [HttpPut("categories/{id:guid}")]
    [ProducesResponseType(typeof(AdminLookupItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminLookupItemDto>> UpdateCategory(Guid id, LookupUpsertRequest request, CancellationToken cancellationToken)
        => Ok(await _categoryService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("categories/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("priorities")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminLookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminLookupItemDto>>> GetPriorities(CancellationToken cancellationToken)
        => Ok(await _priorityService.GetAllAsync(cancellationToken));

    [HttpPost("priorities")]
    [ProducesResponseType(typeof(AdminLookupItemDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AdminLookupItemDto>> CreatePriority(LookupUpsertRequest request, CancellationToken cancellationToken)
    {
        var created = await _priorityService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPriorities), new { version = "1.0" }, created);
    }

    [HttpPut("priorities/{id:guid}")]
    [ProducesResponseType(typeof(AdminLookupItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminLookupItemDto>> UpdatePriority(Guid id, LookupUpsertRequest request, CancellationToken cancellationToken)
        => Ok(await _priorityService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("priorities/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePriority(Guid id, CancellationToken cancellationToken)
    {
        await _priorityService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("statuses")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminLookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminLookupItemDto>>> GetStatuses(CancellationToken cancellationToken)
        => Ok(await _statusService.GetAllAsync(cancellationToken));

    [HttpPost("statuses")]
    [ProducesResponseType(typeof(AdminLookupItemDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AdminLookupItemDto>> CreateStatus(LookupUpsertRequest request, CancellationToken cancellationToken)
    {
        var created = await _statusService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetStatuses), new { version = "1.0" }, created);
    }

    [HttpPut("statuses/{id:guid}")]
    [ProducesResponseType(typeof(AdminLookupItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminLookupItemDto>> UpdateStatus(Guid id, LookupUpsertRequest request, CancellationToken cancellationToken)
        => Ok(await _statusService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("statuses/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStatus(Guid id, CancellationToken cancellationToken)
    {
        await _statusService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
