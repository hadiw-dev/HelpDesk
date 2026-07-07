using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Admin.ActivityLogs.Dtos;
using HelpDesk.Application.Features.Admin.ActivityLogs.Interfaces;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services.Shared;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class ActivityLogQueryService : IActivityLogQueryService
{
    private readonly AppDbContext _dbContext;

    public ActivityLogQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ActivityLogEntryDto>> SearchAsync(ActivityLogQueryParameters query, CancellationToken cancellationToken = default)
    {
        var logsQuery = _dbContext.ActivityLogs.AsNoTracking().AsQueryable();

        if (query.UserId.HasValue)
        {
            logsQuery = logsQuery.Where(l => l.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            logsQuery = logsQuery.Where(l => l.Action == query.Action);
        }

        if (query.DateFrom.HasValue)
        {
            logsQuery = logsQuery.Where(l => l.CreatedAt >= query.DateFrom.Value);
        }

        if (query.DateTo.HasValue)
        {
            logsQuery = logsQuery.Where(l => l.CreatedAt <= query.DateTo.Value);
        }

        var totalCount = await logsQuery.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var entries = await logsQuery
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = entries.Where(l => l.UserId.HasValue).Select(l => l.UserId!.Value).Distinct().ToList();
        var names = await UserDisplayNameResolver.GetNamesAsync(_dbContext, userIds, cancellationToken);

        var items = entries.Select(l => new ActivityLogEntryDto
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = l.UserId.HasValue ? names.GetValueOrDefault(l.UserId.Value, "Unknown") : "System",
            Action = l.Action,
            EntityName = l.EntityName,
            EntityId = l.EntityId,
            Details = l.Details,
            IpAddress = l.IpAddress,
            CreatedAt = l.CreatedAt,
        }).ToList();

        return new PagedResult<ActivityLogEntryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }
}
