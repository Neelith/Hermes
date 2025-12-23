using Hermes.Handlers;
using Hermes.Results;
using Hermes.Webapi.Commands;
using Hermes.Webapi.Responses;

namespace Hermes.Webapi.Handlers;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserIdResponse>
{
    private readonly ILogger<CreateUserCommandHandler> _logger;
    private static int _nextId = 101;

    public CreateUserCommandHandler(ILogger<CreateUserCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<UserIdResponse>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CreateUserCommand for Name: {Name}, Email: {Email}",
            command.Name, command.Email);

        // Simulate async operation
        await Task.Delay(150, cancellationToken);

        // Simulate validation
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Ko<UserIdResponse>("INVALID_NAME", "Name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(command.Email) || !command.Email.Contains('@'))
        {
            return Result.Ko<UserIdResponse>("INVALID_EMAIL", "Email must be valid");
        }

        // Simulate creating user and returning ID
        var userId = _nextId++;
        _logger.LogInformation("User created with ID: {UserId}", userId);

        return Result.Ok(new UserIdResponse(userId));
    }
}
