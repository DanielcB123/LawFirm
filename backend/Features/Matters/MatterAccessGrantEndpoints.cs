using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Features.Matters;

public static class MatterAccessGrantEndpoints
{
    private static readonly string[] AllowedGrantTypes = [MatterAccessGrantTypes.Client, MatterAccessGrantTypes.Staff];

    public static async Task<IResult> ListGrants(
        Guid matterId,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await access.CanReadAsync(user, matterId, cancellationToken))
        {
            return MatterAccessResults.MatterNotFound(httpContext);
        }

        var grants = await dbContext.MatterAccessGrants
            .AsNoTracking()
            .Where(grant => grant.MatterRecordId == matterId)
            .OrderByDescending(grant => grant.GrantedAtUtc)
            .ToListAsync(cancellationToken);

        return Results.Ok(grants.Select(ToResponse).ToArray());
    }

    public static async Task<IResult> CreateGrant(
        Guid matterId,
        CreateMatterAccessGrantRequest request,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (await DenyUnlessGrantManagerAsync(access, user, matterId, httpContext, cancellationToken) is { } denied)
        {
            return denied;
        }

        var actorId = request.ActorId?.Trim() ?? string.Empty;
        var grantType = AllowedGrantTypes.FirstOrDefault(type => string.Equals(type, request.GrantType?.Trim(), StringComparison.OrdinalIgnoreCase));
        var errors = new Dictionary<string, string[]>();
        if (actorId.Length is 0 or > 128)
        {
            errors["actorId"] = ["Actor id is required and must be at most 128 characters."];
        }

        if (grantType is null)
        {
            errors["grantType"] = ["Grant type must be Client or Staff."];
        }

        if (errors.Count > 0)
        {
            return httpContext.ValidationError("Access grant request is invalid.", errors);
        }

        var alreadyActive = await dbContext.MatterAccessGrants.AnyAsync(
            grant => grant.MatterRecordId == matterId && grant.ActorId == actorId && grant.RevokedAtUtc == null,
            cancellationToken);
        if (alreadyActive)
        {
            return GrantExists(httpContext);
        }

        var grant = new MatterAccessGrant
        {
            Id = Guid.NewGuid(),
            MatterRecordId = matterId,
            ActorId = actorId,
            GrantType = grantType!,
            GrantedByActorId = ActorClaims.GetActorId(user),
            GrantedAtUtc = DateTimeOffset.UtcNow
        };
        dbContext.MatterAccessGrants.Add(grant);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The unique index on (matter, actor, active marker) also catches concurrent duplicate grants.
            return GrantExists(httpContext);
        }

        await auditTrailService.RecordAsync(
            httpContext,
            action: "matter.access_grant_created",
            objectType: "matter_access_grant",
            objectId: grant.Id.ToString(),
            metadata: new { matterId, grant.ActorId, grant.GrantType },
            cancellationToken);

        return Results.Created($"/api/matters-v2/{matterId}/access-grants/{grant.Id}", ToResponse(grant));
    }

    public static async Task<IResult> RevokeGrant(
        Guid matterId,
        Guid grantId,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (await DenyUnlessGrantManagerAsync(access, user, matterId, httpContext, cancellationToken) is { } denied)
        {
            return denied;
        }

        var grant = await dbContext.MatterAccessGrants
            .FirstOrDefaultAsync(item => item.Id == grantId && item.MatterRecordId == matterId, cancellationToken);
        if (grant is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("matter_access_grant_not_found", "Access grant was not found.")));
        }

        if (grant.RevokedAtUtc is not null)
        {
            return Results.Ok(ToResponse(grant));
        }

        grant.RevokedAtUtc = DateTimeOffset.UtcNow;
        grant.RevokedByActorId = ActorClaims.GetActorId(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "matter.access_grant_revoked",
            objectType: "matter_access_grant",
            objectId: grant.Id.ToString(),
            metadata: new { matterId, grant.ActorId, grant.GrantType },
            cancellationToken);

        return Results.Ok(ToResponse(grant));
    }

    private static async Task<IResult?> DenyUnlessGrantManagerAsync(
        IMatterAccessService access,
        ClaimsPrincipal user,
        Guid matterId,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (await MatterAccessResults.DenyUnlessWritableAsync(access, user, matterId, httpContext, cancellationToken) is { } denied)
        {
            return denied;
        }

        return await access.CanManageGrantsAsync(user, matterId, cancellationToken)
            ? null
            : Results.Json(
                new ApiErrorResponse(
                    httpContext.GetCorrelationId(),
                    new ApiError("matter_grant_forbidden", "Only the responsible attorney can manage access to this matter.")),
                statusCode: StatusCodes.Status403Forbidden);
    }

    private static IResult GrantExists(HttpContext httpContext)
    {
        return Results.Conflict(new ApiErrorResponse(
            httpContext.GetCorrelationId(),
            new ApiError("matter_access_grant_exists", "This person already has active access to the matter.")));
    }

    private static MatterAccessGrantResponse ToResponse(MatterAccessGrant grant)
    {
        return new MatterAccessGrantResponse(
            grant.Id,
            grant.MatterRecordId,
            grant.ActorId,
            grant.GrantType,
            grant.GrantedByActorId,
            grant.GrantedAtUtc,
            grant.RevokedByActorId,
            grant.RevokedAtUtc);
    }
}
