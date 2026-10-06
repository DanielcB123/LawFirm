namespace EnterpriseKnowledgeAssistant.Api.Features.Auth;

public sealed record UserProfileResponse(
    string Id,
    string Email,
    string DisplayName,
    string Role,
    IReadOnlyCollection<string> Permissions);
