using Hermes.Requests;
using Hermes.Webapi.Responses;

namespace Hermes.Webapi.Commands;

public record CreateUserCommand(string Name, string Email) : ICommand<UserIdResponse>;
