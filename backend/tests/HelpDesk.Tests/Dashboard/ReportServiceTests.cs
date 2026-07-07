using ClosedXML.Excel;
using HelpDesk.Application.Features.Dashboard.Dtos;
using HelpDesk.Application.Features.Dashboard.Interfaces;
using HelpDesk.Infrastructure.Services;
using Moq;
using QuestPDF.Infrastructure;

namespace HelpDesk.Tests.Dashboard;

public class ReportServiceTests
{
    static ReportServiceTests()
    {
        // QuestPDF throws unless a license is selected; Program.cs sets this for the running app,
        // but the test host never executes Program.cs, so it must be set here too.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static Mock<IDashboardService> MockDashboardService()
    {
        var mock = new Mock<IDashboardService>();

        mock.Setup(m => m.GetKpiSummaryAsync(It.IsAny<DashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KpiSummaryDto
            {
                TotalTickets = 10,
                OpenTickets = 4,
                InProgressTickets = 2,
                PendingTickets = 1,
                ResolvedTickets = 2,
                ClosedTickets = 1,
                OverdueTickets = 1,
                UnassignedTickets = 3,
                AverageResolutionHours = 12.5,
            });

        mock.Setup(m => m.GetCategoryBreakdownAsync(It.IsAny<DashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CategoryBreakdownDto>
            {
                new() { CategoryId = Guid.NewGuid(), CategoryName = "Hardware", Count = 6 },
                new() { CategoryId = Guid.NewGuid(), CategoryName = "Software", Count = 4 },
            });

        mock.Setup(m => m.GetPriorityBreakdownAsync(It.IsAny<DashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PriorityBreakdownDto>
            {
                new() { PriorityId = Guid.NewGuid(), PriorityName = "Low", Count = 7 },
                new() { PriorityId = Guid.NewGuid(), PriorityName = "High", Count = 3 },
            });

        mock.Setup(m => m.GetMonthlyTicketsAsync(It.IsAny<DashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MonthlyTicketsDto>
            {
                new() { Year = 2026, Month = 1, MonthLabel = "Jan 2026", CreatedCount = 5, ResolvedCount = 3 },
                new() { Year = 2026, Month = 2, MonthLabel = "Feb 2026", CreatedCount = 5, ResolvedCount = 2 },
            });

        mock.Setup(m => m.GetResolutionTimeAsync(It.IsAny<DashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResolutionTimeDto
            {
                OverallAverageResolutionHours = 12.5,
                ByPriority =
                [
                    new PriorityResolutionTimeDto { PriorityName = "Low", AverageResolutionHours = 20, TicketCount = 5 },
                    new PriorityResolutionTimeDto { PriorityName = "High", AverageResolutionHours = 5, TicketCount = 2 },
                ],
            });

        mock.Setup(m => m.GetSlaReportAsync(It.IsAny<DashboardQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SlaReportDto
            {
                TotalTrackedTickets = 4,
                MetCount = 3,
                BreachedCount = 1,
                CompliancePercentage = 75,
                BreachedTickets =
                [
                    new SlaBreachEntryDto
                    {
                        TicketId = Guid.NewGuid(),
                        TicketNumber = "HD-000042",
                        Title = "Overdue ticket",
                        DueDate = DateTime.UtcNow.AddDays(-2),
                        StatusName = "Open",
                        HoursOverdue = 48,
                    },
                ],
            });

        return mock;
    }

    [Fact]
    public async Task GenerateDashboardPdfAsync_ProducesValidPdfBytes()
    {
        var sut = new ReportService(MockDashboardService().Object);

        var bytes = await sut.GenerateDashboardPdfAsync(new DashboardQueryParameters());

        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task GenerateDashboardExcelAsync_ProducesValidWorkbookWithExpectedSheetsAndData()
    {
        var sut = new ReportService(MockDashboardService().Object);

        var bytes = await sut.GenerateDashboardExcelAsync(new DashboardQueryParameters());

        Assert.NotEmpty(bytes);
        Assert.Equal(0x50, bytes[0]); // 'P'
        Assert.Equal(0x4B, bytes[1]); // 'K' — xlsx is a zip archive

        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var sheetNames = workbook.Worksheets.Select(w => w.Name).ToList();

        Assert.Contains("KPI Summary", sheetNames);
        Assert.Contains("Tickets by Category", sheetNames);
        Assert.Contains("Tickets by Priority", sheetNames);
        Assert.Contains("Monthly Tickets", sheetNames);
        Assert.Contains("Resolution Time", sheetNames);
        Assert.Contains("SLA Report", sheetNames);

        var kpiSheet = workbook.Worksheet("KPI Summary");
        Assert.Equal("Total Tickets", kpiSheet.Cell(2, 1).GetString());
        Assert.Equal("10", kpiSheet.Cell(2, 2).GetString());

        var slaSheet = workbook.Worksheet("SLA Report");
        Assert.Equal("HD-000042", slaSheet.Cell(7, 1).GetString());
    }
}
