using HelpDesk.Domain.Common;
using HelpDesk.Domain.Identity;

namespace HelpDesk.Domain.Entities;

public class TicketComment : BaseEntity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
