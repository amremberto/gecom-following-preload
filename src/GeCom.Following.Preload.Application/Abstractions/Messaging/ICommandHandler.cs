using GeCom.Following.Preload.SharedKernel.Results;
using MediatR;

namespace GeCom.Following.Preload.Application.Abstractions.Messaging;

/// <summary>
/// Represents a handler for commands that do not return a value.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public interface ICommandHandler<in TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>
/// Represents a handler for commands that return a value of type TResponse.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResponse">The type of the response value.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
