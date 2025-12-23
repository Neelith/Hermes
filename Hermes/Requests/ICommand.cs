using System;
using System.Collections.Generic;
using System.Text;
using Hermes.Results;

namespace Hermes.Requests;

public interface ICommand : ICommand<Result>, IRequest;

public interface ICommand<TResponse> : IRequest;

