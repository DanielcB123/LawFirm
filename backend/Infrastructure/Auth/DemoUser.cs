namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;

public sealed record DemoUser(
    string Id,
    string Email,
    string DisplayName,
    string Role,
    string PasswordHash,
    IReadOnlyCollection<string> Permissions);
