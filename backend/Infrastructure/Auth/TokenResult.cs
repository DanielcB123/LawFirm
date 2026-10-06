namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;

public sealed record TokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string TokenType = "Bearer");
