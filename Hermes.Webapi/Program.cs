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

app.MapGet("id", () =>
{
    return ResponseFactory.CreateIdResponse(Guid.NewGuid());
});

app.MapGet("list", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    return ResponseFactory.CreatePagedResponse(summaries, summaries.Length);
});

app.MapGet("response", () =>
{
    return ResponseFactory.CreateResponse(new { Hello = "World"}, new Dictionary<string, string?> { { "key", "value" } });
});

// Result object demo endpoints
app.MapGet("result/success", () =>
{
    var result = ResultFactory.Success(
        new { Message = "Operation completed successfully", Data = 42 },
        new Dictionary<string, string?> { { "timestamp", DateTime.UtcNow.ToString() } }
    );
    return result;
});

app.MapGet("result/failure", () =>
{
    var result = ResultFactory.Failure<object>(
        ErrorCodes.ValidationError,
        "Invalid input provided",
        new Dictionary<string, string?> { { "field", "email" } }
    );
    return result;
});

app.MapGet("result/multiple-errors", () =>
{
    var errors = new[]
    {
        ResultFactory.CreateError(ErrorCodes.ValidationError, "Email is required", new Dictionary<string, string?> { { "field", "email" } }),
        ResultFactory.CreateError(ErrorCodes.ValidationError, "Password must be at least 8 characters", new Dictionary<string, string?> { { "field", "password" } })
    };
    
    var result = ResultFactory.Failure<object>(errors);
    return result;
});

app.MapGet("result/with-error-metadata", () =>
{
    var error = ResultFactory.CreateError(
        ErrorCodes.NotFound,
        "User not found",
        new Dictionary<string, string?> 
        { 
            { "userId", "123" },
            { "attemptedAt", DateTime.UtcNow.ToString() }
        }
    );
    
    var result = ResultFactory.Failure<object>(
        error,
        new Dictionary<string, string?> { { "requestId", Guid.NewGuid().ToString() } }
    );
    return result;
});

app.MapGet("result/process/{id:int}", (int id) =>
{
    // Simulate processing with conditional success/failure
    if (id <= 0)
    {
        return ResultFactory.Failure<string>(
            ErrorCodes.ValidationError,
            "ID must be greater than zero",
            new Dictionary<string, string?> { { "providedId", id.ToString() } }
        );
    }
    
    if (id > 100)
    {
        return ResultFactory.Failure<string>(
            ErrorCodes.NotFound,
            $"Resource with ID {id} not found"
        );
    }
    
    return ResultFactory.Success(
        $"Processed resource {id}",
        new Dictionary<string, string?> { { "processingTime", "23ms" } }
    );
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
