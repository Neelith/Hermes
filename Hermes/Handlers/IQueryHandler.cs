using System;
using System.Collections.Generic;
using System.Text;
using Hermes.Requests;
using Hermes.Responses;
using Hermes.Results;

namespace Hermes.Handlers;

public interface IQueryHandler<in TQuery, TResponse> : IHandler
    where TQuery : IQuery<TResponse>
    where TResponse : IResponse
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
