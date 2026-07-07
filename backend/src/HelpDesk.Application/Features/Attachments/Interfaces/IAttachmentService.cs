using HelpDesk.Application.Features.Attachments.Dtos;

namespace HelpDesk.Application.Features.Attachments.Interfaces;

/// <summary>
/// Ticket-scoped file attachments. Access follows the same rule as the ticket itself (an Employee
/// only acts on tickets they created, Agent+ on any ticket) — enforced here, not just hidden in the UI.
/// </summary>
public interface IAttachmentService
{
    Task<TicketAttachmentDto> UploadAsync(Guid ticketId, UploadAttachmentRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketAttachmentDto>> GetForTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task<AttachmentDownloadResult> DownloadAsync(Guid ticketId, Guid attachmentId, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid ticketId, Guid attachmentId, CancellationToken cancellationToken = default);
}
