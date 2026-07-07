namespace HelpDesk.Application.Features.Attachments.Dtos;

/// <summary>
/// Framework-agnostic stand-in for an uploaded file (the Api layer maps <c>IFormFile</c> to this),
/// so the Application layer doesn't take a dependency on ASP.NET Core's HTTP types.
/// </summary>
public class UploadAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
    public Stream Content { get; set; } = Stream.Null;
}
