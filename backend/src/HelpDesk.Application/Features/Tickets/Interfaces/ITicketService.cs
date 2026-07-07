using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Tickets.Dtos;

namespace HelpDesk.Application.Features.Tickets.Interfaces;

public interface ITicketService
{
    Task<PagedResult<TicketListItemDto>> SearchAsync(TicketQueryParameters query, CancellationToken cancellationToken = default);

    Task<TicketDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default);

    Task<TicketDto> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TicketDto> RestoreAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketHistoryDto>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default);
}
