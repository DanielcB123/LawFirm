using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;

/// <summary>
/// Restricted parties are visible only to holders of <see cref="AuthPermissions.ContactsReadRestricted"/>.
/// Directory and write paths treat a restricted party as nonexistent for everyone else; conflict search
/// reports that a restricted match exists without identifying it.
/// </summary>
public static class PartyVisibility
{
    public const string MaskedDisplayName = "Restricted Party";
    public const string MaskedPartyType = "Restricted";
    public const string MaskedMatchReason = "Match found in restricted records. Ask an authorized attorney to review details.";

    public static bool CanViewRestricted(ClaimsPrincipal user)
    {
        return ActorClaims.HasPermission(user, AuthPermissions.ContactsReadRestricted);
    }

    public static IQueryable<Party> VisibleParties(IQueryable<Party> parties, ClaimsPrincipal user)
    {
        return CanViewRestricted(user) ? parties : parties.Where(party => !party.IsRestricted);
    }

    public static bool IsVisible(Party party, ClaimsPrincipal user)
    {
        return !party.IsRestricted || CanViewRestricted(user);
    }
}
