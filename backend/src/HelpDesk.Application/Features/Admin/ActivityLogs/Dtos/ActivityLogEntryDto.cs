namespace HelpDesk.Application.Features.Admin.ActivityLogs.Dtos;

public class ActivityLogEntryDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = "System";
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
