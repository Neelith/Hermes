using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Hermes.Handlers;

/// <summary>
/// Provides extension methods for registering Hermes handlers in the dependency injection container.
/// </summary>
public static class AddHandlersExtensions
{
    /// <summary>
    /// Registers all handler implementations found in the specified assemblies.
    /// Scans for implementations of <see cref="IQueryHandler{TQuery, TResponse}"/>, 
    /// <see cref="ICommandHandler{TCommand}"/>, and <see cref="ICommandHandler{TCommand, TResponse}"/>.
    /// </summary>
    /// <param name="services">The service collection to add handlers to.</param>
    /// <param name="assemblies">The assemblies to scan for handler implementations.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddHandlers(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        //Register the query handlers
        //Register the command handlers
        //Here we register both command handlers that return a response and those that don't
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registers a decorator for the specified handler type.
    /// Decorators allow cross-cutting concerns like logging, validation, or caching to be applied to handlers.
    /// </summary>
    /// <param name="services">The service collection to add the decorator to.</param>
    /// <param name="handler">The handler type to decorate.</param>
    /// <param name="decorator">The decorator type to apply.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddHandlerDecorator(
        this IServiceCollection services, Type handler, Type decorator)
    {
        var _ = services.TryDecorate(handler, decorator);
        return services;
    }
}
