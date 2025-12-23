using Hermes.Handlers;
using Hermes.Responses;
using Hermes.Results;
using Hermes.Webapi.Queries;
using Hermes.Webapi.Responses;

namespace Hermes.Webapi.Handlers;

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, Response<UserDto>>
{
    private readonly ILogger<GetUserQueryHandler> _logger;

    public GetUserQueryHandler(ILogger<GetUserQueryHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<Response<UserDto>>> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetUserQuery for UserId: {UserId}", query.UserId);

        // Simulate async operation
        await Task.Delay(100, cancellationToken);

        // Simulate business logic
        if (query.UserId <= 0)
        {
            return Result.Ko<Response<UserDto>>("INVALID_USER_ID", "User ID must be greater than 0");
        }

        if (query.UserId > 100)
        {
            return Result.Ko<Response<UserDto>>("USER_NOT_FOUND", $"User with ID {query.UserId} not found");
        }

        var user = Response<UserDto>.Create(new UserDto(
            query.UserId,
            $"User {query.UserId}",
            $"user{query.UserId}@example.com"
        ));

        return Result.Ok(user);
    }
}
