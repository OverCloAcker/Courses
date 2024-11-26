using Asp.Versioning;
using Courses.WebAPI.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var withApiVersioning = builder.Services.AddApiVersioning();

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();

var apiVersionSetBuilder = app.NewApiVersionSet();
var apiVersionSet = apiVersionSetBuilder
    .HasApiVersion(new ApiVersion(1.0))
    .HasApiVersion(new ApiVersion(2.0))
    .HasApiVersion(new ApiVersion(3.0))
    .ReportApiVersions()
    .Build();

var apiVersion = new ApiVersion(2.0);
var routeGroup = app
    .MapGroup($"api/v{{version:apiVersion}}/weather")
    .WithTags("weather")
    .WithApiVersionSet(apiVersionSet)
    .MapToApiVersion(apiVersion);

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

routeGroup.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.UseDefaultOpenApi();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}