using HelpDesk.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HelpDesk.Infrastructure.Services;

/// <summary>
/// Stores files on local disk, under a root path resolved outside the Api project's content root
/// (configured via <c>FileStorage:RootPath</c>) — nothing under that root is ever served by static
/// file middleware, since none is configured; the only way to read a file back is through
/// <see cref="OpenReadAsync"/>, called by <c>AttachmentService</c> after its own access check.
/// Files are named by a generated GUID, never the caller-supplied name, so nothing here is
/// path-traversal-able from user input.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configuredPath = configuration["FileStorage:RootPath"]
            ?? throw new InvalidOperationException("FileStorage:RootPath is not configured.");

        // Resolved against the current working directory, same convention Serilog's "Logs/log-.txt"
        // path already uses in this project — for `dotnet run`, that's the project's content root.
        _rootPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configuredPath));
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream content, string fileExtension, CancellationToken cancellationToken = default)
    {
        var storedFileName = $"{Guid.NewGuid():N}{fileExtension}";
        var fullPath = Path.Combine(_rootPath, storedFileName);

        await using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        return storedFileName;
    }

    public Task<Stream> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveExistingPath(storedFileName);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storedFileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolveExistingPath(string storedFileName)
    {
        var fullPath = Path.Combine(_rootPath, storedFileName);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Stored file could not be found.", storedFileName);
        }

        return fullPath;
    }
}
