using ClosedXML.Excel;
using HelpDesk.Application.Features.Dashboard.Dtos;
using HelpDesk.Application.Features.Dashboard.Interfaces;
using HelpDesk.Application.Features.Reports.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HelpDesk.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IDashboardService _dashboardService;

    public ReportService(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<byte[]> GenerateDashboardPdfAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default)
    {
        var (kpi, categories, priorities, monthly, resolutionTime, sla) = await GatherDataAsync(query, cancellationToken);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text("HelpDesk Dashboard Report").FontSize(18).Bold();

                page.Content().PaddingTop(10).Column(column =>
                {
                    column.Spacing(14);

                    column.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).FontColor(Colors.Grey.Darken1);
                    if (query.DateFrom.HasValue || query.DateTo.HasValue)
                    {
                        column.Item().Text($"Range: {query.DateFrom?.ToString("yyyy-MM-dd") ?? "(any)"} to {query.DateTo?.ToString("yyyy-MM-dd") ?? "(any)"}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    }

                    column.Item().Text("KPI Summary").Bold().FontSize(13);
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        AddRow(table, "Total Tickets", kpi.TotalTickets.ToString());
                        AddRow(table, "Open", kpi.OpenTickets.ToString());
                        AddRow(table, "In Progress", kpi.InProgressTickets.ToString());
                        AddRow(table, "Pending", kpi.PendingTickets.ToString());
                        AddRow(table, "Resolved", kpi.ResolvedTickets.ToString());
                        AddRow(table, "Closed", kpi.ClosedTickets.ToString());
                        AddRow(table, "Overdue", kpi.OverdueTickets.ToString());
                        AddRow(table, "Unassigned", kpi.UnassignedTickets.ToString());
                        AddRow(table, "Avg. Resolution (hours)", FormatHours(kpi.AverageResolutionHours));
                    });

                    column.Item().Text("Tickets by Category").Bold().FontSize(13);
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        foreach (var category in categories)
                        {
                            AddRow(table, category.CategoryName, category.Count.ToString());
                        }
                    });

                    column.Item().Text("Tickets by Priority").Bold().FontSize(13);
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });
                        foreach (var priority in priorities)
                        {
                            AddRow(table, priority.PriorityName, priority.Count.ToString());
                        }
                    });

                    column.Item().Text("Monthly Tickets").Bold().FontSize(13);
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                        AddRow(table, "Month", "Created", "Resolved");
                        foreach (var month in monthly)
                        {
                            AddRow(table, month.MonthLabel, month.CreatedCount.ToString(), month.ResolvedCount.ToString());
                        }
                    });

                    column.Item().Text("Resolution Time").Bold().FontSize(13);
                    column.Item().Text($"Overall average: {FormatHours(resolutionTime.OverallAverageResolutionHours)} hours");
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                        AddRow(table, "Priority", "Avg. Hours", "Tickets");
                        foreach (var priority in resolutionTime.ByPriority)
                        {
                            AddRow(table, priority.PriorityName, FormatHours(priority.AverageResolutionHours), priority.TicketCount.ToString());
                        }
                    });

                    column.Item().Text("SLA Report").Bold().FontSize(13);
                    column.Item().Text($"Tracked: {sla.TotalTrackedTickets}  |  Met: {sla.MetCount}  |  Breached: {sla.BreachedCount}  |  Compliance: {sla.CompliancePercentage}%");
                    if (sla.BreachedTickets.Count > 0)
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); });
                            AddRow(table, "Ticket #", "Title", "Status", "Hours Overdue");
                            foreach (var breach in sla.BreachedTickets)
                            {
                                AddRow(table, breach.TicketNumber, breach.Title, breach.StatusName, breach.HoursOverdue.ToString("F1"));
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateDashboardExcelAsync(DashboardQueryParameters query, CancellationToken cancellationToken = default)
    {
        var (kpi, categories, priorities, monthly, resolutionTime, sla) = await GatherDataAsync(query, cancellationToken);

        using var workbook = new XLWorkbook();

        var kpiSheet = workbook.Worksheets.Add("KPI Summary");
        kpiSheet.Cell(1, 1).Value = "Metric";
        kpiSheet.Cell(1, 2).Value = "Value";
        var kpiRows = new (string Label, string Value)[]
        {
            ("Total Tickets", kpi.TotalTickets.ToString()),
            ("Open", kpi.OpenTickets.ToString()),
            ("In Progress", kpi.InProgressTickets.ToString()),
            ("Pending", kpi.PendingTickets.ToString()),
            ("Resolved", kpi.ResolvedTickets.ToString()),
            ("Closed", kpi.ClosedTickets.ToString()),
            ("Overdue", kpi.OverdueTickets.ToString()),
            ("Unassigned", kpi.UnassignedTickets.ToString()),
            ("Avg. Resolution (hours)", FormatHours(kpi.AverageResolutionHours)),
        };
        for (var i = 0; i < kpiRows.Length; i++)
        {
            kpiSheet.Cell(i + 2, 1).Value = kpiRows[i].Label;
            kpiSheet.Cell(i + 2, 2).Value = kpiRows[i].Value;
        }
        kpiSheet.Columns().AdjustToContents();

        var categorySheet = workbook.Worksheets.Add("Tickets by Category");
        categorySheet.Cell(1, 1).Value = "Category";
        categorySheet.Cell(1, 2).Value = "Count";
        for (var i = 0; i < categories.Count; i++)
        {
            categorySheet.Cell(i + 2, 1).Value = categories[i].CategoryName;
            categorySheet.Cell(i + 2, 2).Value = categories[i].Count;
        }
        categorySheet.Columns().AdjustToContents();

        var prioritySheet = workbook.Worksheets.Add("Tickets by Priority");
        prioritySheet.Cell(1, 1).Value = "Priority";
        prioritySheet.Cell(1, 2).Value = "Count";
        for (var i = 0; i < priorities.Count; i++)
        {
            prioritySheet.Cell(i + 2, 1).Value = priorities[i].PriorityName;
            prioritySheet.Cell(i + 2, 2).Value = priorities[i].Count;
        }
        prioritySheet.Columns().AdjustToContents();

        var monthlySheet = workbook.Worksheets.Add("Monthly Tickets");
        monthlySheet.Cell(1, 1).Value = "Month";
        monthlySheet.Cell(1, 2).Value = "Created";
        monthlySheet.Cell(1, 3).Value = "Resolved";
        for (var i = 0; i < monthly.Count; i++)
        {
            monthlySheet.Cell(i + 2, 1).Value = monthly[i].MonthLabel;
            monthlySheet.Cell(i + 2, 2).Value = monthly[i].CreatedCount;
            monthlySheet.Cell(i + 2, 3).Value = monthly[i].ResolvedCount;
        }
        monthlySheet.Columns().AdjustToContents();

        var resolutionSheet = workbook.Worksheets.Add("Resolution Time");
        resolutionSheet.Cell(1, 1).Value = "Overall average (hours)";
        resolutionSheet.Cell(1, 2).Value = FormatHours(resolutionTime.OverallAverageResolutionHours);
        resolutionSheet.Cell(3, 1).Value = "Priority";
        resolutionSheet.Cell(3, 2).Value = "Avg. Hours";
        resolutionSheet.Cell(3, 3).Value = "Tickets";
        for (var i = 0; i < resolutionTime.ByPriority.Count; i++)
        {
            var row = resolutionTime.ByPriority[i];
            resolutionSheet.Cell(i + 4, 1).Value = row.PriorityName;
            resolutionSheet.Cell(i + 4, 2).Value = FormatHours(row.AverageResolutionHours);
            resolutionSheet.Cell(i + 4, 3).Value = row.TicketCount;
        }
        resolutionSheet.Columns().AdjustToContents();

        var slaSheet = workbook.Worksheets.Add("SLA Report");
        slaSheet.Cell(1, 1).Value = "Tracked";
        slaSheet.Cell(1, 2).Value = sla.TotalTrackedTickets;
        slaSheet.Cell(2, 1).Value = "Met";
        slaSheet.Cell(2, 2).Value = sla.MetCount;
        slaSheet.Cell(3, 1).Value = "Breached";
        slaSheet.Cell(3, 2).Value = sla.BreachedCount;
        slaSheet.Cell(4, 1).Value = "Compliance %";
        slaSheet.Cell(4, 2).Value = sla.CompliancePercentage;
        slaSheet.Cell(6, 1).Value = "Ticket #";
        slaSheet.Cell(6, 2).Value = "Title";
        slaSheet.Cell(6, 3).Value = "Status";
        slaSheet.Cell(6, 4).Value = "Hours Overdue";
        for (var i = 0; i < sla.BreachedTickets.Count; i++)
        {
            var breach = sla.BreachedTickets[i];
            slaSheet.Cell(i + 7, 1).Value = breach.TicketNumber;
            slaSheet.Cell(i + 7, 2).Value = breach.Title;
            slaSheet.Cell(i + 7, 3).Value = breach.StatusName;
            slaSheet.Cell(i + 7, 4).Value = Math.Round(breach.HoursOverdue, 1);
        }
        slaSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private async Task<(
        KpiSummaryDto Kpi,
        IReadOnlyList<CategoryBreakdownDto> Categories,
        IReadOnlyList<PriorityBreakdownDto> Priorities,
        IReadOnlyList<MonthlyTicketsDto> Monthly,
        ResolutionTimeDto ResolutionTime,
        SlaReportDto Sla)> GatherDataAsync(DashboardQueryParameters query, CancellationToken cancellationToken)
    {
        var kpi = await _dashboardService.GetKpiSummaryAsync(query, cancellationToken);
        var categories = await _dashboardService.GetCategoryBreakdownAsync(query, cancellationToken);
        var priorities = await _dashboardService.GetPriorityBreakdownAsync(query, cancellationToken);
        var monthly = await _dashboardService.GetMonthlyTicketsAsync(query, cancellationToken);
        var resolutionTime = await _dashboardService.GetResolutionTimeAsync(query, cancellationToken);
        var sla = await _dashboardService.GetSlaReportAsync(query, cancellationToken);

        return (kpi, categories, priorities, monthly, resolutionTime, sla);
    }

    private static void AddRow(TableDescriptor table, string first, string second)
    {
        table.Cell().Padding(2).Text(first);
        table.Cell().Padding(2).Text(second);
    }

    private static void AddRow(TableDescriptor table, string first, string second, string third)
    {
        table.Cell().Padding(2).Text(first);
        table.Cell().Padding(2).Text(second);
        table.Cell().Padding(2).Text(third);
    }

    private static void AddRow(TableDescriptor table, string first, string second, string third, string fourth)
    {
        table.Cell().Padding(2).Text(first);
        table.Cell().Padding(2).Text(second);
        table.Cell().Padding(2).Text(third);
        table.Cell().Padding(2).Text(fourth);
    }

    private static string FormatHours(double? hours) => hours.HasValue ? hours.Value.ToString("F1") : "—";
}
