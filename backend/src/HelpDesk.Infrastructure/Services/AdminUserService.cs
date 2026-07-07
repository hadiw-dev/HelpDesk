using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Admin.Users.Dtos;
using HelpDesk.Application.Features.Admin.Users.Interfaces;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class AdminUserService : IAdminUserService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public AdminUserService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<PagedResult<AdminUserDto>> SearchAsync(AdminUserQueryParameters query, CancellationToken cancellationToken = default)
    {
        var usersQuery = _dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            usersQuery = usersQuery.Where(u =>
                u.Email!.Contains(term) || u.FirstName.Contains(term) || u.LastName.Contains(term));
        }

        if (query.IsActive.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var roleId = await _dbContext.Roles
                .Where(r => r.Name == query.Role)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var userIdsInRole = _dbContext.UserRoles.Where(ur => ur.RoleId == roleId).Select(ur => ur.UserId);
            usersQuery = usersQuery.Where(u => userIdsInRole.Contains(u.Id));
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var users = await usersQuery
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIdsOnPage = users.Select(u => u.Id).ToList();
        var rolesByUser = await _dbContext.UserRoles
            .Where(ur => userIdsOnPage.Contains(ur.UserId))
            .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name! })
            .ToListAsync(cancellationToken);

        var items = users
            .Select(u => MapToDto(u, rolesByUser.Where(r => r.UserId == u.Id).Select(r => r.RoleName).ToList()))
            .ToList();

        return new PagedResult<AdminUserDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<AdminUserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundAppException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles.ToList());
    }

    public async Task<AdminUserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            throw new ConflictAppException("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = request.Department,
            JobTitle = request.JobTitle,
            IsActive = true,
            EmailConfirmed = true,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new ValidationAppException(string.Join(" ", createResult.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        await _activityLogService.LogAsync(
            _currentUserService.UserId, "UserCreated", $"Created user {user.Email} with role {request.Role}.", null, cancellationToken);

        return MapToDto(user, [request.Role]);
    }

    public async Task<AdminUserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundAppException("User not found.");

        if (!request.IsActive && id == RequireCurrentUserId())
        {
            throw new ValidationAppException("You cannot deactivate your own account.");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Department = request.Department;
        user.JobTitle = request.JobTitle;
        user.IsActive = request.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new ValidationAppException(string.Join(" ", updateResult.Errors.Select(e => e.Description)));
        }

        await _activityLogService.LogAsync(_currentUserService.UserId, "UserUpdated", $"Updated user {user.Email}.", null, cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles.ToList());
    }

    public async Task<AdminUserDto> ChangeRoleAsync(Guid id, ChangeUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (id == RequireCurrentUserId())
        {
            throw new ValidationAppException("You cannot change your own role.");
        }

        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundAppException("User not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        await _activityLogService.LogAsync(
            _currentUserService.UserId, "UserRoleChanged", $"Changed {user.Email}'s role to {request.Role}.", null, cancellationToken);

        return MapToDto(user, [request.Role]);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == RequireCurrentUserId())
        {
            throw new ValidationAppException("You cannot delete your own account.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundAppException("User not found.");

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(_currentUserService.UserId, "UserDeleted", $"Deleted user {user.Email}.", null, cancellationToken);
    }

    private Guid RequireCurrentUserId() =>
        _currentUserService.UserId ?? throw new UnauthorizedAppException("The current user could not be identified.");

    private static AdminUserDto MapToDto(ApplicationUser user, IReadOnlyList<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Department = user.Department,
        JobTitle = user.JobTitle,
        IsActive = user.IsActive,
        Roles = roles,
        CreatedAt = user.CreatedAt,
    };
}
