using Hermes.Requests;
using Hermes.Responses;

namespace Hermes.Webapi.Queries;

/// <summary>
/// Query that returns a paged list of users using PagedResponse
/// </summary>
public record GetUsersPagedQuery(int Page, int PageSize) : IQuery<PagedResponse<UserDto>>;
