namespace HelpDesk.Application.Features.Dashboard.Dtos;

/// <summary>
/// SLA compliance is measured against each ticket's own <c>DueDate</c> (set at creation), not a fixed
/// per-priority policy table — this phase doesn't introduce configurable SLA policies, it reports
/// against whatever due date was actually committed to. Tickets with no due date are not SLA-tracked.
/// </summary>
public class SlaReportDto
{
    public int TotalTrackedTickets { get; set; }
    public int MetCount { get; set; }
    public int BreachedCount { get; set; }
    public double CompliancePercentage { get; set; }
    public IReadOnlyList<SlaBreachEntryDto> BreachedTickets { get; set; } = [];
}

public class SlaBreachEntryDto
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public double HoursOverdue { get; set; }
}
