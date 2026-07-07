using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Features.Dashboard.Dtos;
using HelpDesk.Application.Features.Dashboard.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services.Shared;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<KpiSummaryDto> GetKpiSummaryAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default)
    {
        var baseQuery = BuildBaseQuery(query);

        var statusCounts = await baseQuery
            .GroupBy(t => t.Status.Name)
            .Select(g => new { StatusName = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var statusCountLookup = statusCounts.ToDictionary(s => s.StatusName, s => s.Count);

        var now = DateTime.UtcNow;
        var overdueCount = await baseQuery.CountAsync(
            t => t.DueDate.HasValue && t.DueDate < now && t.Status.Name != "Resolved" && t.Status.Name != "Closed",
            cancellationToken);
        var unassignedCount = await baseQuery.CountAsync(
            t => t.AssignedToUserId == null && t.Status.Name != "Closed", cancellationToken);

        var resolutionDurations = await baseQuery
            .Where(t => t.ResolvedAt.HasValue)
            .Select(t => new { t.CreatedAt, t.ResolvedAt })
            .ToListAsync(cancellationToken);

        return new KpiSummaryDto
        {
            TotalTickets = statusCounts.Sum(s => s.Count),
            OpenTickets = statusCountLookup.GetValueOrDefault("Open"),
            InProgressTickets = statusCountLookup.GetValueOrDefault("In Progress"),
            PendingTickets = statusCountLookup.GetValueOrDefault("Pending"),
            ResolvedTickets = statusCountLookup.GetValueOrDefault("Resolved"),
            ClosedTickets = statusCountLookup.GetValueOrDefault("Closed"),
            OverdueTickets = overdueCount,
            UnassignedTickets = unassignedCount,
            AverageResolutionHours = AverageHours(resolutionDurations.Select(t => (t.CreatedAt, t.ResolvedAt))),
        };
    }

    public async Task<IReadOnlyList<CategoryBreakdownDto>> GetCategoryBreakdownAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default)
    {
        var grouped = await BuildBaseQuery(query)
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.DisplayOrder })
            .Select(g => new { g.Key.CategoryId, g.Key.Name, g.Key.DisplayOrder, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return grouped
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new CategoryBreakdownDto { CategoryId = g.CategoryId, CategoryName = g.Name, Count = g.Count })
            .ToList();
    }

    public async Task<IReadOnlyList<PriorityBreakdownDto>> GetPriorityBreakdownAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default)
    {
        var grouped = await BuildBaseQuery(query)
            .GroupBy(t => new { t.PriorityId, t.Priority.Name, t.Priority.DisplayOrder })
            .Select(g => new { g.Key.PriorityId, g.Key.Name, g.Key.DisplayOrder, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return grouped
            .OrderBy(g => g.DisplayOrder)
            .Select(g => new PriorityBreakdownDto { PriorityId = g.PriorityId, PriorityName = g.Name, Count = g.Count })
            .ToList();
    }

    public async Task<IReadOnlyList<MonthlyTicketsDto>> GetMonthlyTicketsAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default)
    {
        var baseQuery = BuildBaseQuery(query);

        var created = await baseQuery
            .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var resolved = await baseQuery
            .Where(t => t.ResolvedAt.HasValue)
            .GroupBy(t => new { t.ResolvedAt!.Value.Year, t.ResolvedAt!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var months = created.Select(c => (c.Year, c.Month))
            .Concat(resolved.Select(r => (r.Year, r.Month)))
            .Distinct()
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        return months.Select(m => new MonthlyTicketsDto
        {
            Year = m.Year,
            Month = m.Month,
            MonthLabel = new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy"),
            CreatedCount = created.FirstOrDefault(c => c.Year == m.Year && c.Month == m.Month)?.Count ?? 0,
            ResolvedCount = resolved.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)?.Count ?? 0,
        }).ToList();
    }

    public async Task<ResolutionTimeDto> GetResolutionTimeAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default)
    {
        var resolvedTickets = await BuildBaseQuery(query)
            .Where(t => t.ResolvedAt.HasValue)
            .Select(t => new { t.CreatedAt, t.ResolvedAt, PriorityName = t.Priority.Name, t.Priority.DisplayOrder })
            .ToListAsync(cancellationToken);

        var byPriority = resolvedTickets
            .GroupBy(t => new { t.PriorityName, t.DisplayOrder })
            .OrderBy(g => g.Key.DisplayOrder)
            .Select(g => new PriorityResolutionTimeDto
            {
                PriorityName = g.Key.PriorityName,
                AverageResolutionHours = AverageHours(g.Select(t => (t.CreatedAt, t.ResolvedAt))),
                TicketCount = g.Count(),
            })
            .ToList();

        return new ResolutionTimeDto
        {
            OverallAverageResolutionHours = AverageHours(resolvedTickets.Select(t => (t.CreatedAt, t.ResolvedAt))),
            ByPriority = byPriority,
        };
    }

    public async Task<SlaReportDto> GetSlaReportAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default)
    {
        var trackedTickets = await BuildBaseQuery(query)
            .Where(t => t.DueDate.HasValue)
            .Select(t => new
            {
                t.Id,
                t.TicketNumber,
                t.Title,
                DueDate = t.DueDate!.Value,
                t.ResolvedAt,
                t.ClosedAt,
                StatusName = t.Status.Name,
            })
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var breaches = new List<SlaBreachEntryDto>();
        var metCount = 0;

        foreach (var ticket in trackedTickets)
        {
            var completedAt = ticket.ResolvedAt ?? ticket.ClosedAt;

            var breached = completedAt.HasValue
                ? completedAt.Value > ticket.DueDate
                : now > ticket.DueDate;

            if (!breached)
            {
                metCount++;
                continue;
            }

            var overdueAsOf = completedAt ?? now;
            breaches.Add(new SlaBreachEntryDto
            {
                TicketId = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Title = ticket.Title,
                DueDate = ticket.DueDate,
                ResolvedAt = ticket.ResolvedAt,
                StatusName = ticket.StatusName,
                HoursOverdue = (overdueAsOf - ticket.DueDate).TotalHours,
            });
        }

        var total = trackedTickets.Count;
        return new SlaReportDto
        {
            TotalTrackedTickets = total,
            MetCount = metCount,
            BreachedCount = breaches.Count,
            CompliancePercentage = total > 0 ? Math.Round(metCount * 100.0 / total, 1) : 0,
            BreachedTickets = breaches.OrderByDescending(b => b.HoursOverdue).ToList(),
        };
    }

    private IQueryable<Ticket> BuildBaseQuery(DashboardQueryParameters query)
    {
        var ticketsQuery = _dbContext.Tickets.AsNoTracking().AsQueryable();

        if (!TicketAccessGuard.IsAgentOrAbove(_currentUserService))
        {
            var userId = TicketAccessGuard.RequireUserId(_currentUserService);
            ticketsQuery = ticketsQuery.Where(t => t.CreatedByUserId == userId);
        }

        if (query.DateFrom.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.CreatedAt >= query.DateFrom.Value);
        }

        if (query.DateTo.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.CreatedAt <= query.DateTo.Value);
        }

        return ticketsQuery;
    }

    private static double? AverageHours(IEnumerable<(DateTime CreatedAt, DateTime? ResolvedAt)> tickets)
    {
        var durations = tickets.Where(t => t.ResolvedAt.HasValue)
            .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            .ToList();

        return durations.Count > 0 ? durations.Average() : null;
    }
}
