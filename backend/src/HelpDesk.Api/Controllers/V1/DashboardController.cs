using Asp.Versioning;
using HelpDesk.Application.Features.Dashboard.Dtos;
using HelpDesk.Application.Features.Dashboard.Interfaces;
using HelpDesk.Application.Features.Reports.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IReportService _reportService;

    public DashboardController(IDashboardService dashboardService, IReportService reportService)
    {
        _dashboardService = dashboardService;
        _reportService = reportService;
    }

    [HttpGet("kpi-summary")]
    [ProducesResponseType(typeof(KpiSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<KpiSummaryDto>> GetKpiSummary([FromQuery] DashboardQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetKpiSummaryAsync(query, cancellationToken));

    [HttpGet("tickets-by-category")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryBreakdownDto>>> GetTicketsByCategory([FromQuery] DashboardQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetCategoryBreakdownAsync(query, cancellationToken));

    [HttpGet("tickets-by-priority")]
    [ProducesResponseType(typeof(IReadOnlyList<PriorityBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PriorityBreakdownDto>>> GetTicketsByPriority([FromQuery] DashboardQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetPriorityBreakdownAsync(query, cancellationToken));

    [HttpGet("monthly-tickets")]
    [ProducesResponseType(typeof(IReadOnlyList<MonthlyTicketsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MonthlyTicketsDto>>> GetMonthlyTickets([FromQuery] DashboardQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetMonthlyTicketsAsync(query, cancellationToken));

    [HttpGet("resolution-time")]
    [ProducesResponseType(typeof(ResolutionTimeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResolutionTimeDto>> GetResolutionTime([FromQuery] DashboardQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetResolutionTimeAsync(query, cancellationToken));

    [HttpGet("sla-report")]
    [ProducesResponseType(typeof(SlaReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SlaReportDto>> GetSlaReport([FromQuery] DashboardQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dashboardService.GetSlaReportAsync(query, cancellationToken));

    [HttpGet("reports/pdf")]
    public async Task<IActionResult> GetPdfReport([FromQuery] DashboardQueryParameters query, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.GenerateDashboardPdfAsync(query, cancellationToken);
        return File(bytes, "application/pdf", $"dashboard-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf");
    }

    [HttpGet("reports/excel")]
    public async Task<IActionResult> GetExcelReport([FromQuery] DashboardQueryParameters query, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.GenerateDashboardExcelAsync(query, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"dashboard-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx");
    }
}
