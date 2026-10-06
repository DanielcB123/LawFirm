using System.Reflection;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;

public static class EndpointModuleExtensions
{
    public static IServiceCollection AddEndpointModules(this IServiceCollection services)
    {
        var modules = Assembly
            .GetExecutingAssembly()
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } && typeof(IEndpointModule).IsAssignableFrom(type))
            .Select(type => (IEndpointModule)Activator.CreateInstance(type.AsType())!)
            .ToArray();

        services.AddSingleton<IReadOnlyCollection<IEndpointModule>>(modules);

        return services;
    }

    public static WebApplication MapEndpointModules(this WebApplication app)
    {
        var modules = app.Services.GetRequiredService<IReadOnlyCollection<IEndpointModule>>();
        var apiGroup = app.MapGroup("/api");

        foreach (var module in modules)
        {
            module.MapEndpoints(apiGroup);
        }

        return app;
    }
}
