namespace GeCom.Following.Preload.Application.Abstractions.Storage;

/// <summary>
/// Service interface for Windows impersonation operations.
/// </summary>
public interface IImpersonationService : IAsyncDisposable
{
    /// <summary>
    /// Executes work asynchronously under impersonated context.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="work">The work to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the work.</returns>
    Task<T> RunAsAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken);

    /// <summary>
    /// Executes work asynchronously under impersonated context.
    /// </summary>
    /// <param name="work">The work to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RunAsAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken);
}

