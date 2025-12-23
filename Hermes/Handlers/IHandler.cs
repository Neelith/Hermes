namespace Hermes.Handlers;

/// <summary>
/// Marker interface for all handler types in the Hermes framework.
/// Implemented by <see cref="IQueryHandler{TQuery, TResponse}"/>, <see cref="ICommandHandler{TCommand}"/>, 
/// and <see cref="ICommandHandler{TCommand, TResponse}"/>.
/// </summary>
public interface IHandler;
