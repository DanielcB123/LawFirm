using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;

namespace EnterpriseKnowledgeAssistant.Api.Features.Engagements;

/// <summary>
/// Engagement letters and fee-agreement versions between intake acceptance and an active matter
/// (gap G24; Markdown/modules/intake-and-consultations.md). Not implemented.
/// </summary>
public sealed class EngagementsEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGroup("/engagements").WithTags("Engagements").RequireAuthorization();
    }
}
