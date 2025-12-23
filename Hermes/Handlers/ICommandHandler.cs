using System;
using System.Collections.Generic;
using System.Text;
using Hermes.Requests;
using Hermes.Responses;
using Hermes.Results;

namespace Hermes.Handlers;

/// <summary>
/// Defines a handler for commands that don't return a response value.
/// Commands are write operations that cause side effects or state changes.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle, which must implement <see cref="ICommand"/>.</typeparam>
public interface ICommandHandler<in TCommand> : IHandler
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Result"/> 
    /// indicating success or failure.</returns>
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Defines a handler for commands that return a response value.
/// Commands are write operations that cause side effects or state changes.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle, which must implement <see cref="ICommand{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the command, which must implement <see cref="IResponse"/>.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IHandler
    where TCommand : ICommand<TResponse>
    where TResponse : IResponse
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Result{TResponse}"/> 
    /// with the command result on success or errors on failure.</returns>
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
