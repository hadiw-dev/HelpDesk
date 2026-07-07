using Asp.Versioning;
using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Assignments.Dtos;
using HelpDesk.Application.Features.Assignments.Interfaces;
using HelpDesk.Application.Features.Attachments.Dtos;
using HelpDesk.Application.Features.Attachments.Interfaces;
using HelpDesk.Application.Features.Comments.Dtos;
using HelpDesk.Application.Features.Comments.Interfaces;
using HelpDesk.Application.Features.Tickets.Dtos;
using HelpDesk.Application.Features.Tickets.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IAssignmentService _assignmentService;
    private readonly ICommentService _commentService;
    private readonly IAttachmentService _attachmentService;

    public TicketsController(
        ITicketService ticketService,
        IAssignmentService assignmentService,
        ICommentService commentService,
        IAttachmentService attachmentService)
    {
        _ticketService = ticketService;
        _assignmentService = assignmentService;
        _commentService = commentService;
        _attachmentService = attachmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TicketListItemDto>>> Search(
        [FromQuery] TicketQueryParameters query, CancellationToken cancellationToken)
    {
        return Ok(await _ticketService.SearchAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _ticketService.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<TicketHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TicketHistoryDto>>> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _ticketService.GetHistoryAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id, version = "1.0" }, ticket);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> Update(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _ticketService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAgentOrAbove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _ticketService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = "RequireManagerOrAdmin")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> Restore(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _ticketService.RestoreAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "RequireAgentOrAbove")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> Assign(Guid id, AssignTicketRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _assignmentService.AssignAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:guid}/auto-assign")]
    [Authorize(Policy = "RequireAgentOrAbove")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> AutoAssign(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _assignmentService.AutoAssignAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}/assignments")]
    [ProducesResponseType(typeof(IReadOnlyList<AssignmentHistoryEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AssignmentHistoryEntryDto>>> GetAssignmentHistory(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _assignmentService.GetHistoryAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}/comments")]
    [ProducesResponseType(typeof(IReadOnlyList<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> GetComments(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _commentService.GetForTicketAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> AddComment(Guid id, CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var comment = await _commentService.AddAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetComments), new { id, version = "1.0" }, comment);
    }

    [HttpGet("{id:guid}/attachments")]
    [ProducesResponseType(typeof(IReadOnlyList<TicketAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TicketAttachmentDto>>> GetAttachments(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _attachmentService.GetForTicketAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/attachments")]
    [ProducesResponseType(typeof(TicketAttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketAttachmentDto>> UploadAttachment(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var request = new UploadAttachmentRequest
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            Content = stream,
        };

        var attachment = await _attachmentService.UploadAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetAttachments), new { id, version = "1.0" }, attachment);
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken)
    {
        var result = await _attachmentService.DownloadAsync(id, attachmentId, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken)
    {
        await _attachmentService.DeleteAsync(id, attachmentId, cancellationToken);
        return NoContent();
    }
}
