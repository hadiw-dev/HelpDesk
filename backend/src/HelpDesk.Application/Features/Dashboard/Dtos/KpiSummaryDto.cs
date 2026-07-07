namespace HelpDesk.Application.Features.Dashboard.Dtos;

public class KpiSummaryDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int PendingTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int OverdueTickets { get; set; }
    public int UnassignedTickets { get; set; }
    public double? AverageResolutionHours { get; set; }
}
