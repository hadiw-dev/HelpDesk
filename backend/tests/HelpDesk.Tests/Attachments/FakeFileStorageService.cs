using HelpDesk.Application.Common.Interfaces;

namespace HelpDesk.Tests.Attachments;

/// <summary>In-memory test double for <see cref="IFileStorageService"/> — keeps attachment tests fast and hermetic (no real disk I/O).</summary>
public class FakeFileStorageService : IFileStorageService
{
    private readonly Dictionary<string, byte[]> _files = [];

    public Task<string> SaveAsync(Stream content, string fileExtension, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        content.CopyTo(memoryStream);

        var storedFileName = $"{Guid.NewGuid():N}{fileExtension}";
        _files[storedFileName] = memoryStream.ToArray();
        return Task.FromResult(storedFileName);
    }

    public Task<Stream> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        if (!_files.TryGetValue(storedFileName, out var bytes))
        {
            throw new FileNotFoundException("Stored file could not be found.", storedFileName);
        }

        Stream stream = new MemoryStream(bytes);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        _files.Remove(storedFileName);
        return Task.CompletedTask;
    }

    public bool Contains(string storedFileName) => _files.ContainsKey(storedFileName);
}
