using System.Diagnostics;
using Hermes.Handlers;
using Hermes.Requests;
using Hermes.Responses;
using Hermes.Results;

namespace Hermes.Webapi.Decorators;

public class LoggingCommandHandlerDecorator<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : IResponse
{
    private readonly ICommandHandler<TCommand, TResponse> _inner;
    private readonly ILogger<LoggingCommandHandlerDecorator<TCommand, TResponse>> _logger;

    public LoggingCommandHandlerDecorator(
        ICommandHandler<TCommand, TResponse> inner,
        ILogger<LoggingCommandHandlerDecorator<TCommand, TResponse>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var commandName = typeof(TCommand).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("[DECORATOR] Before executing command: {CommandName}", commandName);
        _logger.LogDebug("[DECORATOR] Command details: {@Command}", command);

        try
        {
            var result = await _inner.Handle(command, cancellationToken);

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "[DECORATOR] Command {CommandName} completed successfully in {ElapsedMs}ms",
                    commandName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "[DECORATOR] Command {CommandName} failed in {ElapsedMs}ms with {ErrorCount} error(s): {Errors}",
                    commandName, stopwatch.ElapsedMilliseconds, result.Errors.Count,
                    string.Join(", ", result.Errors.Select(e => e.Message)));
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "[DECORATOR] Command {CommandName} threw an exception after {ElapsedMs}ms",
                commandName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
