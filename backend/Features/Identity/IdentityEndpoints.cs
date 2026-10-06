using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;

namespace EnterpriseKnowledgeAssistant.Api.Features.Identity;

public sealed class IdentityEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/identity/me", (ClaimsPrincipal user, HttpContext context) =>
            {
                var permissions = user.Claims
                    .Where(claim => claim.Type == AuthConstants.PermissionClaimType)
                    .Select(claim => claim.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToArray();

                var response = new ActorProfileResponse(
                    user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? string.Empty,
                    user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email") ?? string.Empty,
                    user.FindFirstValue("name") ?? string.Empty,
                    user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("role") ?? string.Empty,
                    permissions,
                    context.GetCorrelationId());

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Identity")
            .WithSummary("Returns actor identity details and granted permissions.");
    }
}

public sealed record ActorProfileResponse(
    string ActorId,
    string Email,
    string DisplayName,
    string Role,
    IReadOnlyCollection<string> Permissions,
    string CorrelationId);
