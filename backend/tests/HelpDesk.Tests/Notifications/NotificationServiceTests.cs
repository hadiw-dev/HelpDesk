using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HelpDesk.Tests.Notifications;

public class NotificationServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Mock<ICurrentUserService> MockCurrentUser(Guid userId)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(m => m.UserId).Returns(userId);
        return mock;
    }

    private async Task<ApplicationUser> AddUserAsync(AppDbContext db, string email = "")
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = string.IsNullOrEmpty(email) ? $"{Guid.NewGuid()}@helpdesk.local" : email,
            UserName = $"{Guid.NewGuid()}@helpdesk.local",
            FirstName = "Test",
            LastName = "User",
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static NotificationService CreateSut(AppDbContext db, Mock<ICurrentUserService> currentUser, Mock<IEmailSender>? emailSender = null) =>
        new(db, currentUser.Object, (emailSender ?? new Mock<IEmailSender>()).Object);

    [Fact]
    public async Task NotifyUserAsync_KnownUser_PersistsNotificationAndSendsEmail()
    {
        await using var db = CreateDbContext();
        var user = await AddUserAsync(db, "recipient@helpdesk.local");

        var emailSender = new Mock<IEmailSender>();
        var sut = CreateSut(db, MockCurrentUser(Guid.NewGuid()), emailSender);

        await sut.NotifyUserAsync(user.Id, "Ticket assigned", "Ticket HD-000001 assigned to you.", NotificationType.TicketAssigned, Guid.NewGuid());

        var stored = await db.Notifications.FirstOrDefaultAsync(n => n.UserId == user.Id);
        Assert.NotNull(stored);
        Assert.Equal(NotificationType.TicketAssigned, stored!.Type);
        Assert.False(stored.IsRead);

        emailSender.Verify(
            e => e.SendNotificationEmailAsync("recipient@helpdesk.local", "Ticket assigned", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyUserAsync_UnknownUser_DoesNothing()
    {
        await using var db = CreateDbContext();
        var emailSender = new Mock<IEmailSender>();
        var sut = CreateSut(db, MockCurrentUser(Guid.NewGuid()), emailSender);

        await sut.NotifyUserAsync(Guid.NewGuid(), "Title", "Message", NotificationType.System);

        Assert.Equal(0, await db.Notifications.CountAsync());
        emailSender.Verify(e => e.SendNotificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetForCurrentUserAsync_ReturnsOnlyCurrentUsersNotifications()
    {
        await using var db = CreateDbContext();
        var user = await AddUserAsync(db);
        var otherUser = await AddUserAsync(db);

        var sut = CreateSut(db, MockCurrentUser(user.Id));
        await sut.NotifyUserAsync(user.Id, "Mine", "For me", NotificationType.System);
        await sut.NotifyUserAsync(otherUser.Id, "Not mine", "For someone else", NotificationType.System);

        var result = await sut.GetForCurrentUserAsync(1, 20, unreadOnly: false);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Mine", result.Items[0].Title);
    }

    [Fact]
    public async Task GetForCurrentUserAsync_UnreadOnly_FiltersReadNotifications()
    {
        await using var db = CreateDbContext();
        var user = await AddUserAsync(db);

        var sut = CreateSut(db, MockCurrentUser(user.Id));
        await sut.NotifyUserAsync(user.Id, "First", "First message", NotificationType.System);
        await sut.NotifyUserAsync(user.Id, "Second", "Second message", NotificationType.System);

        var all = await sut.GetForCurrentUserAsync(1, 20, unreadOnly: false);
        await sut.MarkAsReadAsync(all.Items.First(i => i.Title == "First").Id);

        var unread = await sut.GetForCurrentUserAsync(1, 20, unreadOnly: true);

        Assert.Equal(1, unread.TotalCount);
        Assert.Equal("Second", unread.Items[0].Title);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        await using var db = CreateDbContext();
        var user = await AddUserAsync(db);

        var sut = CreateSut(db, MockCurrentUser(user.Id));
        await sut.NotifyUserAsync(user.Id, "First", "First message", NotificationType.System);
        await sut.NotifyUserAsync(user.Id, "Second", "Second message", NotificationType.System);

        Assert.Equal(2, await sut.GetUnreadCountAsync());
    }

    [Fact]
    public async Task MarkAsReadAsync_MarksNotificationReadAndStampsReadAt()
    {
        await using var db = CreateDbContext();
        var user = await AddUserAsync(db);

        var sut = CreateSut(db, MockCurrentUser(user.Id));
        await sut.NotifyUserAsync(user.Id, "Title", "Message", NotificationType.System);
        var notification = await db.Notifications.FirstAsync(n => n.UserId == user.Id);

        await sut.MarkAsReadAsync(notification.Id);

        var updated = await db.Notifications.FirstAsync(n => n.Id == notification.Id);
        Assert.True(updated.IsRead);
        Assert.NotNull(updated.ReadAt);
    }

    [Fact]
    public async Task MarkAsReadAsync_SomeoneElsesNotification_ThrowsForbiddenAppException()
    {
        await using var db = CreateDbContext();
        var owner = await AddUserAsync(db);
        var other = await AddUserAsync(db);

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id));
        await ownerSut.NotifyUserAsync(owner.Id, "Title", "Message", NotificationType.System);
        var notification = await db.Notifications.FirstAsync(n => n.UserId == owner.Id);

        var otherSut = CreateSut(db, MockCurrentUser(other.Id));

        await Assert.ThrowsAsync<ForbiddenAppException>(() => otherSut.MarkAsReadAsync(notification.Id));
    }

    [Fact]
    public async Task MarkAllAsReadAsync_MarksAllUnreadNotificationsAsRead()
    {
        await using var db = CreateDbContext();
        var user = await AddUserAsync(db);

        var sut = CreateSut(db, MockCurrentUser(user.Id));
        await sut.NotifyUserAsync(user.Id, "First", "First message", NotificationType.System);
        await sut.NotifyUserAsync(user.Id, "Second", "Second message", NotificationType.System);

        await sut.MarkAllAsReadAsync();

        Assert.Equal(0, await sut.GetUnreadCountAsync());
    }
}
