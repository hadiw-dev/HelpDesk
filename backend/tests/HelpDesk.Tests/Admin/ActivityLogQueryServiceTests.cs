using HelpDesk.Application.Features.Admin.ActivityLogs.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Admin;

public class ActivityLogQueryServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private async Task<ApplicationUser> AddUserAsync(AppDbContext db)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid()}@helpdesk.local",
            UserName = $"{Guid.NewGuid()}@helpdesk.local",
            FirstName = "Log",
            LastName = "User",
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task SearchAsync_ReturnsEntriesNewestFirstWithResolvedUserNames()
    {
        await using var db = CreateDbContext();
        var user = await AddUserAsync(db);

        db.ActivityLogs.Add(new ActivityLog { Id = Guid.NewGuid(), UserId = user.Id, Action = "Login", EntityName = "User" });
        await db.SaveChangesAsync();
        db.ActivityLogs.Add(new ActivityLog { Id = Guid.NewGuid(), UserId = user.Id, Action = "TicketCreated", EntityName = "Ticket" });
        await db.SaveChangesAsync();

        var sut = new ActivityLogQueryService(db);
        var result = await sut.SearchAsync(new ActivityLogQueryParameters());

        Assert.Equal(2, result.TotalCount);
        Assert.Equal("TicketCreated", result.Items[0].Action);
        Assert.Equal("Log User", result.Items[0].UserName);
    }

    [Fact]
    public async Task SearchAsync_FiltersByAction()
    {
        await using var db = CreateDbContext();
        var user = await AddUserAsync(db);

        db.ActivityLogs.Add(new ActivityLog { Id = Guid.NewGuid(), UserId = user.Id, Action = "Login", EntityName = "User" });
        db.ActivityLogs.Add(new ActivityLog { Id = Guid.NewGuid(), UserId = user.Id, Action = "TicketCreated", EntityName = "Ticket" });
        await db.SaveChangesAsync();

        var sut = new ActivityLogQueryService(db);
        var result = await sut.SearchAsync(new ActivityLogQueryParameters { Action = "Login" });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Login", result.Items[0].Action);
    }

    [Fact]
    public async Task SearchAsync_NullUserId_MapsToSystem()
    {
        await using var db = CreateDbContext();
        db.ActivityLogs.Add(new ActivityLog { Id = Guid.NewGuid(), UserId = null, Action = "SystemStartup", EntityName = "System" });
        await db.SaveChangesAsync();

        var sut = new ActivityLogQueryService(db);
        var result = await sut.SearchAsync(new ActivityLogQueryParameters());

        Assert.Equal("System", result.Items[0].UserName);
    }

    [Fact]
    public async Task SearchAsync_Pagination_ReturnsExpectedPageSize()
    {
        await using var db = CreateDbContext();
        var user = await AddUserAsync(db);

        for (var i = 0; i < 5; i++)
        {
            db.ActivityLogs.Add(new ActivityLog { Id = Guid.NewGuid(), UserId = user.Id, Action = $"Action{i}", EntityName = "Test" });
        }
        await db.SaveChangesAsync();

        var sut = new ActivityLogQueryService(db);
        var result = await sut.SearchAsync(new ActivityLogQueryParameters { Page = 2, PageSize = 2 });

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalPages);
    }
}
