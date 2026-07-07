namespace HelpDesk.Application.Features.Tickets.Dtos;

public class CreateTicketRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid PriorityId { get; set; }
    public DateTime? DueDate { get; set; }
}
