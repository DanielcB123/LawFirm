using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;

namespace EnterpriseKnowledgeAssistant.Api.Features.Trust;

/// <summary>
/// Trust accounting: journal postings, client subledgers, three-way reconciliation, disbursement approvals
/// (gap G52; Markdown/billing/trust-and-settlement.md). Not implemented. Approver must differ from preparer.
/// </summary>
public sealed class TrustEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGroup("/trust").WithTags("Trust").RequireAuthorization();
    }
}
