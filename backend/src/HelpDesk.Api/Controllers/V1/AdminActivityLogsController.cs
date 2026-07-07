using Asp.Versioning;
using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Admin.ActivityLogs.Dtos;
using HelpDesk.Application.Features.Admin.ActivityLogs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireAdmin")]
[Route("api/v{version:apiVersion}/admin/activity-logs")]
public class AdminActivityLogsController : ControllerBase
{
    private readonly IActivityLogQueryService _activityLogQueryService;

    public AdminActivityLogsController(IActivityLogQueryService activityLogQueryService)
    {
        _activityLogQueryService = activityLogQueryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ActivityLogEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ActivityLogEntryDto>>> Search(
        [FromQuery] ActivityLogQueryParameters query, CancellationToken cancellationToken)
    {
        return Ok(await _activityLogQueryService.SearchAsync(query, cancellationToken));
    }
}
