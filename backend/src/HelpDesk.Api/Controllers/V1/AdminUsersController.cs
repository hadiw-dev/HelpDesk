using Asp.Versioning;
using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Admin.Users.Dtos;
using HelpDesk.Application.Features.Admin.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireAdmin")]
[Route("api/v{version:apiVersion}/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> Search([FromQuery] AdminUserQueryParameters query, CancellationToken cancellationToken)
    {
        return Ok(await _adminUserService.SearchAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _adminUserService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AdminUserDto>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _adminUserService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id, version = "1.0" }, user);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _adminUserService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPut("{id:guid}/role")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>> ChangeRole(Guid id, ChangeUserRoleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _adminUserService.ChangeRoleAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _adminUserService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
