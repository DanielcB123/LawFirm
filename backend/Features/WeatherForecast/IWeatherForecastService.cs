namespace EnterpriseKnowledgeAssistant.Api.Features.WeatherForecast;

public interface IWeatherForecastService
{
    IReadOnlyCollection<WeatherForecastResponse> GetNextFiveDays();
}
