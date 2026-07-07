namespace HelpDesk.Application.Features.Comments.Dtos;

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
