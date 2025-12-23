using Hermes.Requests;
using Hermes.Responses;

namespace Hermes.Webapi.Queries;

public record GetUserQuery(int UserId) : IQuery<Response<UserDto>>;
