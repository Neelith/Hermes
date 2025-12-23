using System;
using System.Collections.Generic;
using System.Text;
using Hermes.Requests;
using Hermes.Responses;
using Hermes.Results;

namespace Hermes.Handlers;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : IResponse
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
