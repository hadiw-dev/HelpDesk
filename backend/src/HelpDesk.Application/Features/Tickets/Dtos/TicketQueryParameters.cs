namespace HelpDesk.Application.Features.Tickets.Dtos;

public class TicketQueryParameters
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? PriorityId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? AssignedToUserId { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>One of: createdAt (default), title, priority, status, dueDate, ticketNumber.</summary>
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}
