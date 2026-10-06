using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace EnterpriseKnowledgeAssistant.Api.Features.Health;

using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;

public sealed class HealthEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("GetHealth")
            .WithTags("Health");

        app.MapHealthChecks("/health/ready")
            .WithDisplayName("Readiness");
    }
}
