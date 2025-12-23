using System.Diagnostics;
using Hermes.Handlers;
using Hermes.Requests;
using Hermes.Responses;
using Hermes.Results;

namespace Hermes.Webapi.Decorators;

public class LoggingQueryHandlerDecorator<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : IResponse
{
    private readonly IQueryHandler<TQuery, TResponse> _inner;
    private readonly ILogger<LoggingQueryHandlerDecorator<TQuery, TResponse>> _logger;

    public LoggingQueryHandlerDecorator(
        IQueryHandler<TQuery, TResponse> inner,
        ILogger<LoggingQueryHandlerDecorator<TQuery, TResponse>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        var queryName = typeof(TQuery).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("[DECORATOR] Before executing query: {QueryName}", queryName);
        _logger.LogDebug("[DECORATOR] Query details: {@Query}", query);

        try
        {
            var result = await _inner.Handle(query, cancellationToken);

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "[DECORATOR] Query {QueryName} completed successfully in {ElapsedMs}ms",
                    queryName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "[DECORATOR] Query {QueryName} failed in {ElapsedMs}ms with {ErrorCount} error(s): {Errors}",
                    queryName, stopwatch.ElapsedMilliseconds, result.Errors.Count,
                    string.Join(", ", result.Errors.Select(e => e.Message)));
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[DECORATOR] Query {QueryName} threw an exception after {ElapsedMs}ms",
                queryName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
