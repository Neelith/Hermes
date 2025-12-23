using Hermes.Requests;
using Hermes.Responses;

namespace Hermes.Webapi.Queries;

/// <summary>
/// Query that returns only the user ID using IdResponse
/// </summary>
public record GetUserIdQuery(string Email) : IQuery<IdResponse<int>>;
