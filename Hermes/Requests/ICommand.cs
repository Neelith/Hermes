using Hermes.Results;

namespace Hermes.Requests;

public interface ICommand : ICommand<Result>, IRequest;

public interface ICommand<TResponse> : IRequest;

