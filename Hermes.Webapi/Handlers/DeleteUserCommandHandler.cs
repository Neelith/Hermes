using Hermes.Handlers;
using Hermes.Requests;
using Hermes.Results;
using Hermes.Webapi.Commands;

namespace Hermes.Webapi.Handlers;

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    public Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        // Simulate validation and deletion
        if (command.UserId <= 0)
        {
            return Task.FromResult(Result.Ko("ERR_INVALID_ID", "User ID must be greater than 0"));
        }

        if (command.UserId > 100)
        {
            return Task.FromResult(Result.Ko("ERR_NOT_FOUND", $"User with ID {command.UserId} not found"));
        }

        // Simulate successful deletion
        return Task.FromResult(Result.Ok());
    }
}
