using Hermes.Requests;
using Hermes.Responses;
using Hermes.Results;

namespace Hermes.Handlers;

/// <summary>
/// Defines a handler for queries that return a response.
/// Queries are read-only operations that retrieve data without causing side effects.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle, which must implement <see cref="IQuery{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the query, which must implement <see cref="IResponse"/>.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IHandler
    where TQuery : IQuery<TResponse>
    where TResponse : IResponse
{
    /// <summary>
    /// Handles the specified query asynchronously.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Result{TResponse}"/> 
    /// with the query result on success or errors on failure.</returns>
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
