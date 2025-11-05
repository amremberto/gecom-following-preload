using GeCom.Following.Preload.SharedKernel.Results;
using MediatR;

namespace GeCom.Following.Preload.Application.Abstractions.Messaging;

/// <summary>
/// Represents a command that does not return a value.
/// </summary>
public interface ICommand : IRequest<Result>, IBaseCommand;

/// <summary>
/// Represents a command that returns a value of type TResponse.
/// </summary>
/// <typeparam name="TResponse">The type of the response value.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

/// <summary>
/// Base interface for all commands in the application.
/// </summary>
public interface IBaseCommand;
