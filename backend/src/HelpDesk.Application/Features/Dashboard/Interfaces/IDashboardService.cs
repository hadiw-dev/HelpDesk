using HelpDesk.Application.Features.Dashboard.Dtos;

namespace HelpDesk.Application.Features.Dashboard.Interfaces;

/// <summary>
/// Every method scopes its results the same way ticket search does: an Employee only sees metrics
/// for tickets they created, Agent+ sees metrics across every ticket.
/// </summary>
public interface IDashboardService
{
    Task<KpiSummaryDto> GetKpiSummaryAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryBreakdownDto>> GetCategoryBreakdownAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PriorityBreakdownDto>> GetPriorityBreakdownAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyTicketsDto>> GetMonthlyTicketsAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default);

    Task<ResolutionTimeDto> GetResolutionTimeAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default);

    Task<SlaReportDto> GetSlaReportAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default);
}
