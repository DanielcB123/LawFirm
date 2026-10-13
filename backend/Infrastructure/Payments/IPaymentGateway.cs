namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Payments;

public enum PaymentDestination
{
    Operating,
    Trust
}

public sealed record PaymentIntentRequest(
    decimal Amount,
    string Currency,
    PaymentDestination Destination,
    string IdempotencyKey,
    Guid? InvoiceId);

public sealed record PaymentIntentResult(string ProviderReference, Uri? HostedPaymentUrl);

public sealed record PaymentWebhookEvent(string ProviderEventId, string EventType, string ProviderReference, decimal Amount, string Currency);

/// <summary>
/// Payment processor adapter (gap G51). Card data stays with the provider's hosted capture.
/// Webhooks must be verified and deduplicated by <see cref="PaymentWebhookEvent.ProviderEventId"/>;
/// the firm is mapped from the merchant account, never from provider metadata alone. No provider is chosen yet.
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(PaymentIntentRequest request, CancellationToken cancellationToken = default);

    Task<PaymentWebhookEvent> VerifyWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken = default);
}
