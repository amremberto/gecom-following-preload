using System.Globalization;
using GeCom.Following.Preload.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.Infrastructure.Storage;

/// <summary>
/// Service for file storage operations with Windows impersonation.
/// </summary>
internal sealed class StorageService : IStorageService
{
    private readonly StorageOptions _options;
    private readonly IImpersonationService _impersonationService;
    private readonly ILogger<StorageService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageService"/> class.
    /// </summary>
    /// <param name="options">Storage configuration options.</param>
    /// <param name="impersonationService">Impersonation service.</param>
    /// <param name="logger">Logger instance.</param>
    public StorageService(
        IOptions<StorageOptions> options,
        IImpersonationService impersonationService,
        ILogger<StorageService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(impersonationService);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _impersonationService = impersonationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<string> SaveFileAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileContent);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        string year = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture);
        string month = DateTime.UtcNow.Month.ToString("d2", CultureInfo.InvariantCulture);

        string basePath = _options.BasePath;
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new InvalidOperationException("Storage base path is not configured.");
        }

        // Ensure base path ends with backslash
        if (!basePath.EndsWith('\\'))
        {
            basePath += "\\";
        }

        string yearPath = Path.Combine(basePath, year);
        string monthPath = Path.Combine(yearPath, month);
        string fullPath = Path.Combine(monthPath, fileName);

        return _impersonationService.RunAsAsync(async cancellationToken =>
        {
            // Create year directory if it doesn't exist
            if (!Directory.Exists(yearPath))
            {
                Directory.CreateDirectory(yearPath);
                _logger.LogInformation("Created year directory: {YearPath}", yearPath);
            }

            // Create month directory if it doesn't exist
            if (!Directory.Exists(monthPath))
            {
                Directory.CreateDirectory(monthPath);
                _logger.LogInformation("Created month directory: {MonthPath}", monthPath);
            }

            // Write the file
            await File.WriteAllBytesAsync(fullPath, fileContent, cancellationToken);
            _logger.LogInformation("File saved successfully: {FullPath}", fullPath);

            return fullPath;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<byte[]> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return _impersonationService.RunAsAsync(async cancellationToken =>
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            byte[] fileContent = await File.ReadAllBytesAsync(filePath, cancellationToken);
            _logger.LogInformation("File read successfully: {FilePath}", filePath);

            return fileContent;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return _impersonationService.RunAsAsync(cancellationToken =>
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return Task.CompletedTask;
            }

            File.Delete(filePath);
            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);

            return Task.CompletedTask;
        }, cancellationToken);
    }
}

/// <summary>
/// Configuration options for storage service.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>
    /// Gets or sets the base path for file storage.
    /// </summary>
    public string BasePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the impersonation settings.
    /// </summary>
    public ImpersonationOptions Impersonation { get; set; } = new();
}

/// <summary>
/// Configuration options for Windows impersonation.
/// </summary>
public sealed class ImpersonationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether impersonation is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the username for impersonation (from configuration).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the domain for impersonation (from configuration).
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for impersonation (from configuration).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the environment variable name for username (optional, takes precedence over Username).
    /// </summary>
    public string? UserEnvVar { get; set; }

    /// <summary>
    /// Gets or sets the environment variable name for password (optional, takes precedence over Password).
    /// </summary>
    public string? PasswordEnvVar { get; set; }

    /// <summary>
    /// Gets or sets the environment variable name for domain (optional, takes precedence over Domain).
    /// </summary>
    public string? DomainEnvVar { get; set; }

    /// <summary>
    /// Gets or sets the logon type. Valid values: "Interactive" or "NewCredentials" (default).
    /// </summary>
    public string LogonType { get; set; } = "NewCredentials";
}

