using Hermes.Handlers;
using Hermes.Responses;
using Hermes.Results;
using Hermes.Webapi.Queries;

namespace Hermes.Webapi.Handlers;

public class GetUsersPagedQueryHandler : IQueryHandler<GetUsersPagedQuery, PagedResponse<UserDto>>
{
    public Task<Result<PagedResponse<UserDto>>> Handle(GetUsersPagedQuery query, CancellationToken cancellationToken)
    {
        // Validate pagination parameters
        if (query.Page <= 0)
        {
            return Task.FromResult(Result.Ko<PagedResponse<UserDto>>("ERR_INVALID_PAGE", "Page must be greater than 0"));
        }

        if (query.PageSize <= 0 || query.PageSize > 100)
        {
            return Task.FromResult(Result.Ko<PagedResponse<UserDto>>("ERR_INVALID_PAGE_SIZE", "PageSize must be between 1 and 100"));
        }

        // Simulate total count
        const int totalCount = 250;

        // Calculate skip and take
        var skip = (query.Page - 1) * query.PageSize;
        var take = query.PageSize;

        // Generate sample users
        var users = Enumerable.Range(skip + 1, Math.Min(take, totalCount - skip))
            .Select(i => new UserDto(i, $"User {i}", $"user{i}@example.com"))
            .ToList();

        // Create PagedResponse with metadata
        var response = PagedResponse<UserDto>.Create(users, totalCount, new Dictionary<string, string?>
        {
            ["Page"] = query.Page.ToString(),
            ["PageSize"] = query.PageSize.ToString(),
            ["TotalPages"] = Math.Ceiling((double)totalCount / query.PageSize).ToString(),
            ["HasNextPage"] = (skip + take < totalCount).ToString(),
            ["HasPreviousPage"] = (query.Page > 1).ToString()
        });

        return Task.FromResult(Result.Ok(response));
    }
}
