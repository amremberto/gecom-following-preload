using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.SharedKernel.Entities;

/// <summary>
/// Represents the base class for domain entities, providing support for managing domain events.
/// </summary>
/// <remarks>This class serves as a foundation for domain-driven design (DDD) entities. It includes functionality
/// for raising and managing domain events, which can be used to capture and handle changes or actions  within the
/// domain model.</remarks>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets a read-only collection of all domain events raised by this entity.
    /// </summary>
    /// <value>A read-only list containing all domain events that have been raised by this entity.</value>
    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    /// <summary>
    /// Clears all domain events from this entity.
    /// </summary>
    /// <remarks>This method is typically called after domain events have been processed
    /// to prevent them from being raised again in subsequent operations.</remarks>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Raises a domain event by adding it to the entity's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    /// <remarks>Domain events are not immediately processed when raised. They are collected
    /// and can be processed later by the application's event handling infrastructure.</remarks>
    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
