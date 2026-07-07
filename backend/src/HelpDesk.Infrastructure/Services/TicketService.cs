using AutoMapper;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Tickets.Dtos;
using HelpDesk.Application.Features.Tickets.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services.Shared;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public TicketService(
        AppDbContext dbContext,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<PagedResult<TicketListItemDto>> SearchAsync(TicketQueryParameters query, CancellationToken cancellationToken = default)
    {
        var ticketsQuery = _dbContext.Tickets.AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .AsQueryable();

        if (!TicketAccessGuard.IsAgentOrAbove(_currentUserService))
        {
            var userId = TicketAccessGuard.RequireUserId(_currentUserService);
            ticketsQuery = ticketsQuery.Where(t => t.CreatedByUserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            ticketsQuery = ticketsQuery.Where(t => t.Title.Contains(term) || t.TicketNumber.Contains(term));
        }

        if (query.CategoryId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.CategoryId == query.CategoryId.Value);
        }

        if (query.PriorityId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.PriorityId == query.PriorityId.Value);
        }

        if (query.StatusId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.StatusId == query.StatusId.Value);
        }

        if (query.AssignedToUserId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.AssignedToUserId == query.AssignedToUserId.Value);
        }

        ticketsQuery = ApplySorting(ticketsQuery, query.SortBy, query.SortDescending);

        var totalCount = await ticketsQuery.CountAsync(cancellationToken);

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var tickets = await ticketsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = await MapListItemsAsync(tickets, cancellationToken);

        return new PagedResult<TicketListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<TicketDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, id, cancellationToken);
        return await MapToDtoAsync(ticket, cancellationToken);
    }

    public async Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = TicketAccessGuard.RequireUserId(_currentUserService);

        await EnsureCategoryAndPriorityExistAsync(request.CategoryId, request.PriorityId, cancellationToken);

        var openStatusId = await _dbContext.Statuses
            .Where(s => s.Name == "Open")
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (openStatusId == Guid.Empty)
        {
            throw new ValidationAppException("The default 'Open' status is not configured.");
        }

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = await GenerateTicketNumberAsync(cancellationToken),
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            PriorityId = request.PriorityId,
            StatusId = openStatusId,
            CreatedByUserId = currentUserId,
            DueDate = request.DueDate,
        };

        _dbContext.Tickets.Add(ticket);
        RecordHistory(ticket.Id, "Ticket", null, "Created");
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(currentUserId, "TicketCreated", $"Ticket {ticket.TicketNumber} created.", null, cancellationToken);

        return await GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task<TicketDto> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, id, cancellationToken);
        var currentUserId = TicketAccessGuard.RequireUserId(_currentUserService);
        var isPrivileged = TicketAccessGuard.IsAgentOrAbove(_currentUserService);

        if (!isPrivileged)
        {
            if (ticket.CreatedByUserId != currentUserId)
            {
                throw new ForbiddenAppException("You do not have access to this ticket.");
            }

            if (ticket.Status.Name != "Open")
            {
                throw new ValidationAppException("Only tickets in the Open status can be edited by their creator.");
            }
        }

        await EnsureCategoryAndPriorityExistAsync(request.CategoryId, request.PriorityId, cancellationToken);

        RecordHistory(ticket.Id, "Title", ticket.Title, request.Title);
        RecordHistory(ticket.Id, "Description", ticket.Description, request.Description);
        ticket.Title = request.Title;
        ticket.Description = request.Description;
        ticket.DueDate = request.DueDate;

        if (isPrivileged)
        {
            await ApplyPrivilegedChangesAsync(ticket, request, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _activityLogService.LogAsync(currentUserId, "TicketUpdated", $"Ticket {ticket.TicketNumber} updated.", null, cancellationToken);

        return await GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TicketAccessGuard.IsAgentOrAbove(_currentUserService))
        {
            throw new ForbiddenAppException("You do not have permission to delete tickets.");
        }

        var ticket = await _dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundAppException("Ticket not found.");

        _dbContext.Tickets.Remove(ticket);
        RecordHistory(ticket.Id, "Ticket", "Active", "Deleted");
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(TicketAccessGuard.RequireUserId(_currentUserService), "TicketDeleted", $"Ticket {ticket.TicketNumber} soft-deleted.", null, cancellationToken);
    }

    public async Task<TicketDto> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TicketAccessGuard.IsManagerOrAdmin(_currentUserService))
        {
            throw new ForbiddenAppException("You do not have permission to restore tickets.");
        }

        var ticket = await _dbContext.Tickets.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundAppException("Ticket not found.");

        if (!ticket.IsDeleted)
        {
            throw new ValidationAppException("Ticket is not deleted.");
        }

        ticket.IsDeleted = false;
        ticket.DeletedAt = null;
        ticket.DeletedBy = null;

        RecordHistory(ticket.Id, "Ticket", "Deleted", "Restored");
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(TicketAccessGuard.RequireUserId(_currentUserService), "TicketRestored", $"Ticket {ticket.TicketNumber} restored.", null, cancellationToken);

        return await GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<TicketHistoryDto>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Ensures access control (throws if the ticket doesn't exist or the caller can't see it).
        await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, id, cancellationToken);

        var entries = await _dbContext.TicketHistories.AsNoTracking()
            .Where(h => h.TicketId == id)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);

        var userIds = entries.Where(h => h.ChangedByUserId.HasValue).Select(h => h.ChangedByUserId!.Value).Distinct().ToList();
        var userNames = await UserDisplayNameResolver.GetNamesAsync(_dbContext, userIds, cancellationToken);

        return entries.Select(h => new TicketHistoryDto
        {
            Id = h.Id,
            FieldName = h.FieldName,
            OldValue = h.OldValue,
            NewValue = h.NewValue,
            ChangedByName = h.ChangedByUserId.HasValue ? userNames.GetValueOrDefault(h.ChangedByUserId.Value, "Unknown") : "System",
            ChangedAt = h.CreatedAt,
        }).ToList();
    }

    private async Task ApplyPrivilegedChangesAsync(Ticket ticket, UpdateTicketRequest request, CancellationToken cancellationToken)
    {
        var categories = await _dbContext.Categories.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
        var priorities = await _dbContext.Priorities.AsNoTracking().ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);
        var statuses = await _dbContext.Statuses.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        if (!statuses.ContainsKey(request.StatusId))
        {
            throw new ValidationAppException("Status not found.");
        }

        RecordHistory(ticket.Id, "Category", categories.GetValueOrDefault(ticket.CategoryId), categories.GetValueOrDefault(request.CategoryId));
        RecordHistory(ticket.Id, "Priority", priorities.GetValueOrDefault(ticket.PriorityId), priorities.GetValueOrDefault(request.PriorityId));
        RecordHistory(ticket.Id, "Status", statuses.GetValueOrDefault(ticket.StatusId), statuses.GetValueOrDefault(request.StatusId));

        var newStatusName = statuses[request.StatusId];
        if (newStatusName == "Resolved" && ticket.ResolvedAt is null)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }
        else if (newStatusName == "Closed" && ticket.ClosedAt is null)
        {
            ticket.ClosedAt = DateTime.UtcNow;
        }

        ticket.CategoryId = request.CategoryId;
        ticket.PriorityId = request.PriorityId;
        ticket.StatusId = request.StatusId;
    }

    private async Task<TicketDto> MapToDtoAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<TicketDto>(ticket);

        var userIds = new List<Guid> { ticket.CreatedByUserId };
        if (ticket.AssignedToUserId.HasValue)
        {
            userIds.Add(ticket.AssignedToUserId.Value);
        }

        var names = await UserDisplayNameResolver.GetNamesAsync(_dbContext, userIds, cancellationToken);
        dto.CreatedByName = names.GetValueOrDefault(ticket.CreatedByUserId, "Unknown");
        dto.AssignedToName = ticket.AssignedToUserId.HasValue ? names.GetValueOrDefault(ticket.AssignedToUserId.Value, "Unknown") : null;

        return dto;
    }

    private async Task<List<TicketListItemDto>> MapListItemsAsync(List<Ticket> tickets, CancellationToken cancellationToken)
    {
        var userIds = tickets.Select(t => t.CreatedByUserId)
            .Concat(tickets.Where(t => t.AssignedToUserId.HasValue).Select(t => t.AssignedToUserId!.Value))
            .Distinct()
            .ToList();

        var names = await UserDisplayNameResolver.GetNamesAsync(_dbContext, userIds, cancellationToken);

        return tickets.Select(ticket =>
        {
            var dto = _mapper.Map<TicketListItemDto>(ticket);
            dto.CreatedByName = names.GetValueOrDefault(ticket.CreatedByUserId, "Unknown");
            dto.AssignedToName = ticket.AssignedToUserId.HasValue ? names.GetValueOrDefault(ticket.AssignedToUserId.Value, "Unknown") : null;
            return dto;
        }).ToList();
    }

    private async Task EnsureCategoryAndPriorityExistAsync(Guid categoryId, Guid priorityId, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken))
        {
            throw new ValidationAppException("Category not found.");
        }

        if (!await _dbContext.Priorities.AnyAsync(p => p.Id == priorityId, cancellationToken))
        {
            throw new ValidationAppException("Priority not found.");
        }
    }

    private async Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken)
    {
        var count = await _dbContext.Tickets.IgnoreQueryFilters().CountAsync(cancellationToken);
        return $"HD-{count + 1:D6}";
    }

    private void RecordHistory(Guid ticketId, string fieldName, string? oldValue, string? newValue) =>
        TicketHistoryRecorder.Record(_dbContext, _currentUserService, ticketId, fieldName, oldValue, newValue);

    private static IQueryable<Ticket> ApplySorting(IQueryable<Ticket> query, string? sortBy, bool descending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "priority" => descending ? query.OrderByDescending(t => t.Priority.DisplayOrder) : query.OrderBy(t => t.Priority.DisplayOrder),
            "status" => descending ? query.OrderByDescending(t => t.Status.DisplayOrder) : query.OrderBy(t => t.Status.DisplayOrder),
            "duedate" => descending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "ticketnumber" => descending ? query.OrderByDescending(t => t.TicketNumber) : query.OrderBy(t => t.TicketNumber),
            _ => descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
        };
    }
}
