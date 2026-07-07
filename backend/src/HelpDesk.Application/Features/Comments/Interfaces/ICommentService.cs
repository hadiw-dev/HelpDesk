using HelpDesk.Application.Features.Comments.Dtos;

namespace HelpDesk.Application.Features.Comments.Interfaces;

public interface ICommentService
{
    /// <summary>
    /// Internal notes (<c>IsInternal == true</c>) are only included when the caller is Agent+;
    /// an Employee never sees internal notes, even on their own ticket.
    /// </summary>
    Task<IReadOnlyList<CommentDto>> GetForTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task<CommentDto> AddAsync(Guid ticketId, CreateCommentRequest request, CancellationToken cancellationToken = default);
}
