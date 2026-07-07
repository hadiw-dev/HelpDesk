namespace HelpDesk.Application.Common.Interfaces;

/// <summary>
/// Abstracts physical file storage so the Application layer never touches disk paths directly.
/// The implementation is responsible for keeping files outside any web-servable root — files are
/// only ever reachable through an authenticated download endpoint, never a direct static URL.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Saves the stream under a storage-generated name and returns that name (not the caller's original filename).</summary>
    Task<string> SaveAsync(Stream content, string fileExtension, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default);
}
