using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Features.Admin.Users.Dtos;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HelpDesk.Tests.Admin;

public class AdminUserServiceTests
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

    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private async Task<ApplicationUser> AddUserAsync(AppDbContext db, string email = "")
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = string.IsNullOrEmpty(email) ? $"{Guid.NewGuid()}@helpdesk.local" : email,
            UserName = string.IsNullOrEmpty(email) ? $"{Guid.NewGuid()}@helpdesk.local" : email,
            FirstName = "Existing",
            LastName = "User",
            IsActive = true,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static AdminUserService CreateSut(
        AppDbContext db, Mock<UserManager<ApplicationUser>> userManager, Guid currentUserId, Mock<IActivityLogService>? activityLog = null)
    {
        return new AdminUserService(db, userManager.Object, MockCurrentUser(currentUserId).Object, (activityLog ?? new Mock<IActivityLogService>()).Object);
    }

    [Fact]
    public async Task CreateAsync_NewEmail_CreatesUserAndAssignsRole()
    {
        await using var db = CreateDbContext();
        var admin = await AddUserAsync(db);
        var userManager = MockUserManager();

        userManager.Setup(m => m.FindByEmailAsync("new.agent@helpdesk.local")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "Passw0rd1!")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "IT Support Agent")).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut(db, userManager, admin.Id);

        var request = new CreateUserRequest
        {
            Email = "new.agent@helpdesk.local",
            Password = "Passw0rd1!",
            FirstName = "New",
            LastName = "Agent",
            Role = "IT Support Agent",
        };

        var result = await sut.CreateAsync(request);

        Assert.Equal("new.agent@helpdesk.local", result.Email);
        Assert.Contains("IT Support Agent", result.Roles);
        userManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "Passw0rd1!"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsConflictAppException()
    {
        await using var db = CreateDbContext();
        var admin = await AddUserAsync(db);
        var existing = await AddUserAsync(db, "taken@helpdesk.local");

        var userManager = MockUserManager();
        userManager.Setup(m => m.FindByEmailAsync("taken@helpdesk.local")).ReturnsAsync(existing);

        var sut = CreateSut(db, userManager, admin.Id);

        var request = new CreateUserRequest
        {
            Email = "taken@helpdesk.local",
            Password = "Passw0rd1!",
            FirstName = "Dup",
            LastName = "User",
            Role = "Employee",
        };

        await Assert.ThrowsAsync<ConflictAppException>(() => sut.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProfileFields()
    {
        await using var db = CreateDbContext();
        var admin = await AddUserAsync(db);
        var target = await AddUserAsync(db);

        var userManager = MockUserManager();
        userManager.Setup(m => m.FindByIdAsync(target.Id.ToString())).ReturnsAsync(target);
        userManager.Setup(m => m.UpdateAsync(target)).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.GetRolesAsync(target)).ReturnsAsync(["Employee"]);

        var sut = CreateSut(db, userManager, admin.Id);

        var result = await sut.UpdateAsync(target.Id, new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            IsActive = true,
        });

        Assert.Equal("Updated", result.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_SelfDeactivate_ThrowsValidationAppException()
    {
        await using var db = CreateDbContext();
        var admin = await AddUserAsync(db);

        var userManager = MockUserManager();
        userManager.Setup(m => m.FindByIdAsync(admin.Id.ToString())).ReturnsAsync(admin);

        var sut = CreateSut(db, userManager, admin.Id);

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.UpdateAsync(admin.Id, new UpdateUserRequest
        {
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            IsActive = false,
        }));
    }

    [Fact]
    public async Task ChangeRoleAsync_RemovesOldRolesAndAssignsNew()
    {
        await using var db = CreateDbContext();
        var admin = await AddUserAsync(db);
        var target = await AddUserAsync(db);

        var userManager = MockUserManager();
        userManager.Setup(m => m.FindByIdAsync(target.Id.ToString())).ReturnsAsync(target);
        userManager.Setup(m => m.GetRolesAsync(target)).ReturnsAsync(["Employee"]);
        userManager.Setup(m => m.RemoveFromRolesAsync(target, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddToRoleAsync(target, "Manager")).ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut(db, userManager, admin.Id);

        var result = await sut.ChangeRoleAsync(target.Id, new ChangeUserRoleRequest { Role = "Manager" });

        Assert.Contains("Manager", result.Roles);
        userManager.Verify(m => m.RemoveFromRolesAsync(target, It.Is<IEnumerable<string>>(r => r.Contains("Employee"))), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(target, "Manager"), Times.Once);
    }

    [Fact]
    public async Task ChangeRoleAsync_Self_ThrowsValidationAppException()
    {
        await using var db = CreateDbContext();
        var admin = await AddUserAsync(db);
        var sut = CreateSut(db, MockUserManager(), admin.Id);

        await Assert.ThrowsAsync<ValidationAppException>(
            () => sut.ChangeRoleAsync(admin.Id, new ChangeUserRoleRequest { Role = "Employee" }));
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesUser()
    {
        await using var db = CreateDbContext();
        var admin = await AddUserAsync(db);
        var target = await AddUserAsync(db);

        var sut = CreateSut(db, MockUserManager(), admin.Id);
        await sut.DeleteAsync(target.Id);

        var visible = await db.Users.FirstOrDefaultAsync(u => u.Id == target.Id);
        Assert.Null(visible);

        var actual = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == target.Id);
        Assert.NotNull(actual);
        Assert.True(actual!.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_Self_ThrowsValidationAppException()
    {
        await using var db = CreateDbContext();
        var admin = await AddUserAsync(db);
        var sut = CreateSut(db, MockUserManager(), admin.Id);

        await Assert.ThrowsAsync<ValidationAppException>(() => sut.DeleteAsync(admin.Id));
    }

    [Fact]
    public async Task SearchAsync_FiltersBySearchTerm()
    {
        await using var db = CreateDbContext();
        var admin = await AddUserAsync(db);
        await AddUserAsync(db, "findme@helpdesk.local");
        await AddUserAsync(db, "other@helpdesk.local");

        var sut = CreateSut(db, MockUserManager(), admin.Id);

        var result = await sut.SearchAsync(new AdminUserQueryParameters { SearchTerm = "findme" });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("findme@helpdesk.local", result.Items[0].Email);
    }
}
