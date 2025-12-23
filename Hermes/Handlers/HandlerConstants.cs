using System;
using System.Collections.Generic;
using System.Text;

namespace Hermes.Handlers;

public static class HandlerConstants
{
    public static List<Type> HandlerInterfaces => 
        [
            typeof(IQueryHandler<,>),
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>)
        ];
}
