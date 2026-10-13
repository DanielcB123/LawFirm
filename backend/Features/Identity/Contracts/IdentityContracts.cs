namespace EnterpriseKnowledgeAssistant.Api.Features.Identity;

public sealed record ActorProfileResponse(
    string ActorId,
    string Email,
    string DisplayName,
    string Role,
    IReadOnlyCollection<string> Permissions,
    string CorrelationId);
