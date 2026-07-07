using AutoMapper;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Features.Assignments.Dtos;
using HelpDesk.Application.Features.Notifications.Interfaces;
using HelpDesk.Application.Features.Tickets.Dtos;
using HelpDesk.Application.Features.Tickets.Mappings;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HelpDesk.Tests.Assignments;

public class AssignmentServiceTests
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

    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<TicketMappingProfile>());
        return configuration.CreateMapper();
    }

    private async Task<ApplicationUser> AddUserAsync(AppDbContext db, string firstName, string lastName, bool isActive = true)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid()}@helpdesk.local",
            UserName = $"{Guid.NewGuid()}@helpdesk.local",
            FirstName = firstName,
            LastName = lastName,
            IsActive = isActive,
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

    private AssignmentService CreateSut(
        AppDbContext db,
        Mock<ICurrentUserService> currentUser,
        Mock<UserManager<ApplicationUser>> userManager,
        Mock<INotificationService>? notificationService = null,
        Mock<IActivityLogService>? activityLog = null)
    {
        var ticketService = new TicketService(db, CreateMapper(), currentUser.Object, (activityLog ?? new Mock<IActivityLogService>()).Object);

        return new AssignmentService(
            db,
            currentUser.Object,
            (activityLog ?? new Mock<IActivityLogService>()).Object,
            (notificationService ?? new Mock<INotificationService>()).Object,
            ticketService,
            userManager.Object);
    }

    [Fact]
    public async Task AssignAsync_AsAgent_AssignsTicketAndRecordsHistoryAndNotifies()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "One");
        var ticket = await AddTicketAsync(db, owner.Id);

        var userManager = MockUserManager();
        userManager.Setup(m => m.FindByIdAsync(agent.Id.ToString())).ReturnsAsync(agent);
        userManager.Setup(m => m.GetRolesAsync(agent)).ReturnsAsync(["IT Support Agent"]);

        var notificationService = new Mock<INotificationService>();
        var sut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"), userManager, notificationService);

        var result = await sut.AssignAsync(ticket.Id, new AssignTicketRequest { AssignedToUserId = agent.Id });

        Assert.Equal(agent.Id, result.AssignedToUserId);

        var historyRow = await db.TicketHistories.FirstOrDefaultAsync(h => h.TicketId == ticket.Id && h.FieldName == "AssignedTo");
        Assert.NotNull(historyRow);
        Assert.Equal("Unassigned", historyRow!.OldValue);

        var assignmentRow = await db.TicketAssignments.FirstOrDefaultAsync(a => a.TicketId == ticket.Id);
        Assert.NotNull(assignmentRow);
        Assert.Equal(AssignmentType.Manual, assignmentRow!.AssignmentType);
        Assert.Equal(agent.Id, assignmentRow.AssignedByUserId);

        notificationService.Verify(
            n => n.NotifyUserAsync(agent.Id, It.IsAny<string>(), It.IsAny<string>(), NotificationType.TicketAssigned, ticket.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AssignAsync_ToUnprivilegedUser_ThrowsValidationAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "One");
        var employee = await AddUserAsync(db, "Employee", "Two");
        var ticket = await AddTicketAsync(db, owner.Id);

        var userManager = MockUserManager();
        userManager.Setup(m => m.FindByIdAsync(employee.Id.ToString())).ReturnsAsync(employee);
        userManager.Setup(m => m.GetRolesAsync(employee)).ReturnsAsync(["Employee"]);

        var sut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"), userManager);

        await Assert.ThrowsAsync<ValidationAppException>(
            () => sut.AssignAsync(ticket.Id, new AssignTicketRequest { AssignedToUserId = employee.Id }));
    }

    [Fact]
    public async Task AssignAsync_AsEmployee_ThrowsForbiddenAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var ticket = await AddTicketAsync(db, owner.Id);

        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), MockUserManager());

        await Assert.ThrowsAsync<ForbiddenAppException>(
            () => sut.AssignAsync(ticket.Id, new AssignTicketRequest { AssignedToUserId = owner.Id }));
    }

    [Fact]
    public async Task AssignAsync_Unassign_ClearsAssigneeAndRecordsHistory()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "One");
        var ticket = await AddTicketAsync(db, owner.Id, agent.Id);

        var sut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"), MockUserManager());

        var result = await sut.AssignAsync(ticket.Id, new AssignTicketRequest { AssignedToUserId = null });

        Assert.Null(result.AssignedToUserId);

        var historyRow = await db.TicketHistories.FirstOrDefaultAsync(h => h.TicketId == ticket.Id && h.FieldName == "AssignedTo");
        Assert.NotNull(historyRow);
        Assert.Equal("Unassigned", historyRow!.NewValue);
    }

    [Fact]
    public async Task AutoAssignAsync_RotatesThroughEligibleAgentsInOrder()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent1 = await AddUserAsync(db, "Agent", "Alpha");
        var agent2 = await AddUserAsync(db, "Agent", "Beta");
        var orderedAgentIds = new[] { agent1.Id, agent2.Id }.OrderBy(id => id).ToList();

        var ticket1 = await AddTicketAsync(db, owner.Id);
        var ticket2 = await AddTicketAsync(db, owner.Id);

        var userManager = MockUserManager();
        userManager.Setup(m => m.GetUsersInRoleAsync("IT Support Agent")).ReturnsAsync([agent1, agent2]);

        var sut = CreateSut(db, MockCurrentUser(agent1.Id, "IT Support Agent"), userManager);

        var firstResult = await sut.AutoAssignAsync(ticket1.Id);
        var secondResult = await sut.AutoAssignAsync(ticket2.Id);

        Assert.Equal(orderedAgentIds[0], firstResult.AssignedToUserId);
        Assert.Equal(orderedAgentIds[1], secondResult.AssignedToUserId);
    }

    [Fact]
    public async Task AutoAssignAsync_NoEligibleAgents_ThrowsValidationAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "One");
        var ticket = await AddTicketAsync(db, owner.Id);

        var userManager = MockUserManager();
        userManager.Setup(m => m.GetUsersInRoleAsync("IT Support Agent")).ReturnsAsync([]);

        var sut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"), userManager);

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.AutoAssignAsync(ticket.Id));
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsEntriesNewestFirst()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent1 = await AddUserAsync(db, "Agent", "Alpha");
        var agent2 = await AddUserAsync(db, "Agent", "Beta");
        var ticket = await AddTicketAsync(db, owner.Id);

        var userManager = MockUserManager();
        userManager.Setup(m => m.FindByIdAsync(agent1.Id.ToString())).ReturnsAsync(agent1);
        userManager.Setup(m => m.GetRolesAsync(agent1)).ReturnsAsync(["IT Support Agent"]);
        userManager.Setup(m => m.FindByIdAsync(agent2.Id.ToString())).ReturnsAsync(agent2);
        userManager.Setup(m => m.GetRolesAsync(agent2)).ReturnsAsync(["IT Support Agent"]);

        var sut = CreateSut(db, MockCurrentUser(agent1.Id, "IT Support Agent"), userManager);

        await sut.AssignAsync(ticket.Id, new AssignTicketRequest { AssignedToUserId = agent1.Id });
        await sut.AssignAsync(ticket.Id, new AssignTicketRequest { AssignedToUserId = agent2.Id });

        var history = await sut.GetHistoryAsync(ticket.Id);

        Assert.Equal(2, history.Count);
        Assert.Equal("Agent Beta", history[0].AssignedToName);
        Assert.Equal("Agent Alpha", history[1].AssignedToName);
    }
}
