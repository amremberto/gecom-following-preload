namespace GeCom.Following.Preload.Application.Abstractions.Storage;

/// <summary>
/// Service interface for file storage operations with Windows impersonation.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Saves a file to the storage location using Windows impersonation.
    /// Creates the directory structure (Year/Month) if it doesn't exist.
    /// </summary>
    /// <param name="fileContent">The file content as a byte array.</param>
    /// <param name="fileName">The name of the file to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The full path where the file was saved.</returns>
    Task<string> SaveFileAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default);
}

