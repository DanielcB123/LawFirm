namespace EnterpriseKnowledgeAssistant.Api.Features.WeatherForecast;

using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;

public sealed class WeatherForecastEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/weather/forecast", (IWeatherForecastService service) => Results.Ok(service.GetNextFiveDays()))
            .WithName("GetWeatherForecast")
            .WithTags("Weather")
            .WithSummary("Gets a 5-day weather forecast.");

        // Backward-compatible route from the starter template.
        app.MapGet("/weatherforecast", (IWeatherForecastService service) => Results.Ok(service.GetNextFiveDays()))
            .WithName("GetWeatherForecastLegacy")
            .ExcludeFromDescription();
    }
}
