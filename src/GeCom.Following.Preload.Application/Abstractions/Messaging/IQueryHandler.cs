using GeCom.Following.Preload.SharedKernel.Results;
using MediatR;

namespace GeCom.Following.Preload.Application.Abstractions.Messaging;

/// <summary>
/// Represents a handler for queries that return a value of type TResponse.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResponse">The type of the response value.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
