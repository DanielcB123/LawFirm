using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;

public static class ActorClaims
{
    private static readonly string[] StaffRoles = [AuthRoles.Attorney, AuthRoles.Paralegal, AuthRoles.Admin];

    public static string GetActorId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? string.Empty;
    }

    public static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        return user.Claims.Any(claim =>
            claim.Type == AuthConstants.PermissionClaimType &&
            string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasAnyPermission(ClaimsPrincipal user, params string[] permissions)
    {
        return permissions.Any(permission => HasPermission(user, permission));
    }

    public static bool IsStaff(ClaimsPrincipal user)
    {
        return StaffRoles.Any(user.IsInRole);
    }

    public static bool IsClient(ClaimsPrincipal user)
    {
        return user.IsInRole(AuthRoles.Client);
    }
}
