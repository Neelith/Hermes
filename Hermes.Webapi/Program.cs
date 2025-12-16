using YourProjectName.Shared.Responses;

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

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
