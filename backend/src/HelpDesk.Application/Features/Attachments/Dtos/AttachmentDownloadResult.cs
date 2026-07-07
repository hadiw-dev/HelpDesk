namespace HelpDesk.Application.Features.Attachments.Dtos;

public class AttachmentDownloadResult
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
