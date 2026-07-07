namespace HelpDesk.Application.Features.Admin.Settings.Dtos;

public class UpdateSystemSettingsRequest
{
    public string SiteName { get; set; } = string.Empty;
    public int MaxFileUploadSizeMb { get; set; }
    public string AllowedFileExtensions { get; set; } = string.Empty;
    public int DefaultPageSize { get; set; }
}
