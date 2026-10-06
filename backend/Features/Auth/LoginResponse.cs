namespace EnterpriseKnowledgeAssistant.Api.Features.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc,
    UserProfileResponse User);
