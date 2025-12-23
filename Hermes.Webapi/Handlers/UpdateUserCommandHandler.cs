using Hermes.Handlers;
using Hermes.Requests;
using Hermes.Results;
using Hermes.Webapi.Commands;
using Hermes.Webapi.Responses;

namespace Hermes.Webapi.Handlers;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserIdResponse>
{
    public Task<Result<UserIdResponse>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        // Validate
        var errors = new List<IError>();

        if (command.UserId <= 0)
        {
            errors.Add(new Error("ERR_INVALID_ID", "User ID must be greater than 0"));
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            errors.Add(new Error("ERR_INVALID_NAME", "Name cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(command.Email) || !command.Email.Contains('@'))
        {
            errors.Add(new Error("ERR_INVALID_EMAIL", "Email must be valid"));
        }

        if (errors.Count > 0)
        {
            return Task.FromResult(Result.Ko<UserIdResponse>(errors));
        }

        if (command.UserId > 100)
        {
            return Task.FromResult(Result.Ko<UserIdResponse>("ERR_NOT_FOUND", $"User with ID {command.UserId} not found"));
        }

        // Simulate successful update
        var response = new UserIdResponse(command.UserId);
        return Task.FromResult(Result.Ok(response));
    }
}
