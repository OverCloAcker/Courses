using Courses.WebAPI.ApiVersioning;

namespace Courses.WebAPI.Endpoints;

public static class WeatherEndpoints
{
    private const string WeatherForecastEndpointName = "WeatherForecast";
    
    public static IEndpointRouteBuilder MapWeatherEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionManager apiVersionManager)
    {
        var api = apiVersionManager.GetApi(endpoints, "weather", "Weather", [
            ApiVersionManager.ApiVersions.V0_1,
            ApiVersionManager.ApiVersions.V1_0_Alpha,
            ApiVersionManager.ApiVersions.V1_0
        ]);

        api.MapWeatherEndpoints();

        return endpoints;
    }

    private static void MapWeatherEndpoints(this IEndpointRouteBuilder api)
    {
        api
            .MapGet("weatherforecast", GetWeatherForecastAsync)
            .WithName(WeatherForecastEndpointName)
            .Produces<IEnumerable<WeatherForecast>>();
    }

    private static async Task<IResult> GetWeatherForecastAsync(CancellationToken cancellationToken)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return await Task.FromResult(Results.Ok(forecast));
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}