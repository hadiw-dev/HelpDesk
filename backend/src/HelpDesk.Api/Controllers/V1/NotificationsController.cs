using Asp.Versioning;
using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Notifications.Dtos;
using HelpDesk.Application.Features.Notifications.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotificationDto>>> Search(
        [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] bool unreadOnly, CancellationToken cancellationToken)
    {
        var effectivePage = page <= 0 ? 1 : page;
        var effectivePageSize = pageSize <= 0 ? 20 : pageSize;

        return Ok(await _notificationService.GetForCurrentUserAsync(effectivePage, effectivePageSize, unreadOnly, cancellationToken));
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken)
    {
        return Ok(await _notificationService.GetUnreadCountAsync(cancellationToken));
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await _notificationService.MarkAllAsReadAsync(cancellationToken);
        return NoContent();
    }
}
