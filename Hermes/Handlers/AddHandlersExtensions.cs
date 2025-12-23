using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Hermes.Handlers;

public static class AddHandlersExtensions
{
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
}
