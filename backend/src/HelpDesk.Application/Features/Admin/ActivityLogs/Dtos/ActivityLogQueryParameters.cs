namespace HelpDesk.Application.Features.Admin.ActivityLogs.Dtos;

public class ActivityLogQueryParameters
{
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
