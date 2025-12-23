using System.Reflection;
using Hermes.Handlers;
using Hermes.Responses;
using Hermes.Webapi.Commands;
using Hermes.Webapi.Decorators;
using Hermes.Webapi.Queries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register handlers from the WebApi assembly
builder.Services.AddHandlers([Assembly.GetExecutingAssembly()]);

// Register decorators for handlers
builder.Services.AddHandlerDecorator(typeof(IQueryHandler<,>), typeof(LoggingQueryHandlerDecorator<,>));
builder.Services.AddHandlerDecorator(typeof(ICommandHandler<,>), typeof(LoggingCommandHandlerDecorator<,>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// =============================================================================
// QUERY HANDLER TESTS - IQueryHandler<TQuery, TResponse>
// =============================================================================

// Query: Get User by ID - Returns Response<UserDto>
app.MapGet("/api/users/{userId:int}", async (
    int userId,
    IQueryHandler<GetUserQuery, Response<UserDto>> handler,
    CancellationToken ct) =>
{
    var query = new GetUserQuery(userId);
    var result = await handler.Handle(query, ct);

    return result.Match(
        onSuccess: response => Results.Ok(response),
        onFailure: errors => Results.BadRequest(new { Errors = errors })
    );
})
.WithName("GetUser")
.WithDescription("Query handler - returns Response<UserDto>");

// Query: Get User ID by Email - Returns IdResponse<int>
app.MapGet("/api/users/by-email/{email}", async (
    string email,
    IQueryHandler<GetUserIdQuery, IdResponse<int>> handler,
    CancellationToken ct) =>
{
    var query = new GetUserIdQuery(email);
    var result = await handler.Handle(query, ct);

    return result.Match(
        onSuccess: response => Results.Ok(response),
        onFailure: errors => Results.BadRequest(new { Errors = errors })
    );
})
.WithName("GetUserIdByEmail")
.WithDescription("Query handler - returns IdResponse<int>");

// Query: Get Paged Users - Returns PagedResponse<UserDto>
app.MapGet("/api/users/paged", async (
    int page,
    int pageSize,
    IQueryHandler<GetUsersPagedQuery, PagedResponse<UserDto>> handler,
    CancellationToken ct) =>
{
    var query = new GetUsersPagedQuery(page, pageSize);
    var result = await handler.Handle(query, ct);

    return result.Match(
        onSuccess: response => Results.Ok(response),
        onFailure: errors => Results.BadRequest(new { Errors = errors })
    );
})
.WithName("GetUsersPaged")
.WithDescription("Query handler - returns PagedResponse<UserDto>");

// =============================================================================
// COMMAND HANDLER TESTS - ICommandHandler<TCommand> (without response)
// =============================================================================

// Command without Response: Delete User - Returns only Result
app.MapDelete("/api/users/{userId:int}", async (
    int userId,
    ICommandHandler<DeleteUserCommand> handler,
    CancellationToken ct) =>
{
    var command = new DeleteUserCommand(userId);
    var result = await handler.Handle(command, ct);

    return result.Match(
        onSuccess: () => Results.NoContent(),
        onFailure: errors => Results.BadRequest(new { Errors = errors })
    );
})
.WithName("DeleteUser")
.WithDescription("Command handler without response - returns only Result");

app.Run();

// Request/Response DTOs
public record UserDto(int Id, string Name, string Email);