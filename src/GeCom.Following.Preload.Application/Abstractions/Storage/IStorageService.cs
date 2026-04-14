namespace GeCom.Following.Preload.Application.Abstractions.Storage;

/// <summary>
/// Service interface for file storage operations with Windows impersonation.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Checks if a file exists in storage by its path using Windows impersonation.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file exists, otherwise false.</returns>
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a file to the storage location using Windows impersonation.
    /// Creates the directory structure (Year/Month) if it doesn't exist.
    /// </summary>
    /// <param name="fileContent">The file content as a byte array.</param>
    /// <param name="fileName">The name of the file to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The full path where the file was saved.</returns>
    Task<string> SaveFileAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a payment detail (recibo) PDF to the PaymentDetailPath location using Windows impersonation.
    /// Creates the directory structure (Year/Month) if it doesn't exist.
    /// </summary>
    /// <param name="fileContent">The file content as a byte array.</param>
    /// <param name="fileName">The name of the file to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The full path where the file was saved.</returns>
    Task<string> SavePaymentDetailFileAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a file from storage by its path using Windows impersonation.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file content as byte array.</returns>
    Task<byte[]> ReadFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a payment detail (recibo) PDF from the PaymentDetailPath location using Windows impersonation.
    /// The path is built as PaymentDetailPath\{year}\{month}\{fileName}.
    /// </summary>
    /// <param name="fileName">The name of the file (e.g. from PaymentDetail.NamePdf).</param>
    /// <param name="year">Year for the subfolder.</param>
    /// <param name="month">Month for the subfolder (1-12).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file content as byte array.</returns>
    Task<byte[]> ReadPaymentDetailFileAsync(string fileName, int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a retention receipt PDF from the RetentionReceiptPath location using Windows impersonation.
    /// </summary>
    /// <param name="fileName">The name of the file (e.g. from RetentionReceipt.NamePdf).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file content as byte array.</returns>
    Task<byte[]> ReadRetentionReceiptPdfAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage by its path using Windows impersonation.
    /// </summary>
    /// <param name="filePath">The full path to the file to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}

