using Hermes.Requests;
using Hermes.Webapi.Responses;

namespace Hermes.Webapi.Commands;

/// <summary>
/// Command with response - demonstrates ICommand<TResponse> handler
/// </summary>
public record UpdateUserCommand(int UserId, string Name, string Email) : ICommand<UserIdResponse>;
