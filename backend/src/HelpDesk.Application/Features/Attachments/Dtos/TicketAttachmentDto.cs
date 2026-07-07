namespace HelpDesk.Application.Features.Attachments.Dtos;

public class TicketAttachmentDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
