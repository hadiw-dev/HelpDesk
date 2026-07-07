using HelpDesk.Domain.Common;
using HelpDesk.Domain.Identity;

namespace HelpDesk.Domain.Entities;

public class TicketHistory : BaseEntity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid? ChangedByUserId { get; set; }
    public ApplicationUser? ChangedByUser { get; set; }

    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
