namespace HelpDesk.Application.Features.Comments.Dtos;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public IReadOnlyList<Guid> MentionedUserIds { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
