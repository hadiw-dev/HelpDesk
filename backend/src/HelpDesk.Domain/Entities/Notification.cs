using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Identity;

namespace HelpDesk.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public Guid? RelatedTicketId { get; set; }
    public Ticket? RelatedTicket { get; set; }
}
