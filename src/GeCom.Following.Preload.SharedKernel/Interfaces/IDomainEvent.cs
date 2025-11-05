using MediatR;

namespace GeCom.Following.Preload.SharedKernel.Interfaces;

/// <summary>
/// Represents a domain event in the context of a domain-driven design (DDD) application.
/// </summary>
/// <remarks>A domain event captures an occurrence within the domain that is of significance to the business.
/// Implementations of this interface are typically used to notify other parts of the system about changes or actions
/// that have occurred within the domain. This interface extends <see cref="INotification"/>, making it compatible with
/// the mediator pattern for event handling.</remarks>
public interface IDomainEvent : INotification;
