using HelpDesk.Application.Features.Admin.Settings.Dtos;
using HelpDesk.Application.Features.Admin.Settings.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly AppDbContext _dbContext;

    public SystemSettingsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SystemSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateAsync(cancellationToken);
        return MapToDto(settings);
    }

    public async Task<SystemSettingsDto> UpdateAsync(UpdateSystemSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateAsync(cancellationToken);

        settings.SiteName = request.SiteName;
        settings.MaxFileUploadSizeMb = request.MaxFileUploadSizeMb;
        settings.AllowedFileExtensions = request.AllowedFileExtensions;
        settings.DefaultPageSize = request.DefaultPageSize;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(settings);
    }

    /// <summary>
    /// Production always has exactly one row via migration seed data; the InMemory provider used by
    /// unit tests doesn't apply that seed, so this also covers "no row yet" by creating one with defaults.
    /// </summary>
    private async Task<SystemSetting> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var settings = await _dbContext.SystemSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings is not null)
        {
            return settings;
        }

        settings = new SystemSetting { Id = Guid.NewGuid() };
        _dbContext.SystemSettings.Add(settings);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return settings;
    }

    private static SystemSettingsDto MapToDto(SystemSetting settings) => new()
    {
        SiteName = settings.SiteName,
        MaxFileUploadSizeMb = settings.MaxFileUploadSizeMb,
        AllowedFileExtensions = settings.AllowedFileExtensions,
        DefaultPageSize = settings.DefaultPageSize,
    };
}
