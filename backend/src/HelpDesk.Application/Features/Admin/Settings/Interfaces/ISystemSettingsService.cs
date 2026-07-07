using HelpDesk.Application.Features.Admin.Settings.Dtos;

namespace HelpDesk.Application.Features.Admin.Settings.Interfaces;

/// <summary>
/// Also consumed by <c>IAttachmentService</c> to enforce the current file-size/extension limits at
/// upload time — Settings isn't just a form that saves to nowhere, it drives real validation.
/// </summary>
public interface ISystemSettingsService
{
    Task<SystemSettingsDto> GetAsync(CancellationToken cancellationToken = default);

    Task<SystemSettingsDto> UpdateAsync(UpdateSystemSettingsRequest request, CancellationToken cancellationToken = default);
}
