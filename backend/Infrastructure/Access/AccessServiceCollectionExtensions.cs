namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;

public static class AccessServiceCollectionExtensions
{
    public static IServiceCollection AddLawFirmAccessControl(this IServiceCollection services)
    {
        services.AddScoped<IMatterAccessService, MatterAccessService>();
        return services;
    }
}
