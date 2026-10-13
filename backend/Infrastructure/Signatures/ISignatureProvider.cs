namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Signatures;

public sealed record SignatureRequest(Guid DocumentVersionId, string DocumentSha256, IReadOnlyList<SignatureSigner> Signers, string IdempotencyKey);

public sealed record SignatureSigner(string Name, string Email, int RoutingOrder);

public sealed record SignatureEnvelope(string ProviderEnvelopeId, string Status);

/// <summary>
/// E-signature adapter for engagement letters and fee agreements (gap G24). The signed hash must match the
/// exact document version that was approved. No provider is chosen or registered yet.
/// </summary>
public interface ISignatureProvider
{
    Task<SignatureEnvelope> CreateEnvelopeAsync(SignatureRequest request, CancellationToken cancellationToken = default);

    Task<SignatureEnvelope> GetEnvelopeAsync(string providerEnvelopeId, CancellationToken cancellationToken = default);
}
