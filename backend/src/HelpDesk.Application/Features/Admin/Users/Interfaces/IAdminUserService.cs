using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Admin.Users.Dtos;

namespace HelpDesk.Application.Features.Admin.Users.Interfaces;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> SearchAsync(AdminUserQueryParameters query, CancellationToken cancellationToken = default);

    Task<AdminUserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AdminUserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<AdminUserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<AdminUserDto> ChangeRoleAsync(Guid id, ChangeUserRoleRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
