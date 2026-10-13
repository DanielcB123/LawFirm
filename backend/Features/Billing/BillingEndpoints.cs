using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;

namespace EnterpriseKnowledgeAssistant.Api.Features.Billing;

/// <summary>
/// Legal billing: time entries, expenses, invoices (gap G50; Markdown/billing/README.md). Not implemented.
/// Distinct from SaaS subscription billing (G53). The existing <c>/billing/*</c> demo routes in
/// <c>Features/Authorization</c> are placeholders and are removed when this module ships.
/// </summary>
public sealed class BillingEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGroup("/billing-v2").WithTags("Billing").RequireAuthorization();
    }
}
