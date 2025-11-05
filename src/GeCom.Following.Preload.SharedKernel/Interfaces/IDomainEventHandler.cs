namespace GeCom.Following.Preload.SharedKernel.Interfaces;

/// <summary>
/// Defines a handler for a specific type of domain event.
/// </summary>
/// <typeparam name="T">The type of domain event to handle.</typeparam>
/// <remarks>This interface is used to implement the handler pattern for domain events,
/// allowing for loose coupling and separation of concerns when processing domain events.</remarks>
public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    /// <summary>
    /// Handles the specified domain event asynchronously.
    /// </summary>
    /// <param name="domainEvent">The domain event to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(T domainEvent, CancellationToken cancellationToken);
}
