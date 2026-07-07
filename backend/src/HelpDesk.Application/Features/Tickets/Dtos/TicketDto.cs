namespace HelpDesk.Application.Features.Tickets.Dtos;

public class TicketDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public Guid PriorityId { get; set; }
    public string PriorityName { get; set; } = string.Empty;

    public Guid StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;

    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
