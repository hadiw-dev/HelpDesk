using HelpDesk.Application.Features.Admin.Settings.Dtos;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Admin;

public class SystemSettingsServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAsync_NoExistingRow_CreatesRowWithDefaults()
    {
        await using var db = CreateDbContext();
        var sut = new SystemSettingsService(db);

        var result = await sut.GetAsync();

        Assert.Equal("HelpDesk System", result.SiteName);
        Assert.Equal(1, await db.SystemSettings.CountAsync());
    }

    [Fact]
    public async Task GetAsync_CalledTwice_DoesNotCreateASecondRow()
    {
        await using var db = CreateDbContext();
        var sut = new SystemSettingsService(db);

        await sut.GetAsync();
        await sut.GetAsync();

        Assert.Equal(1, await db.SystemSettings.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        await using var db = CreateDbContext();
        var sut = new SystemSettingsService(db);

        var result = await sut.UpdateAsync(new UpdateSystemSettingsRequest
        {
            SiteName = "Acme Help Desk",
            MaxFileUploadSizeMb = 25,
            AllowedFileExtensions = ".pdf,.docx",
            DefaultPageSize = 50,
        });

        Assert.Equal("Acme Help Desk", result.SiteName);
        Assert.Equal(25, result.MaxFileUploadSizeMb);

        var reread = await sut.GetAsync();
        Assert.Equal("Acme Help Desk", reread.SiteName);
        Assert.Equal(".pdf,.docx", reread.AllowedFileExtensions);
    }
}
