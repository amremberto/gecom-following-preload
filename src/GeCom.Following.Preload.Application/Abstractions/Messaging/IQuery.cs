using GeCom.Following.Preload.SharedKernel.Results;
using MediatR;

namespace GeCom.Following.Preload.Application.Abstractions.Messaging;

/// <summary>
/// Represents a query that returns a value of type TResponse.
/// </summary>
/// <typeparam name="TResponse">The type of the response value.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
