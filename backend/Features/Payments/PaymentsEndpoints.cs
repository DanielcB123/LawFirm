using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;

namespace EnterpriseKnowledgeAssistant.Api.Features.Payments;

/// <summary>
/// Payments, allocations, refunds, and provider webhooks (gap G51; Markdown/billing/payments-and-ledger.md).
/// Not implemented. Webhook routes will be anonymous but signature-verified, so they belong in a separate group.
/// </summary>
public sealed class PaymentsEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGroup("/payments").WithTags("Payments").RequireAuthorization();
    }
}
