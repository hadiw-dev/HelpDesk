using AutoMapper;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Common.Options;
using HelpDesk.Application.Features.Auth.Dtos;
using HelpDesk.Application.Features.Auth.Mappings;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace HelpDesk.Tests.Auth;

public class AuthServiceTests
{
    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<AuthMappingProfile>());
        return configuration.CreateMapper();
    }

    private static AuthService CreateSut(
        Mock<UserManager<ApplicationUser>> userManagerMock,
        AppDbContext dbContext,
        Mock<IActivityLogService>? activityLogMock = null,
        Mock<IEmailSender>? emailSenderMock = null)
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "HelpDeskSystem.Tests",
            Audience = "HelpDeskSystem.Tests.Client",
            SecretKey = "unit-test-signing-key-that-is-long-enough-1234567890",
            ExpiryMinutes = 15,
            RefreshTokenExpiryDays = 7,
        });

        var tokenService = new TokenService(jwtOptions);

        return new AuthService(
            userManagerMock.Object,
            dbContext,
            tokenService,
            (activityLogMock ?? new Mock<IActivityLogService>()).Object,
            (emailSenderMock ?? new Mock<IEmailSender>()).Object,
            CreateMapper(),
            jwtOptions);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsConflictAppException()
    {
        var existingUser = new ApplicationUser { Id = Guid.NewGuid(), Email = "existing@helpdesk.local" };
        var userManagerMock = MockUserManager();
        userManagerMock.Setup(m => m.FindByEmailAsync(existingUser.Email!)).ReturnsAsync(existingUser);

        await using var dbContext = CreateDbContext();
        var sut = CreateSut(userManagerMock, dbContext);

        var request = new RegisterRequest
        {
            Email = existingUser.Email!,
            Password = "Passw0rd!",
            ConfirmPassword = "Passw0rd!",
            FirstName = "Existing",
            LastName = "User",
        };

        await Assert.ThrowsAsync<ConflictAppException>(() => sut.RegisterAsync(request, "127.0.0.1"));
    }

    [Fact]
    public async Task RegisterAsync_WhenNewEmail_CreatesUserAndReturnsTokens()
    {
        var userManagerMock = MockUserManager();
        userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Employee"))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock
            .Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(["Employee"]);

        await using var dbContext = CreateDbContext();
        var sut = CreateSut(userManagerMock, dbContext);

        var request = new RegisterRequest
        {
            Email = "new.user@helpdesk.local",
            Password = "Passw0rd!",
            ConfirmPassword = "Passw0rd!",
            FirstName = "New",
            LastName = "User",
        };

        var result = await sut.RegisterAsync(request, "127.0.0.1");

        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Contains("Employee", result.User.Roles);
        Assert.Single(dbContext.RefreshTokens);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsUnauthorizedAppException()
    {
        var userManagerMock = MockUserManager();
        userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        await using var dbContext = CreateDbContext();
        var sut = CreateSut(userManagerMock, dbContext);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sut.LoginAsync(new LoginRequest { Email = "nobody@helpdesk.local", Password = "whatever" }, "127.0.0.1"));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ThrowsUnauthorizedAppExceptionAndRecordsFailure()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@helpdesk.local", IsActive = true };
        var userManagerMock = MockUserManager();
        userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);
        userManagerMock.Setup(m => m.AccessFailedAsync(user)).ReturnsAsync(IdentityResult.Success);

        var activityLogMock = new Mock<IActivityLogService>();
        await using var dbContext = CreateDbContext();
        var sut = CreateSut(userManagerMock, dbContext, activityLogMock);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sut.LoginAsync(new LoginRequest { Email = user.Email!, Password = "wrong-password" }, "127.0.0.1"));

        userManagerMock.Verify(m => m.AccessFailedAsync(user), Times.Once);
        activityLogMock.Verify(m => m.LogAsync(
            user.Id, "LoginFailed", It.IsAny<string>(), "127.0.0.1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenAccountIsInactive_ThrowsUnauthorizedAppExceptionAndLogsFailureWithoutUserId()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "inactive@helpdesk.local", IsActive = false };
        var userManagerMock = MockUserManager();
        userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var activityLogMock = new Mock<IActivityLogService>();
        await using var dbContext = CreateDbContext();
        var sut = CreateSut(userManagerMock, dbContext, activityLogMock);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sut.LoginAsync(new LoginRequest { Email = user.Email!, Password = "anything" }, "127.0.0.1"));

        // An inactive/unknown account is deliberately logged without a user id, so the log itself
        // doesn't confirm the account exists to anyone reading it.
        activityLogMock.Verify(m => m.LogAsync(
            null, "LoginFailed", It.IsAny<string>(), "127.0.0.1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenAccountIsLockedOut_ThrowsUnauthorizedAppExceptionAndLogsFailure()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "locked@helpdesk.local", IsActive = true };
        var userManagerMock = MockUserManager();
        userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true);

        var activityLogMock = new Mock<IActivityLogService>();
        await using var dbContext = CreateDbContext();
        var sut = CreateSut(userManagerMock, dbContext, activityLogMock);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sut.LoginAsync(new LoginRequest { Email = user.Email!, Password = "anything" }, "127.0.0.1"));

        activityLogMock.Verify(m => m.LogAsync(
            user.Id, "LoginFailed", It.IsAny<string>(), "127.0.0.1", It.IsAny<CancellationToken>()), Times.Once);
        userManagerMock.Verify(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenSuccessful_LogsUserLoggedInActivity()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@helpdesk.local", IsActive = true };
        var userManagerMock = MockUserManager();
        userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManagerMock.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(false);
        userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);
        userManagerMock.Setup(m => m.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["Employee"]);

        var activityLogMock = new Mock<IActivityLogService>();
        await using var dbContext = CreateDbContext();
        var sut = CreateSut(userManagerMock, dbContext, activityLogMock);

        var result = await sut.LoginAsync(new LoginRequest { Email = user.Email!, Password = "correct" }, "127.0.0.1");

        Assert.NotEmpty(result.AccessToken);
        activityLogMock.Verify(m => m.LogAsync(
            user.Id, "UserLoggedIn", It.IsAny<string>(), "127.0.0.1", It.IsAny<CancellationToken>()), Times.Once);
        activityLogMock.Verify(m => m.LogAsync(
            It.IsAny<Guid?>(), "LoginFailed", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenDoesNotExist_ThrowsUnauthorizedAppException()
    {
        var userManagerMock = MockUserManager();
        await using var dbContext = CreateDbContext();
        var sut = CreateSut(userManagerMock, dbContext);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sut.RefreshTokenAsync("does-not-exist", "127.0.0.1"));
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsRevoked_ThrowsUnauthorizedAppException()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@helpdesk.local", IsActive = true };
        await using var dbContext = CreateDbContext();
        dbContext.Users.Add(user);
        dbContext.RefreshTokens.Add(new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            RevokedAt = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync();

        var userManagerMock = MockUserManager();
        var sut = CreateSut(userManagerMock, dbContext);

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            sut.RefreshTokenAsync("revoked-token", "127.0.0.1"));
    }
}
