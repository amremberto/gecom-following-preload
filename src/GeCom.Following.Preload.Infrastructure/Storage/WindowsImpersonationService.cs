using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using GeCom.Following.Preload.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;

namespace GeCom.Following.Preload.Infrastructure.Storage;

#pragma warning disable CA1416 // Validate platform compatibility
/// <summary>
/// Windows impersonation service implementation using SafeAccessTokenHandle and RunImpersonatedAsync.
/// </summary>
internal sealed class WindowsImpersonationService : IImpersonationService
{
    private readonly ImpersonationOptions _options;
    private readonly ILogger<WindowsImpersonationService> _logger;
    private SafeAccessTokenHandle? _token;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsImpersonationService"/> class.
    /// </summary>
    /// <param name="options">Impersonation configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public WindowsImpersonationService(
        IOptions<ImpersonationOptions> options,
        ILogger<WindowsImpersonationService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T> RunAsAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(work);

        if (!_options.Enabled)
        {
            return await work(cancellationToken);
        }

        EnsureToken();

        return await WindowsIdentity.RunImpersonatedAsync(_token!, async () => await work(cancellationToken));
    }

    /// <inheritdoc />
    public async Task RunAsAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(work);

        if (!_options.Enabled)
        {
            await work(cancellationToken);
            return;
        }

        EnsureToken();

        await WindowsIdentity.RunImpersonatedAsync(_token!, async () => await work(cancellationToken));
    }

    private void EnsureToken()
    {
        if (_token is not null && !_token.IsInvalid)
        {
            return;
        }

        // Get credentials from configuration or environment variables
        string user = GetCredential(_options.Username, _options.UserEnvVar);
        string pass = GetCredential(_options.Password, _options.PasswordEnvVar);
        string domain = GetCredential(_options.Domain, _options.DomainEnvVar);

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            throw new InvalidOperationException("Impersonation credentials not found. Configure Storage:Impersonation or set environment variables.");
        }

        int logonType = _options.LogonType.Equals("Interactive", StringComparison.OrdinalIgnoreCase)
            ? LOGON32_LOGON_INTERACTIVE
            : LOGON32_LOGON_NEW_CREDENTIALS;

        bool ok = LogonUser(user, domain, pass, logonType, LOGON32_PROVIDER_WINNT50, out SafeAccessTokenHandle token);
        if (!ok)
        {
            int err = Marshal.GetLastWin32Error();
            _logger.LogError("LogonUser failed for {Domain}\\{User} (type {LogonType}). Error code: {ErrorCode}", domain, user, logonType, err);
            throw new Win32Exception(err, $"LogonUser failed for {domain}\\{user} (type {logonType}).");
        }

        _token = token;
        _logger.LogDebug("Successfully impersonated user {Domain}\\{User}", domain, user);
    }

    private static string GetCredential(string configValue, string? envVarName)
    {
        // Use environment variable only if explicitly configured, otherwise use configuration value
        // This ensures that by default, credentials come from the configuration file
        if (!string.IsNullOrWhiteSpace(envVarName))
        {
            string? envValue = Environment.GetEnvironmentVariable(envVarName);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue;
            }
        }

        return configValue;
    }

    private const int LOGON32_LOGON_INTERACTIVE = 2;
    private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
    private const int LOGON32_PROVIDER_WINNT50 = 3;

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport(
        "advapi32.dll",
        SetLastError = true,
        CharSet = CharSet.Unicode,
        EntryPoint = "LogonUserW")]
    private static extern bool LogonUser(
        string lpszUsername,
        string lpszDomain,
        string lpszPassword,
        int dwLogonType,
        int dwLogonProvider,
        out SafeAccessTokenHandle phToken);

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _token?.Dispose();
        _token = null;
        return ValueTask.CompletedTask;
    }
}
#pragma warning restore CA1416 // Validate platform compatibility

