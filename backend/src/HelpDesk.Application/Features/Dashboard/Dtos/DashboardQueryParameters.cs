namespace HelpDesk.Application.Features.Dashboard.Dtos;

/// <summary>Optional date range filter (applied to <c>Ticket.CreatedAt</c>) shared by every dashboard/report endpoint.</summary>
public class DashboardQueryParameters
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
