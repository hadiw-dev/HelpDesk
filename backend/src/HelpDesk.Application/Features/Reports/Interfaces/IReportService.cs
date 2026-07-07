using HelpDesk.Application.Features.Dashboard.Dtos;

namespace HelpDesk.Application.Features.Reports.Interfaces;

/// <summary>Renders the same data <see cref="Dashboard.Interfaces.IDashboardService"/> exposes into a downloadable document.</summary>
public interface IReportService
{
    Task<byte[]> GenerateDashboardPdfAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default);

    Task<byte[]> GenerateDashboardExcelAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default);
}
