using Hermes.Handlers;
using Hermes.Responses;
using Hermes.Results;
using Hermes.Webapi.Queries;

namespace Hermes.Webapi.Handlers;

public class GetUserIdQueryHandler : IQueryHandler<GetUserIdQuery, IdResponse<int>>
{
    public Task<Result<IdResponse<int>>> Handle(GetUserIdQuery query, CancellationToken cancellationToken)
    {
        // Validate email
        if (string.IsNullOrWhiteSpace(query.Email) || !query.Email.Contains('@'))
        {
            return Task.FromResult(Result.Ko<IdResponse<int>>("ERR_INVALID_EMAIL", "Email must be valid"));
        }

        // Simulate finding user by email
        var userId = Math.Abs(query.Email.GetHashCode()) % 100 + 1;

        // Create IdResponse with metadata
        var response = IdResponse<int>.Create(userId, new Dictionary<string, string?>
        {
            ["Email"] = query.Email,
            ["LookedUpAt"] = DateTime.UtcNow.ToString("o")
        });

        return Task.FromResult(Result.Ok(response));
    }
}
