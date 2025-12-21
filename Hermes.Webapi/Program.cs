using Hermes.Responses;
using Hermes.Results;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Test endpoints for Result and Response functionality

// 1. Simple successful Result without value
app.MapGet("/api/test/result-ok", () =>
{
    var result = Result.Ok();
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
});

// 2. Successful Result with value
app.MapGet("/api/test/result-ok-with-value", () =>
{
    var result = Result.Ok("Operation completed successfully");
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
});

// 3. Failed Result with single error
app.MapGet("/api/test/result-ko-single", () =>
{
    var result = Result.Ko("ERR001", "Something went wrong");
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
});

// 4. Failed Result with multiple errors
app.MapGet("/api/test/result-ko-multiple", () =>
{
    var errors = new List<IError>
    {
        new Error("ERR001", "First error occurred"),
        new Error("ERR002", "Second error occurred"),
        new Error("ERR003", "Third error occurred")
    };
    var result = Result.Ko(errors);
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
});

// 5. Result with metadata
app.MapGet("/api/test/result-with-metadata", () =>
{
    var metadata = new Dictionary<string, string?>
    {
        ["RequestId"] = Guid.NewGuid().ToString(),
        ["Timestamp"] = DateTime.UtcNow.ToString("o")
    };
    var result = Result.Ok(42, metadata);
    return Results.Ok(result);
});

// 6. Generic Response
app.MapGet("/api/test/response-generic", () =>
{
    var response = ResponseFactory.CreateResponse(
        new { Name = "John Doe", Age = 30, Email = "john@example.com" },
        new Dictionary<string, string?> { ["Source"] = "TestAPI" }
    );
    return Results.Ok(response);
});

// 7. ID Response
app.MapGet("/api/test/response-id", () =>
{
    var response = ResponseFactory.CreateIdResponse(
        Guid.NewGuid(),
        new Dictionary<string, string?> { ["CreatedAt"] = DateTime.UtcNow.ToString("o") }
    );
    return Results.Ok(response);
});

// 8. Paged Response
app.MapGet("/api/test/response-paged", () =>
{
    var items = Enumerable.Range(1, 10).Select(i => new
    {
        Id = i,
        Name = $"Item {i}",
        Description = $"This is item number {i}"
    });
    
    var response = ResponseFactory.CreatePagedResponse(
        items,
        totalCount: 100,
        new Dictionary<string, string?> { ["PageSize"] = "10", ["CurrentPage"] = "1" }
    );
    return Results.Ok(response);
});

// 9. Result Match pattern
app.MapGet("/api/test/result-match/{shouldSucceed}", (bool shouldSucceed) =>
{
    var result = shouldSucceed 
        ? Result.Ok("Success!")
        : Result.Ko<string>("ERR001", "Operation failed");
    
    var message = result.Match(
        onSuccess: value => $"Operation succeeded with value: {value}",
        onFailure: errors => $"Operation failed with {errors.Count} error(s): {string.Join(", ", errors.Select(e => e.Message))}"
    );
    
    return Results.Ok(new { Message = message, Result = result });
});

// 10. Combined Result and Response
app.MapGet("/api/test/combined/{userId:int}", (int userId) =>
{
    // Simulate a service operation that returns a Result
    Result<UserDto> userResult = userId switch
    {
        > 0 and <= 100 => Result.Ok(new UserDto(userId, $"User{userId}", $"user{userId}@example.com")),
        > 100 => Result.Ko<UserDto>("ERR404", "User not found"),
        _ => Result.Ko<UserDto>("ERR400", "Invalid user ID")
    };
    
    return userResult.Match(
        onSuccess: user =>
        {
            var response = ResponseFactory.CreateResponse(
                user,
                new Dictionary<string, string?> { ["RetrievedAt"] = DateTime.UtcNow.ToString("o") }
            );
            return Results.Ok(response);
        },
        onFailure: errors => Results.BadRequest(new { Errors = errors })
    );
});

app.Run();

// Helper record for testing
record UserDto(int Id, string Name, string Email);