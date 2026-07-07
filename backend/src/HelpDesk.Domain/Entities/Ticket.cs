using HelpDesk.Domain.Common;
using HelpDesk.Domain.Identity;

namespace HelpDesk.Domain.Entities;

public class Ticket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid PriorityId { get; set; }
    public Priority Priority { get; set; } = null!;

    public Guid StatusId { get; set; }
    public Status Status { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public Guid? AssignedToUserId { get; set; }
    public ApplicationUser? AssignedToUser { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    public ICollection<TicketHistory> HistoryEntries { get; set; } = new List<TicketHistory>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<TicketAssignment> Assignments { get; set; } = new List<TicketAssignment>();
}
