namespace EnterpriseKnowledgeAssistant.Api.Features.WeatherForecast;

public sealed class WeatherForecastService : IWeatherForecastService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public IReadOnlyCollection<WeatherForecastResponse> GetNextFiveDays()
    {
        return Enumerable
            .Range(1, 5)
            .Select(index => new WeatherForecastResponse(
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
                Random.Shared.Next(-20, 55),
                Summaries[Random.Shared.Next(Summaries.Length)]))
            .ToArray();
    }
}
