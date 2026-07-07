using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Features.Admin.Settings.Interfaces;
using HelpDesk.Application.Features.Attachments.Dtos;
using HelpDesk.Application.Features.Attachments.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services.Shared;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ISystemSettingsService _systemSettingsService;
    private readonly IActivityLogService _activityLogService;

    public AttachmentService(
        AppDbContext dbContext,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        ISystemSettingsService systemSettingsService,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _systemSettingsService = systemSettingsService;
        _activityLogService = activityLogService;
    }

    public async Task<TicketAttachmentDto> UploadAsync(Guid ticketId, UploadAttachmentRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, ticketId, cancellationToken);
        var currentUserId = TicketAccessGuard.RequireUserId(_currentUserService);

        var originalFileName = Path.GetFileName(request.FileName);
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ValidationAppException("A file name is required.");
        }

        var settings = await _systemSettingsService.GetAsync(cancellationToken);

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var allowedExtensions = settings.AllowedFileExtensions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(e => e.ToLowerInvariant())
            .ToHashSet();

        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            throw new ValidationAppException($"File type '{extension}' is not allowed. Allowed types: {settings.AllowedFileExtensions}");
        }

        var maxBytes = settings.MaxFileUploadSizeMb * 1024L * 1024L;
        if (request.Length <= 0 || request.Length > maxBytes)
        {
            throw new ValidationAppException($"File size must be greater than 0 and no more than {settings.MaxFileUploadSizeMb} MB.");
        }

        var storedFileName = await _fileStorageService.SaveAsync(request.Content, extension, cancellationToken);

        var attachment = new TicketAttachment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            UploadedByUserId = currentUserId,
            FileName = originalFileName,
            StoredFileName = storedFileName,
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType,
            FileSizeBytes = request.Length,
        };

        _dbContext.TicketAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(
            currentUserId, "AttachmentUploaded", $"Uploaded '{originalFileName}' to ticket {ticket.TicketNumber}.", null, cancellationToken);

        var uploaderName = await UserDisplayNameResolver.GetNameAsync(_dbContext, currentUserId, cancellationToken);
        return MapToDto(attachment, uploaderName);
    }

    public async Task<IReadOnlyList<TicketAttachmentDto>> GetForTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, ticketId, cancellationToken);

        var attachments = await _dbContext.TicketAttachments.AsNoTracking()
            .Where(a => a.TicketId == ticketId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var names = await UserDisplayNameResolver.GetNamesAsync(
            _dbContext, attachments.Select(a => a.UploadedByUserId).Distinct().ToList(), cancellationToken);

        return attachments.Select(a => MapToDto(a, names.GetValueOrDefault(a.UploadedByUserId, "Unknown"))).ToList();
    }

    public async Task<AttachmentDownloadResult> DownloadAsync(Guid ticketId, Guid attachmentId, CancellationToken cancellationToken = default)
    {
        await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, ticketId, cancellationToken);

        var attachment = await _dbContext.TicketAttachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TicketId == ticketId, cancellationToken)
            ?? throw new NotFoundAppException("Attachment not found.");

        var stream = await _fileStorageService.OpenReadAsync(attachment.StoredFileName, cancellationToken);

        return new AttachmentDownloadResult
        {
            Content = stream,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
        };
    }

    public async Task DeleteAsync(Guid ticketId, Guid attachmentId, CancellationToken cancellationToken = default)
    {
        var ticket = await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, ticketId, cancellationToken);
        var currentUserId = TicketAccessGuard.RequireUserId(_currentUserService);

        var attachment = await _dbContext.TicketAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TicketId == ticketId, cancellationToken)
            ?? throw new NotFoundAppException("Attachment not found.");

        var isUploader = attachment.UploadedByUserId == currentUserId;
        if (!isUploader && !TicketAccessGuard.IsAgentOrAbove(_currentUserService))
        {
            throw new ForbiddenAppException("You do not have permission to delete this attachment.");
        }

        _dbContext.TicketAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _fileStorageService.DeleteAsync(attachment.StoredFileName, cancellationToken);

        await _activityLogService.LogAsync(
            currentUserId, "AttachmentDeleted", $"Deleted '{attachment.FileName}' from ticket {ticket.TicketNumber}.", null, cancellationToken);
    }

    private static TicketAttachmentDto MapToDto(TicketAttachment attachment, string uploaderName) => new()
    {
        Id = attachment.Id,
        TicketId = attachment.TicketId,
        FileName = attachment.FileName,
        ContentType = attachment.ContentType,
        FileSizeBytes = attachment.FileSizeBytes,
        UploadedByUserId = attachment.UploadedByUserId,
        UploadedByName = uploaderName,
        CreatedAt = attachment.CreatedAt,
    };
}
