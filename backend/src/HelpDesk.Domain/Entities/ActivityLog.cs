using HelpDesk.Domain.Common;
using HelpDesk.Domain.Identity;

namespace HelpDesk.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}
