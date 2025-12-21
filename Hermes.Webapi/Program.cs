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
    var result = Result<object>.Ok(
        new { Message = "Operation completed successfully", Data = 42 },
        new Dictionary<string, string?> { { "timestamp", DateTime.UtcNow.ToString() } }
    );
    return result;
});

app.MapGet("result/failure", () =>
{
    var result = Result<object>.Ko(
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
        new Error(ErrorCodes.ValidationError, "Email is required", new Dictionary<string, string?> { { "field", "email" } }),
        new Error(ErrorCodes.ValidationError, "Password must be at least 8 characters", new Dictionary<string, string?> { { "field", "password" } })
    };
    
    var result = Result<object>.Ko(errors);
    return result;
});

app.MapGet("result/with-error-metadata", () =>
{
    var error = new Error(
        ErrorCodes.NotFound,
        "User not found",
        new Dictionary<string, string?> 
        { 
            { "userId", "123" },
            { "attemptedAt", DateTime.UtcNow.ToString() }
        }
    );
    
    var result = Result<object>.Ko(
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
        return Result<string>.Ko(
            ErrorCodes.ValidationError,
            "ID must be greater than zero",
            new Dictionary<string, string?> { { "providedId", id.ToString() } }
        );
    }
    
    if (id > 100)
    {
        return Result<string>.Ko(
            ErrorCodes.NotFound,
            $"Resource with ID {id} not found"
        );
    }
    
    return Result<string>.Ok(
        $"Processed resource {id}",
        new Dictionary<string, string?> { { "processingTime", "23ms" } }
    );
});

// Demo endpoint showing implicit conversion from value to Result
app.MapGet("result/implicit-value/{value:int}", (int value) =>
{
    // Implicit conversion from int to Result<int>
    Result<int> result = value * 2;
    return result;
});

// Demo endpoint showing implicit conversion from Error to Result
app.MapGet("result/implicit-error", () =>
{
    // Implicit conversion from Error to Result<string>
    Result<string> result = new Error(ErrorCodes.InternalError, "Something went wrong");
    return result;
});

// Demo endpoint showing implicit conversion to extract value
app.MapGet("result/implicit-extract", () =>
{
    Result<string> result = Result<string>.Ok("Hello, World!");
    
    // Implicit conversion from Result<string> to string
    string value = result;
    
    return new { ExtractedValue = value };
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
