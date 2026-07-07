using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Admin.ActivityLogs.Dtos;

namespace HelpDesk.Application.Features.Admin.ActivityLogs.Interfaces;

/// <summary>Read side of the activity log, for the Admin viewer. The write side (<c>IActivityLogService.LogAsync</c>,
/// used throughout the app) is a separate, pre-existing interface — kept untouched rather than merged into this one.</summary>
public interface IActivityLogQueryService
{
    Task<PagedResult<ActivityLogEntryDto>> SearchAsync(ActivityLogQueryParameters query, CancellationToken cancellationToken = default);
}
