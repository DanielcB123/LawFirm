using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;

/// <summary>
/// Single source of matter-level access decisions. Policy (see Markdown/security/matter-access-policy.md):
/// staff read/write a matter when they hold the firm-wide permission, or the assigned permission plus
/// assignment as responsible attorney/paralegal or an unrevoked Staff grant. Clients reach a matter only
/// through an unrevoked Client grant and only through client-portal routes.
/// </summary>
public interface IMatterAccessService
{
    IQueryable<MatterRecord> ReadableMatters(ClaimsPrincipal user);

    IQueryable<MatterRecord> WritableMatters(ClaimsPrincipal user);

    IQueryable<MatterRecord> ClientMatters(ClaimsPrincipal user);

    Task<bool> CanReadAsync(ClaimsPrincipal user, Guid matterId, CancellationToken cancellationToken);

    Task<bool> CanWriteAsync(ClaimsPrincipal user, Guid matterId, CancellationToken cancellationToken);

    Task<bool> CanManageGrantsAsync(ClaimsPrincipal user, Guid matterId, CancellationToken cancellationToken);

    Task<bool> IsClientOfAsync(ClaimsPrincipal user, Guid matterId, CancellationToken cancellationToken);
}

public sealed class MatterAccessService(LawFirmDbContext dbContext) : IMatterAccessService
{
    public IQueryable<MatterRecord> ReadableMatters(ClaimsPrincipal user)
    {
        return StaffScoped(user, AuthPermissions.MattersReadAll, AuthPermissions.MattersReadAssigned);
    }

    public IQueryable<MatterRecord> WritableMatters(ClaimsPrincipal user)
    {
        return StaffScoped(user, AuthPermissions.MattersUpdateAll, AuthPermissions.MattersUpdateAssigned);
    }

    public IQueryable<MatterRecord> ClientMatters(ClaimsPrincipal user)
    {
        var actorId = ActorClaims.GetActorId(user);
        if (actorId.Length == 0 ||
            !ActorClaims.IsClient(user) ||
            !ActorClaims.HasPermission(user, AuthPermissions.MattersReadOwn))
        {
            return None();
        }

        return dbContext.MatterRecords.Where(matter => dbContext.MatterAccessGrants.Any(grant =>
            grant.MatterRecordId == matter.Id &&
            grant.ActorId == actorId &&
            grant.GrantType == MatterAccessGrantTypes.Client &&
            grant.RevokedAtUtc == null));
    }

    public Task<bool> CanReadAsync(ClaimsPrincipal user, Guid matterId, CancellationToken cancellationToken)
    {
        return ReadableMatters(user).AnyAsync(matter => matter.Id == matterId, cancellationToken);
    }

    public Task<bool> CanWriteAsync(ClaimsPrincipal user, Guid matterId, CancellationToken cancellationToken)
    {
        return WritableMatters(user).AnyAsync(matter => matter.Id == matterId, cancellationToken);
    }

    public async Task<bool> CanManageGrantsAsync(ClaimsPrincipal user, Guid matterId, CancellationToken cancellationToken)
    {
        if (!user.IsInRole(AuthRoles.Attorney))
        {
            return false;
        }

        var actorId = ActorClaims.GetActorId(user);
        var firmWide = ActorClaims.HasPermission(user, AuthPermissions.MattersUpdateAll);
        return await WritableMatters(user).AnyAsync(
            matter => matter.Id == matterId && (firmWide || matter.ResponsibleAttorneyActorId == actorId),
            cancellationToken);
    }

    public Task<bool> IsClientOfAsync(ClaimsPrincipal user, Guid matterId, CancellationToken cancellationToken)
    {
        return ClientMatters(user).AnyAsync(matter => matter.Id == matterId, cancellationToken);
    }

    private IQueryable<MatterRecord> StaffScoped(ClaimsPrincipal user, string firmWidePermission, string assignedPermission)
    {
        var actorId = ActorClaims.GetActorId(user);
        if (actorId.Length == 0 || !ActorClaims.IsStaff(user))
        {
            return None();
        }

        if (ActorClaims.HasPermission(user, firmWidePermission))
        {
            return dbContext.MatterRecords;
        }

        if (!ActorClaims.HasPermission(user, assignedPermission))
        {
            return None();
        }

        return dbContext.MatterRecords.Where(matter =>
            matter.ResponsibleAttorneyActorId == actorId ||
            matter.ParalegalActorId == actorId ||
            dbContext.MatterAccessGrants.Any(grant =>
                grant.MatterRecordId == matter.Id &&
                grant.ActorId == actorId &&
                grant.GrantType == MatterAccessGrantTypes.Staff &&
                grant.RevokedAtUtc == null));
    }

    private IQueryable<MatterRecord> None()
    {
        return dbContext.MatterRecords.Where(_ => false);
    }
}
