using Microsoft.AspNetCore.Authorization;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
