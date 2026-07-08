using System.Text;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Features.Admin.Settings.Dtos;
using HelpDesk.Application.Features.Admin.Settings.Interfaces;
using HelpDesk.Application.Features.Attachments.Dtos;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HelpDesk.Tests.Attachments;

public class AttachmentServiceTests
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

    private static Mock<ISystemSettingsService> MockSettings(int maxSizeMb = 5, string extensions = ".pdf,.png,.txt")
    {
        var mock = new Mock<ISystemSettingsService>();
        mock.Setup(m => m.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new SystemSettingsDto
        {
            SiteName = "Test",
            MaxFileUploadSizeMb = maxSizeMb,
            AllowedFileExtensions = extensions,
            DefaultPageSize = 20,
        });
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

    private async Task<Ticket> AddTicketAsync(AppDbContext db, Guid createdByUserId)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = "HD-000001",
            Title = "Test ticket",
            Description = "Test description",
            CategoryId = _categoryId,
            PriorityId = _priorityId,
            StatusId = _openStatusId,
            CreatedByUserId = createdByUserId,
        };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
        return ticket;
    }

    private static UploadAttachmentRequest CreateUploadRequest(string fileName, string contentType = "text/plain", string content = "hello world")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new UploadAttachmentRequest
        {
            FileName = fileName,
            ContentType = contentType,
            Length = bytes.Length,
            Content = new MemoryStream(bytes),
        };
    }

    private AttachmentService CreateSut(
        AppDbContext db, Mock<ICurrentUserService> currentUser, FakeFileStorageService fileStorage,
        Mock<ISystemSettingsService>? settings = null, Mock<IActivityLogService>? activityLog = null)
    {
        return new AttachmentService(
            db,
            currentUser.Object,
            fileStorage,
            (settings ?? MockSettings()).Object,
            (activityLog ?? new Mock<IActivityLogService>()).Object);
    }

    [Fact]
    public async Task UploadAsync_ValidFile_StoresAttachmentAndFile()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);
        var fileStorage = new FakeFileStorageService();

        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), fileStorage);

        var result = await sut.UploadAsync(ticket.Id, CreateUploadRequest("report.txt"));

        Assert.Equal("report.txt", result.FileName);
        Assert.Equal(owner.Id, result.UploadedByUserId);

        var stored = await db.TicketAttachments.FirstAsync(a => a.Id == result.Id);
        Assert.True(fileStorage.Contains(stored.StoredFileName));
        Assert.NotEqual("report.txt", stored.StoredFileName); // stored under a generated name, not the original
    }

    [Fact]
    public async Task UploadAsync_ExtensionAllowedButContentDoesNotMatchSignature_ThrowsValidationAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);

        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), new FakeFileStorageService());

        // ".pdf" is an allowed extension, but the actual bytes are plain text, not a real PDF —
        // this should be caught by the magic-byte check even though the extension whitelist passes.
        await Assert.ThrowsAsync<ValidationAppException>(
            () => sut.UploadAsync(ticket.Id, CreateUploadRequest("report.pdf", "application/pdf", "just plain text, not a pdf")));
    }

    [Fact]
    public async Task UploadAsync_PdfWithMatchingSignature_Succeeds()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);

        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), new FakeFileStorageService());

        var result = await sut.UploadAsync(
            ticket.Id, CreateUploadRequest("report.pdf", "application/pdf", "%PDF-1.4\n%fake but correctly-signed pdf content"));

        Assert.Equal("report.pdf", result.FileName);
    }

    [Fact]
    public async Task UploadAsync_DisallowedExtension_ThrowsValidationAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);

        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), new FakeFileStorageService());

        await Assert.ThrowsAsync<ValidationAppException>(
            () => sut.UploadAsync(ticket.Id, CreateUploadRequest("malware.exe")));
    }

    [Fact]
    public async Task UploadAsync_FileTooLarge_ThrowsValidationAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);

        var settings = MockSettings(maxSizeMb: 1);
        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), new FakeFileStorageService(), settings);

        var request = CreateUploadRequest("big.txt");
        request.Length = 2 * 1024 * 1024; // 2MB, over the 1MB limit

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.UploadAsync(ticket.Id, request));
    }

    [Fact]
    public async Task UploadAsync_EmployeeUploadingToSomeoneElsesTicket_ThrowsForbiddenAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var other = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);

        var sut = CreateSut(db, MockCurrentUser(other.Id, "Employee"), new FakeFileStorageService());

        await Assert.ThrowsAsync<ForbiddenAppException>(() => sut.UploadAsync(ticket.Id, CreateUploadRequest("file.txt")));
    }

    [Fact]
    public async Task DownloadAsync_ReturnsStoredContent()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);
        var fileStorage = new FakeFileStorageService();
        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), fileStorage);

        var uploaded = await sut.UploadAsync(ticket.Id, CreateUploadRequest("notes.txt", content: "important notes"));

        var result = await sut.DownloadAsync(ticket.Id, uploaded.Id);

        using var reader = new StreamReader(result.Content);
        Assert.Equal("important notes", await reader.ReadToEndAsync());
        Assert.Equal("notes.txt", result.FileName);
    }

    [Fact]
    public async Task DownloadAsync_EmployeeAccessingSomeoneElsesTicket_ThrowsForbiddenAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var other = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);
        var fileStorage = new FakeFileStorageService();

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), fileStorage);
        var uploaded = await ownerSut.UploadAsync(ticket.Id, CreateUploadRequest("file.txt"));

        var otherSut = CreateSut(db, MockCurrentUser(other.Id, "Employee"), fileStorage);
        await Assert.ThrowsAsync<ForbiddenAppException>(() => otherSut.DownloadAsync(ticket.Id, uploaded.Id));
    }

    [Fact]
    public async Task DeleteAsync_ByUploader_Succeeds()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);
        var fileStorage = new FakeFileStorageService();
        var sut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), fileStorage);

        var uploaded = await sut.UploadAsync(ticket.Id, CreateUploadRequest("file.txt"));
        await sut.DeleteAsync(ticket.Id, uploaded.Id);

        var visible = await db.TicketAttachments.FirstOrDefaultAsync(a => a.Id == uploaded.Id);
        Assert.Null(visible);
    }

    [Fact]
    public async Task DeleteAsync_ByNonUploaderEmployee_ThrowsForbiddenAppException()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var other = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);
        var fileStorage = new FakeFileStorageService();

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), fileStorage);
        var uploaded = await ownerSut.UploadAsync(ticket.Id, CreateUploadRequest("file.txt"));

        // Agent adds the other Employee as a co-viewer isn't a real concept here; instead verify
        // an Agent can access (would need Agent role) — use a second Employee attempt on their own ticket
        // context isn't valid, so assert against the owner's ticket using another Employee directly.
        var otherSut = CreateSut(db, MockCurrentUser(other.Id, "Employee"), fileStorage);
        await Assert.ThrowsAsync<ForbiddenAppException>(() => otherSut.DeleteAsync(ticket.Id, uploaded.Id));
    }

    [Fact]
    public async Task DeleteAsync_ByAgent_Succeeds()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var agent = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);
        var fileStorage = new FakeFileStorageService();

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), fileStorage);
        var uploaded = await ownerSut.UploadAsync(ticket.Id, CreateUploadRequest("file.txt"));

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"), fileStorage);
        await agentSut.DeleteAsync(ticket.Id, uploaded.Id);

        var visible = await db.TicketAttachments.FirstOrDefaultAsync(a => a.Id == uploaded.Id);
        Assert.Null(visible);
        Assert.False(fileStorage.Contains(uploaded.Id.ToString())); // physically removed too
    }

    [Fact]
    public async Task GetForTicketAsync_Agent_SeesAllAttachments()
    {
        await using var db = CreateSeededDbContext();
        var owner = await AddUserAsync(db);
        var agent = await AddUserAsync(db);
        var ticket = await AddTicketAsync(db, owner.Id);
        var fileStorage = new FakeFileStorageService();

        var ownerSut = CreateSut(db, MockCurrentUser(owner.Id, "Employee"), fileStorage);
        await ownerSut.UploadAsync(ticket.Id, CreateUploadRequest("a.txt"));
        await ownerSut.UploadAsync(ticket.Id, CreateUploadRequest("b.txt"));

        var agentSut = CreateSut(db, MockCurrentUser(agent.Id, "IT Support Agent"), fileStorage);
        var result = await agentSut.GetForTicketAsync(ticket.Id);

        Assert.Equal(2, result.Count);
    }
}
