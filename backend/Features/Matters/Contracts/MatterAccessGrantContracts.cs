namespace EnterpriseKnowledgeAssistant.Api.Features.Matters;

public sealed record CreateMatterAccessGrantRequest(string? ActorId, string? GrantType);

public sealed record MatterAccessGrantResponse(
    Guid Id,
    Guid MatterId,
    string ActorId,
    string GrantType,
    string GrantedByActorId,
    DateTimeOffset GrantedAtUtc,
    string? RevokedByActorId,
    DateTimeOffset? RevokedAtUtc);
