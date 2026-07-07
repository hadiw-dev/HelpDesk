using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Features.Dashboard.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HelpDesk.Tests.Dashboard;

public class DashboardServiceTests
{
    private readonly Guid _hardwareCategoryId = Guid.NewGuid();
    private readonly Guid _softwareCategoryId = Guid.NewGuid();
    private readonly Guid _lowPriorityId = Guid.NewGuid();
    private readonly Guid _highPriorityId = Guid.NewGuid();
    private readonly Guid _openStatusId = Guid.NewGuid();
    private readonly Guid _inProgressStatusId = Guid.NewGuid();
    private readonly Guid _resolvedStatusId = Guid.NewGuid();
    private readonly Guid _closedStatusId = Guid.NewGuid();

    private AppDbContext CreateSeededDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);

        db.Categories.AddRange(
            new Category { Id = _hardwareCategoryId, Name = "Hardware", DisplayOrder = 1 },
            new Category { Id = _softwareCategoryId, Name = "Software", DisplayOrder = 2 });
        db.Priorities.AddRange(
            new Priority { Id = _lowPriorityId, Name = "Low", DisplayOrder = 1 },
            new Priority { Id = _highPriorityId, Name = "High", DisplayOrder = 2 });
        db.Statuses.AddRange(
            new Status { Id = _openStatusId, Name = "Open", DisplayOrder = 1 },
            new Status { Id = _inProgressStatusId, Name = "In Progress", DisplayOrder = 2 },
            new Status { Id = _resolvedStatusId, Name = "Resolved", DisplayOrder = 3 },
            new Status { Id = _closedStatusId, Name = "Closed", DisplayOrder = 4 });
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

    private async Task<ApplicationUser> AddUserAsync(AppDbContext db)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid()}@helpdesk.local",
            UserName = $"{Guid.NewGuid()}@helpdesk.local",
            FirstName = "Test",
            LastName = "User",
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// AppDbContext.SaveChanges always stamps CreatedAt = UtcNow for newly-Added entities (see
    /// ApplyAuditInformation), so historical CreatedAt values for test fixtures have to be applied
    /// as a second, separate update — at that point the entity is Modified, not Added, and
    /// ApplyAuditInformation leaves CreatedAt alone for Modified entries.
    /// </summary>
    private async Task<Ticket> AddTicketAsync(
        AppDbContext db,
        Guid createdByUserId,
        Guid statusId,
        Guid priorityId,
        Guid categoryId,
        DateTime createdAt,
        Guid? assignedToUserId = null,
        DateTime? dueDate = null,
        DateTime? resolvedAt = null,
        DateTime? closedAt = null)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = $"HD-{Guid.NewGuid():N}"[..10],
            Title = "Test ticket",
            Description = "Test description",
            CategoryId = categoryId,
            PriorityId = priorityId,
            StatusId = statusId,
            CreatedByUserId = createdByUserId,
            AssignedToUserId = assignedToUserId,
            DueDate = dueDate,
            ResolvedAt = resolvedAt,
            ClosedAt = closedAt,
        };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        ticket.CreatedAt = createdAt;
        await db.SaveChangesAsync();

        return ticket;
    }

    [Fact]
    public async Task GetKpiSummaryAsync_ComputesCountsAndAverageResolution()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var agent = await AddUserAsync(db);
        var now = DateTime.UtcNow;

        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now.AddDays(-5));
        await AddTicketAsync(
            db, owner.Id, _resolvedStatusId, _lowPriorityId, _hardwareCategoryId, now.AddDays(-10),
            resolvedAt: now.AddDays(-8)); // 48h resolution
        await AddTicketAsync(db, owner.Id, _closedStatusId, _highPriorityId, _softwareCategoryId, now.AddDays(-3), closedAt: now.AddDays(-1));
        await AddTicketAsync(db, owner.Id, _openStatusId, _highPriorityId, _softwareCategoryId, now.AddDays(-2), dueDate: now.AddDays(-1)); // overdue, unassigned
        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now.AddDays(-1), assignedToUserId: agent.Id);

        var sut = new DashboardService(db, MockCurrentUser(owner.Id, "Employee").Object);
        var result = await sut.GetKpiSummaryAsync(new DashboardQueryParameters());

        Assert.Equal(5, result.TotalTickets);
        Assert.Equal(3, result.OpenTickets);
        Assert.Equal(1, result.ResolvedTickets);
        Assert.Equal(1, result.ClosedTickets);
        Assert.Equal(1, result.OverdueTickets);
        Assert.Equal(3, result.UnassignedTickets); // Ticket1 (Open), Ticket2 (Resolved), Ticket4 (Open) — Closed is the only status excluded
        Assert.NotNull(result.AverageResolutionHours);
        Assert.Equal(48, result.AverageResolutionHours!.Value, precision: 0);
    }

    [Fact]
    public async Task GetKpiSummaryAsync_Employee_OnlySeesOwnTickets()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var other = await AddUserAsync(db);
        var now = DateTime.UtcNow;

        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now);
        await AddTicketAsync(db, other.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now);

        var sut = new DashboardService(db, MockCurrentUser(owner.Id, "Employee").Object);
        var result = await sut.GetKpiSummaryAsync(new DashboardQueryParameters());

        Assert.Equal(1, result.TotalTickets);
    }

    [Fact]
    public async Task GetKpiSummaryAsync_Agent_SeesAllTickets()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var other = await AddUserAsync(db);
        var agent = await AddUserAsync(db);
        var now = DateTime.UtcNow;

        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now);
        await AddTicketAsync(db, other.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now);

        var sut = new DashboardService(db, MockCurrentUser(agent.Id, "IT Support Agent").Object);
        var result = await sut.GetKpiSummaryAsync(new DashboardQueryParameters());

        Assert.Equal(2, result.TotalTickets);
    }

    [Fact]
    public async Task GetCategoryBreakdownAsync_GroupsAndOrdersByDisplayOrder()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var now = DateTime.UtcNow;

        // Insert Software first to prove ordering comes from DisplayOrder, not insertion/alphabetical order.
        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _softwareCategoryId, now);
        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now);
        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now);

        var sut = new DashboardService(db, MockCurrentUser(owner.Id, "IT Support Agent").Object);
        var result = await sut.GetCategoryBreakdownAsync(new DashboardQueryParameters());

        Assert.Equal(2, result.Count);
        Assert.Equal("Hardware", result[0].CategoryName);
        Assert.Equal(2, result[0].Count);
        Assert.Equal("Software", result[1].CategoryName);
        Assert.Equal(1, result[1].Count);
    }

    [Fact]
    public async Task GetPriorityBreakdownAsync_GroupsAndOrdersByDisplayOrder()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var now = DateTime.UtcNow;

        await AddTicketAsync(db, owner.Id, _openStatusId, _highPriorityId, _hardwareCategoryId, now);
        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now);

        var sut = new DashboardService(db, MockCurrentUser(owner.Id, "IT Support Agent").Object);
        var result = await sut.GetPriorityBreakdownAsync(new DashboardQueryParameters());

        Assert.Equal(2, result.Count);
        Assert.Equal("Low", result[0].PriorityName);
        Assert.Equal("High", result[1].PriorityName);
    }

    [Fact]
    public async Task GetMonthlyTicketsAsync_GroupsByCreatedMonthChronologically()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);

        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc));
        await AddTicketAsync(
            db, owner.Id, _resolvedStatusId, _lowPriorityId, _hardwareCategoryId, new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
            resolvedAt: new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc));

        var sut = new DashboardService(db, MockCurrentUser(owner.Id, "IT Support Agent").Object);
        var result = await sut.GetMonthlyTicketsAsync(new DashboardQueryParameters());

        Assert.Equal(2, result.Count);
        Assert.Equal(2026, result[0].Year);
        Assert.Equal(1, result[0].Month);
        Assert.Equal(2, result[0].CreatedCount);
        Assert.Equal(0, result[0].ResolvedCount);
        Assert.Equal(2, result[1].Month);
        Assert.Equal(1, result[1].CreatedCount);
        Assert.Equal(1, result[1].ResolvedCount);
    }

    [Fact]
    public async Task GetResolutionTimeAsync_ComputesOverallAndByPriority()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var now = DateTime.UtcNow;

        await AddTicketAsync(
            db, owner.Id, _resolvedStatusId, _lowPriorityId, _hardwareCategoryId, now.AddHours(-24),
            resolvedAt: now); // 24h
        await AddTicketAsync(
            db, owner.Id, _resolvedStatusId, _highPriorityId, _hardwareCategoryId, now.AddHours(-4),
            resolvedAt: now); // 4h
        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now); // unresolved, excluded

        var sut = new DashboardService(db, MockCurrentUser(owner.Id, "IT Support Agent").Object);
        var result = await sut.GetResolutionTimeAsync(new DashboardQueryParameters());

        Assert.Equal(14, result.OverallAverageResolutionHours!.Value, precision: 0);
        Assert.Equal(2, result.ByPriority.Count);
        Assert.Equal("Low", result.ByPriority[0].PriorityName);
        Assert.Equal(24, result.ByPriority[0].AverageResolutionHours!.Value, precision: 0);
        Assert.Equal("High", result.ByPriority[1].PriorityName);
        Assert.Equal(4, result.ByPriority[1].AverageResolutionHours!.Value, precision: 0);
    }

    [Fact]
    public async Task GetSlaReportAsync_ClassifiesMetAndBreachedTickets()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var now = DateTime.UtcNow;

        // Met: resolved before due date.
        await AddTicketAsync(
            db, owner.Id, _resolvedStatusId, _lowPriorityId, _hardwareCategoryId, now.AddDays(-3),
            dueDate: now.AddDays(-1), resolvedAt: now.AddDays(-2));
        // Breached: resolved after due date.
        var breachedResolved = await AddTicketAsync(
            db, owner.Id, _resolvedStatusId, _lowPriorityId, _hardwareCategoryId, now.AddDays(-3),
            dueDate: now.AddDays(-2), resolvedAt: now.AddHours(-1));
        // Breached: still open, past due date.
        var breachedOpen = await AddTicketAsync(
            db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now.AddDays(-5),
            dueDate: now.AddDays(-1));
        // Met (on track): still open, due date in the future.
        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now, dueDate: now.AddDays(3));
        // Not tracked: no due date at all.
        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, now);

        var sut = new DashboardService(db, MockCurrentUser(owner.Id, "IT Support Agent").Object);
        var result = await sut.GetSlaReportAsync(new DashboardQueryParameters());

        Assert.Equal(4, result.TotalTrackedTickets);
        Assert.Equal(2, result.MetCount);
        Assert.Equal(2, result.BreachedCount);
        Assert.Equal(50, result.CompliancePercentage);
        Assert.Contains(result.BreachedTickets, b => b.TicketId == breachedResolved.Id);
        Assert.Contains(result.BreachedTickets, b => b.TicketId == breachedOpen.Id);
    }

    [Fact]
    public async Task GetKpiSummaryAsync_DateRangeFilter_OnlyIncludesTicketsWithinRange()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);

        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        await AddTicketAsync(db, owner.Id, _openStatusId, _lowPriorityId, _hardwareCategoryId, new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc));

        var sut = new DashboardService(db, MockCurrentUser(owner.Id, "IT Support Agent").Object);
        var result = await sut.GetKpiSummaryAsync(new DashboardQueryParameters
        {
            DateFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            DateTo = new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc),
        });

        Assert.Equal(1, result.TotalTickets);
    }
}
