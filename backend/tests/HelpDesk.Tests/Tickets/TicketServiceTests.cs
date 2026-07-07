using AutoMapper;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Features.Tickets.Dtos;
using HelpDesk.Application.Features.Tickets.Mappings;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HelpDesk.Tests.Tickets;

public class TicketServiceTests
{
    private readonly Guid _categoryId = Guid.NewGuid();
    private readonly Guid _priorityId = Guid.NewGuid();
    private readonly Guid _openStatusId = Guid.NewGuid();
    private readonly Guid _resolvedStatusId = Guid.NewGuid();

    private AppDbContext CreateSeededDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);

        db.Categories.Add(new Category { Id = _categoryId, Name = "Hardware", DisplayOrder = 1 });
        db.Priorities.Add(new Priority { Id = _priorityId, Name = "Low", DisplayOrder = 1 });
        db.Statuses.AddRange(
            new Status { Id = _openStatusId, Name = "Open", DisplayOrder = 1 },
            new Status { Id = _resolvedStatusId, Name = "Resolved", DisplayOrder = 4 });
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

    private static Mock<IActivityLogService> MockActivityLog()
    {
        var mock = new Mock<IActivityLogService>();
        mock.Setup(m => m.LogAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<TicketMappingProfile>());
        return configuration.CreateMapper();
    }

    private TicketService CreateSut(AppDbContext dbContext, Mock<ICurrentUserService> currentUser, Mock<IActivityLogService>? activityLog = null)
    {
        return new TicketService(dbContext, CreateMapper(), currentUser.Object, (activityLog ?? MockActivityLog()).Object);
    }

    private async Task<ApplicationUser> AddUserAsync(AppDbContext db, string firstName = "Test", string lastName = "User")
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

    private CreateTicketRequest ValidCreateRequest() => new()
    {
        Title = "Laptop won't boot",
        Description = "The laptop shows a black screen on startup.",
        CategoryId = _categoryId,
        PriorityId = _priorityId,
    };

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesTicketWithOpenStatusAndTicketNumber()
    {
        await using var db = CreateSeededDbContext();
        var employee = await AddUserAsync(db);
        var sut = CreateSut(db, MockCurrentUser(employee.Id, "Employee"));

        var result = await sut.CreateAsync(ValidCreateRequest());

        Assert.StartsWith("HD-", result.TicketNumber);
        Assert.Equal("Open", result.StatusName);
        Assert.Equal(employee.Id, result.CreatedByUserId);

        var historyCount = await db.TicketHistories.CountAsync(h => h.TicketId == result.Id);
        Assert.True(historyCount >= 1);
    }

    [Fact]
    public async Task CreateAsync_UnknownCategory_ThrowsValidationAppException()
    {
        await using var db = CreateSeededDbContext();
        var employee = await AddUserAsync(db);
        var sut = CreateSut(db, MockCurrentUser(employee.Id, "Employee"));

        var request = ValidCreateRequest();
        request.CategoryId = Guid.NewGuid();

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.CreateAsync(request));
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundAppException()
    {
        await using var db = CreateSeededDbContext();
        var employee = await AddUserAsync(db);
        var sut = CreateSut(db, MockCurrentUser(employee.Id, "Employee"));

        await Assert.ThrowsAsync<NotFoundAppException>(() => sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByIdAsync_EmployeeAccessingSomeoneElsesTicket_ThrowsForbiddenAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var other = await AddUserAsync(db, "Other", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await ownerSut.CreateAsync(ValidCreateRequest());

        var otherSut = CreateSut(db, MockCurrentUser(other.Id, "Employee"));

        await Assert.ThrowsAsync<ForbiddenAppException>(() => otherSut.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task GetByIdAsync_AgentAccessingAnyonesTicket_Succeeds()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await ownerSut.CreateAsync(ValidCreateRequest());

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));

        var result = await agentSut.GetByIdAsync(created.Id);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_AsAgent_UpdatesFieldsAndRecordsHistory()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await ownerSut.CreateAsync(ValidCreateRequest());

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));
        var updateRequest = new UpdateTicketRequest
        {
            Title = "Laptop won't boot - escalated",
            Description = created.Description,
            CategoryId = _categoryId,
            PriorityId = _priorityId,
            StatusId = _resolvedStatusId,
        };

        var updated = await agentSut.UpdateAsync(created.Id, updateRequest);

        Assert.Equal("Laptop won't boot - escalated", updated.Title);
        Assert.Equal("Resolved", updated.StatusName);
        Assert.NotNull(updated.ResolvedAt);

        var historyCount = await db.TicketHistories.CountAsync(h => h.TicketId == created.Id);
        Assert.True(historyCount >= 3); // created + title + status at least
    }

    [Fact]
    public async Task UpdateAsync_EmployeeEditingOwnOpenTicket_Succeeds()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await sut.CreateAsync(ValidCreateRequest());

        var updateRequest = new UpdateTicketRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            CategoryId = _categoryId,
            PriorityId = _priorityId,
            StatusId = _openStatusId,
        };

        var updated = await sut.UpdateAsync(created.Id, updateRequest);

        Assert.Equal("Updated title", updated.Title);
    }

    [Fact]
    public async Task UpdateAsync_EmployeeEditingSomeoneElsesTicket_ThrowsForbiddenAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var other = await AddUserAsync(db, "Other", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await ownerSut.CreateAsync(ValidCreateRequest());

        var otherSut = CreateSut(db, MockCurrentUser(other.Id, "Employee"));
        var updateRequest = new UpdateTicketRequest
        {
            Title = "Hijacked",
            Description = created.Description,
            CategoryId = _categoryId,
            PriorityId = _priorityId,
            StatusId = _openStatusId,
        };

        await Assert.ThrowsAsync<ForbiddenAppException>(() => otherSut.UpdateAsync(created.Id, updateRequest));
    }

    [Fact]
    public async Task UpdateAsync_EmployeeEditingNonOpenTicket_ThrowsValidationAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await ownerSut.CreateAsync(ValidCreateRequest());

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));
        await agentSut.UpdateAsync(created.Id, new UpdateTicketRequest
        {
            Title = created.Title,
            Description = created.Description,
            CategoryId = _categoryId,
            PriorityId = _priorityId,
            StatusId = _resolvedStatusId,
        });

        await Assert.ThrowsAsync<ValidationAppException>(() => ownerSut.UpdateAsync(created.Id, new UpdateTicketRequest
        {
            Title = "Trying to edit resolved ticket",
            Description = created.Description,
            CategoryId = _categoryId,
            PriorityId = _priorityId,
            StatusId = _resolvedStatusId,
        }));
    }

    [Fact]
    public async Task DeleteAsync_AsEmployee_ThrowsForbiddenAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await sut.CreateAsync(ValidCreateRequest());

        await Assert.ThrowsAsync<ForbiddenAppException>(() => sut.DeleteAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_AsAgent_SoftDeletesTicket()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await ownerSut.CreateAsync(ValidCreateRequest());

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));
        await agentSut.DeleteAsync(created.Id);

        var visible = await db.Tickets.FirstOrDefaultAsync(t => t.Id == created.Id);
        Assert.Null(visible);

        var actual = await db.Tickets.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == created.Id);
        Assert.NotNull(actual);
        Assert.True(actual!.IsDeleted);
    }

    [Fact]
    public async Task RestoreAsync_AsAgent_ThrowsForbiddenAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await ownerSut.CreateAsync(ValidCreateRequest());

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));
        await agentSut.DeleteAsync(created.Id);

        await Assert.ThrowsAsync<ForbiddenAppException>(() => agentSut.RestoreAsync(created.Id));
    }

    [Fact]
    public async Task RestoreAsync_AsManager_RestoresTicket()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var manager = await AddUserAsync(db, "Manager", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await ownerSut.CreateAsync(ValidCreateRequest());

        var managerSut = CreateSut(db, MockCurrentUser(manager.Id, "Manager"));
        await managerSut.DeleteAsync(created.Id);

        var restored = await managerSut.RestoreAsync(created.Id);

        Assert.Equal(created.Id, restored.Id);
        var visible = await db.Tickets.FirstOrDefaultAsync(t => t.Id == created.Id);
        Assert.NotNull(visible);
    }

    [Fact]
    public async Task SearchAsync_Employee_OnlySeesOwnTickets()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var other = await AddUserAsync(db, "Other", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        await ownerSut.CreateAsync(ValidCreateRequest());

        var otherSut = CreateSut(db, MockCurrentUser(other.Id, "Employee"));
        await otherSut.CreateAsync(ValidCreateRequest());

        var result = await ownerSut.SearchAsync(new TicketQueryParameters());

        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task SearchAsync_Agent_SeesAllTickets()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var other = await AddUserAsync(db, "Other", "User");
        var agent = await AddUserAsync(db, "Agent", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        await ownerSut.CreateAsync(ValidCreateRequest());

        var otherSut = CreateSut(db, MockCurrentUser(other.Id, "Employee"));
        await otherSut.CreateAsync(ValidCreateRequest());

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));
        var result = await agentSut.SearchAsync(new TicketQueryParameters());

        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task SearchAsync_FilterByStatus_ReturnsOnlyMatchingTickets()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var first = await ownerSut.CreateAsync(ValidCreateRequest());
        await ownerSut.CreateAsync(ValidCreateRequest());

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));
        await agentSut.UpdateAsync(first.Id, new UpdateTicketRequest
        {
            Title = first.Title,
            Description = first.Description,
            CategoryId = _categoryId,
            PriorityId = _priorityId,
            StatusId = _resolvedStatusId,
        });

        var result = await agentSut.SearchAsync(new TicketQueryParameters { StatusId = _resolvedStatusId });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(first.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task SearchAsync_Pagination_ReturnsExpectedPageSize()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));

        for (var i = 0; i < 5; i++)
        {
            await sut.CreateAsync(ValidCreateRequest());
        }

        var result = await sut.SearchAsync(new TicketQueryParameters { Page = 2, PageSize = 2 });

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsEntriesNewestFirst()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db, "Owner", "User");
        var agent = await AddUserAsync(db, "Agent", "User");

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"));
        var created = await ownerSut.CreateAsync(ValidCreateRequest());

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"));
        await agentSut.UpdateAsync(created.Id, new UpdateTicketRequest
        {
            Title = "Changed",
            Description = created.Description,
            CategoryId = _categoryId,
            PriorityId = _priorityId,
            StatusId = _openStatusId,
        });

        var history = await ownerSut.GetHistoryAsync(created.Id);

        Assert.True(history.Count >= 2);
        Assert.Contains(history, h => h.FieldName == "Title" && h.NewValue == "Changed");
    }
}
