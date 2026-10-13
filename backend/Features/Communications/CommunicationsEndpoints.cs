using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;

namespace EnterpriseKnowledgeAssistant.Api.Features.Communications;

/// <summary>
/// Published client updates and secure matter messaging (gaps G41, G42; Markdown/modules/communications.md).
/// Not implemented. Notifications go through the outbox, not inline provider calls.
/// </summary>
public sealed class CommunicationsEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGroup("/communications").WithTags("Communications").RequireAuthorization();
    }
}
