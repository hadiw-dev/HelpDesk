using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Features.Comments.Dtos;
using HelpDesk.Application.Features.Notifications.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HelpDesk.Tests.Comments;

public class CommentServiceTests
{
    private readonly Guid _categoryId = Guid.NewGuid();
    private readonly Guid _priorityId = Guid.NewGuid();
    private readonly Guid _openStatusId = Guid.NewGuid();

    private AppDbContext CreateSeededDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);

        db.Categories.Add(new Category { Id = _categoryId, Name = "Hardware", DisplayOrder = 1 });
        db.Priorities.Add(new Priority { Id = _priorityId, Name = "Low", DisplayOrder = 1 });
        db.Statuses.Add(new Status { Id = _openStatusId, Name = "Open", DisplayOrder = 1 });
        db.SaveChanges();

        return db;
    }

    private static Mock<ICurrentUserService> MockCurrentUser(Guid userId, params string[] roles)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(m => m.UserId).Returns(userId);
        mock.Setup(m => m.Roles).Returns(roles);
        return mock;
    }

    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private async Task<ApplicationUser> AddUserAsync(AppDbContext db, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid()}@helpdesk.local",
            UserName = $"{Guid.NewGuid()}@helpdesk.local",
            FirstName = firstName,
            LastName = lastName,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private async Task<Ticket> AddTicketAsync(AppDbContext db, Guid createdByUserId, Guid? assignedToUserId = null)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = "HD-000001",
            Title = "Laptop won't boot",
            Description = "Black screen on startup.",
            CategoryId = _categoryId,
            PriorityId = _priorityId,
            StatusId = _openStatusId,
            CreatedByUserId = createdByUserId,
            AssignedToUserId = assignedToUserId,
        };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
        return ticket;
    }

    private static CommentService CreateSut(
        AppDbContext db,
        Mock<ICurrentUserService> currentUser,
        Mock<UserManager<ApplicationUser>>? userManager = null,
        Mock<INotificationService>? notificationService = null,
        Mock<IActivityLogService>? activityLog = null)
    {
        return new CommentService(
            db,
            currentUser.Object,
            (activityLog ?? new Mock<IActivityLogService>()).Object,
            (notificationService ?? new Mock<INotificationService>()).Object,
            (userManager ?? MockUserManager()).Object);
    }

    [Fact]
    public async Task AddAsync_PublicCommentAsOwner_Succeeds()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var ticket = await AddTicketAsync(db, owner.Id);

        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));

        var result = await sut.AddAsync(ticket.Id, new CreateCommentRequest { Content = "Any update?", IsInternal = false });

        Assert.False(result.IsInternal);
        Assert.Equal("Owner User", result.AuthorName);
    }

    [Fact]
    public async Task AddAsync_InternalNoteAsEmployee_ThrowsForbiddenAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var ticket = await AddTicketAsync(db, owner.Id);

        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));

        await Assert.ThrowsAsync<ForbiddenAppException>(
            () => sut.AddAsync(ticket.Id, new CreateCommentRequest { Content = "Trying to sneak a note", IsInternal = true }));
    }

    [Fact]
    public async Task AddAsync_InternalNoteAsAgent_Succeeds()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "One");
        var ticket = await AddTicketAsync(db, owner.Id, agent.Id);

        var sut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));

        var result = await sut.AddAsync(ticket.Id, new CreateCommentRequest { Content = "Internal: escalate to network team", IsInternal = true });

        Assert.True(result.IsInternal);
    }

    [Fact]
    public async Task GetForTicketAsync_AsEmployee_ExcludesInternalNotes()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "One");
        var ticket = await AddTicketAsync(db, owner.Id, agent.Id);

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));
        await agentSut.AddAsync(ticket.Id, new CreateCommentRequest { Content = "Public update", IsInternal = false });
        await agentSut.AddAsync(ticket.Id, new CreateCommentRequest { Content = "Internal only", IsInternal = true });

        var employeeSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var comments = await employeeSut.GetForTicketAsync(ticket.Id);

        Assert.Single(comments);
        Assert.False(comments[0].IsInternal);
    }

    [Fact]
    public async Task GetForTicketAsync_AsAgent_IncludesInternalNotes()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "One");
        var ticket = await AddTicketAsync(db, owner.Id, agent.Id);

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));
        await agentSut.AddAsync(ticket.Id, new CreateCommentRequest { Content = "Public update", IsInternal = false });
        await agentSut.AddAsync(ticket.Id, new CreateCommentRequest { Content = "Internal only", IsInternal = true });

        var comments = await agentSut.GetForTicketAsync(ticket.Id);

        Assert.Equal(2, comments.Count);
    }

    [Fact]
    public async Task AddAsync_WithMention_NotifiesMentionedUserAndExtractsId()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "One");
        var ticket = await AddTicketAsync(db, owner.Id, agent.Id);

        var userManager = MockUserManager();
        userManager.Setup(m => m.FindByIdAsync(agent.Id.ToString())).ReturnsAsync(agent);
        userManager.Setup(m => m.GetRolesAsync(agent)).ReturnsAsync(["IT Support Agent"]);

        var notificationService = new Mock<INotificationService>();
        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), userManager, notificationService);

        var content = $"Hey @[Agent One]({agent.Id}), any update?";
        var result = await sut.AddAsync(ticket.Id, new CreateCommentRequest { Content = content, IsInternal = false });

        Assert.Contains(agent.Id, result.MentionedUserIds);

        notificationService.Verify(
            n => n.NotifyUserAsync(agent.Id, It.IsAny<string>(), It.IsAny<string>(), NotificationType.Mention, ticket.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddAsync_InternalNoteMentioningEmployee_DoesNotNotifyEmployee()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "One");
        var ticket = await AddTicketAsync(db, owner.Id, agent.Id);

        var userManager = MockUserManager();
        userManager.Setup(m => m.FindByIdAsync(owner.Id.ToString())).ReturnsAsync(owner);
        userManager.Setup(m => m.GetRolesAsync(owner)).ReturnsAsync(["Employee"]);

        var notificationService = new Mock<INotificationService>();
        var sut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"), userManager, notificationService);

        var content = $"Internal: don't tell @[Owner User]({owner.Id}) yet";
        await sut.AddAsync(ticket.Id, new CreateCommentRequest { Content = content, IsInternal = true });

        notificationService.Verify(
            n => n.NotifyUserAsync(owner.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddAsync_PublicComment_NotifiesOwnerAndAssigneeExcludingAuthor()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "One");
        var ticket = await AddTicketAsync(db, owner.Id, agent.Id);

        var notificationService = new Mock<INotificationService>();
        var sut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"), notificationService: notificationService);

        await sut.AddAsync(ticket.Id, new CreateCommentRequest { Content = "Working on it now.", IsInternal = false });

        notificationService.Verify(
            n => n.NotifyUserAsync(owner.Id, It.IsAny<string>(), It.IsAny<string>(), NotificationType.TicketCommented, ticket.Id, It.IsAny<CancellationToken>()),
            Times.Once);
        notificationService.Verify(
            n => n.NotifyUserAsync(agent.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
