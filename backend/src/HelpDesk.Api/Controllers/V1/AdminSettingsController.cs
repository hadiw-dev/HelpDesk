using Asp.Versioning;
using HelpDesk.Application.Features.Admin.Settings.Dtos;
using HelpDesk.Application.Features.Admin.Settings.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = "RequireAdmin")]
[Route("api/v{version:apiVersion}/admin/settings")]
public class AdminSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _systemSettingsService;

    public AdminSettingsController(ISystemSettingsService systemSettingsService)
    {
        _systemSettingsService = systemSettingsService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemSettingsDto>> Get(CancellationToken cancellationToken)
        => Ok(await _systemSettingsService.GetAsync(cancellationToken));

    [HttpPut]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemSettingsDto>> Update(UpdateSystemSettingsRequest request, CancellationToken cancellationToken)
        => Ok(await _systemSettingsService.UpdateAsync(request, cancellationToken));
}
