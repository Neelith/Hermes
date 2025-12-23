using System;
using System.Collections.Generic;
using System.Text;

namespace Hermes.Handlers;

/// <summary>
/// Provides constants related to handler types in the Hermes framework.
/// </summary>
public static class HandlerConstants
{
    /// <summary>
    /// Gets the list of all handler interface types supported by the framework.
    /// Includes <see cref="IQueryHandler{TQuery, TResponse}"/>, <see cref="ICommandHandler{TCommand}"/>, 
    /// and <see cref="ICommandHandler{TCommand, TResponse}"/>.
    /// </summary>
    public static List<Type> HandlerInterfaces => 
        [
            typeof(IQueryHandler<,>),
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>)
        ];
}
