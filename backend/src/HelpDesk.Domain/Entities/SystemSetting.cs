using HelpDesk.Domain.Common;

namespace HelpDesk.Domain.Entities;

/// <summary>Singleton row of system-wide, admin-configurable settings.</summary>
public class SystemSetting : BaseEntity
{
    public string SiteName { get; set; } = "HelpDesk System";
    public int MaxFileUploadSizeMb { get; set; } = 10;
    public string AllowedFileExtensions { get; set; } = ".pdf,.png,.jpg,.jpeg,.docx,.xlsx,.txt,.zip";
    public int DefaultPageSize { get; set; } = 20;
}
