using AutoMapper;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Common.Options;
using HelpDesk.Application.Features.Auth.Dtos;
using HelpDesk.Application.Features.Auth.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Services;

public class AuthService : IAuthService
{
    private const string DefaultRegistrationRole = "Employee";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IActivityLogService _activityLogService;
    private readonly IEmailSender _emailSender;
    private readonly IMapper _mapper;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        ITokenService tokenService,
        IActivityLogService activityLogService,
        IEmailSender emailSender,
        IMapper mapper,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _activityLogService = activityLogService;
        _emailSender = emailSender;
        _mapper = mapper;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            throw new ConflictAppException("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new ValidationAppException(string.Join(" ", createResult.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, DefaultRegistrationRole);
        await _activityLogService.LogAsync(user.Id, "UserRegistered", $"User {user.Email} registered.", ipAddress, cancellationToken);

        return await IssueAuthResultAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new UnauthorizedAppException("Account is locked due to too many failed attempts. Try again later.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        await _activityLogService.LogAsync(user.Id, "UserLoggedIn", $"User {user.Email} logged in.", ipAddress, cancellationToken);

        return await IssueAuthResultAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var existingToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            throw new UnauthorizedAppException("Invalid or expired refresh token.");
        }

        var newRefreshToken = CreateRefreshToken(existingToken.UserId, ipAddress);

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedByIp = ipAddress;
        existingToken.ReplacedByToken = newRefreshToken.Token;

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(existingToken.UserId, "TokenRefreshed", null, ipAddress, cancellationToken);

        var roles = await _userManager.GetRolesAsync(existingToken.User);
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(existingToken.User, roles);

        return BuildAuthResult(existingToken.User, roles, accessToken, expiresAt, newRefreshToken.Token);
    }

    public async Task LogoutAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return;
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedByIp = ipAddress;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(existingToken.UserId, "UserLoggedOut", null, ipAddress, cancellationToken);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // Do not reveal whether the email is registered.
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailSender.SendPasswordResetEmailAsync(user.Email!, token, cancellationToken);
        await _activityLogService.LogAsync(user.Id, "PasswordResetRequested", null, null, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            throw new ValidationAppException("Invalid or expired reset token.");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationAppException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await RevokeAllActiveRefreshTokensAsync(user.Id, cancellationToken);
        await _activityLogService.LogAsync(user.Id, "PasswordReset", null, null, cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundAppException("User not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationAppException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await _activityLogService.LogAsync(user.Id, "PasswordChanged", null, null, cancellationToken);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundAppException("User not found.");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Department = request.Department;
        user.JobTitle = request.JobTitle;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ValidationAppException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await _activityLogService.LogAsync(user.Id, "ProfileUpdated", null, null, cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task<UserDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new NotFoundAppException("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    private async Task<AuthResult> IssueAuthResultAsync(ApplicationUser user, string? ipAddress, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user, roles);

        var refreshToken = CreateRefreshToken(user.Id, ipAddress);
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return BuildAuthResult(user, roles, accessToken, expiresAt, refreshToken.Token);
    }

    private RefreshToken CreateRefreshToken(Guid userId, string? ipAddress) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Token = _tokenService.GenerateRefreshToken(),
        ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
        CreatedByIp = ipAddress,
    };

    private async Task RevokeAllActiveRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var token in activeTokens)
        {
            if (token.IsActive)
            {
                token.RevokedAt = now;
            }
        }

        if (activeTokens.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private AuthResult BuildAuthResult(ApplicationUser user, IList<string> roles, string accessToken, DateTime expiresAt, string refreshToken) => new()
    {
        AccessToken = accessToken,
        AccessTokenExpiresAt = expiresAt,
        RefreshToken = refreshToken,
        User = MapToDto(user, roles),
    };

    private UserDto MapToDto(ApplicationUser user, IList<string> roles)
    {
        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = roles.ToList();
        return dto;
    }
}
