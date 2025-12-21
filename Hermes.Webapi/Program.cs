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

// Demo endpoint showing implicit conversion from value to Result
app.MapGet("result/implicit-value/{value:int}", (int value) =>
{
    // Implicit conversion from int to Result<int>
    Result<int> result = value * 2;
    return result;
});

// Demo endpoint showing Map extension
app.MapGet("result/extensions/map/{value:int}", (int value) =>
{
    var result = Result<int>.Ok(value);
    
    // Map to string
    var stringResult = result.Map(x => $"Value is {x}");
    
    return stringResult;
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal record User(int Id, string Name, string Email);
