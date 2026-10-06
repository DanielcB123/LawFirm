using System.Security.Claims;
using System.Text.Json;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Features.Conflicts;

public sealed class ConflictEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var conflicts = app.MapGroup("/conflicts")
            .WithTags("Conflicts");

        conflicts.MapPost("/checks", RunConflictCheck)
            .RequireAuthorization(AuthPolicies.ConflictsSearch)
            .WithSummary("Runs a conflict search and stores a retained check snapshot.");

        conflicts.MapPost("/checks/{checkId:guid}/decisions", RecordConflictDecision)
            .RequireAuthorization(AuthPolicies.ConflictsReview)
            .WithSummary("Records attorney/admin conflict review decision with rationale and snapshot.");
    }

    private static async Task<IResult> RunConflictCheck(
        ConflictCheckRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return httpContext.ValidationError(
                "Conflict check request is invalid.",
                new Dictionary<string, string[]>
                {
                    ["query"] = ["Search query is required."]
                });
        }

        var normalizedQuery = request.Query.Trim();
        var loweredQuery = normalizedQuery.ToLowerInvariant();
        var canSeeRestrictedDetails = user.IsInRole(AuthRoles.Attorney) || user.IsInRole(AuthRoles.Admin);

        var parties = await dbContext.Parties
            .AsNoTracking()
            .Include(party => party.Aliases)
            .OrderBy(party => party.DisplayName)
            .ToListAsync(cancellationToken);

        var matches = new List<ConflictMatchResponse>();
        foreach (var party in parties)
        {
            var matchReason = ResolveMatchReason(party, loweredQuery);
            if (matchReason is null)
            {
                continue;
            }

            var similarityScore = ComputeSimilarityScore(loweredQuery, party.DisplayName.ToLowerInvariant());
            var safeName = party.IsRestricted && !canSeeRestrictedDetails
                ? "Restricted Party"
                : party.DisplayName;
            var safeReason = party.IsRestricted && !canSeeRestrictedDetails
                ? "Match found in restricted records. Ask an attorney/admin to review details."
                : matchReason;

            matches.Add(new ConflictMatchResponse(
                party.Id,
                safeName,
                party.PartyType,
                party.IsRestricted,
                Math.Round(similarityScore, 3),
                safeReason));
        }

        matches = matches
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Take(75)
            .ToList();

        var actorId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "unknown";
        var actorDisplayName = user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var check = new ConflictCheck
        {
            Id = Guid.NewGuid(),
            Query = normalizedQuery,
            RequestedByActorId = actorId,
            RequestedByActorDisplayName = actorDisplayName,
            MatchSnapshotJson = JsonSerializer.Serialize(matches),
            RequestedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.ConflictChecks.Add(check);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "conflict.check_created",
            objectType: "conflict_check",
            objectId: check.Id.ToString(),
            metadata: new { check.Query, matchCount = matches.Count },
            cancellationToken);

        return Results.Ok(new ConflictCheckResponse(
            check.Id,
            check.Query,
            check.RequestedAtUtc,
            matches,
            httpContext.GetCorrelationId()));
    }

    private static async Task<IResult> RecordConflictDecision(
        Guid checkId,
        ConflictDecisionRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Decision) || string.IsNullOrWhiteSpace(request.Rationale))
        {
            return httpContext.ValidationError(
                "Conflict decision request is invalid.",
                new Dictionary<string, string[]>
                {
                    ["decision"] = ["Decision is required."],
                    ["rationale"] = ["Rationale is required."]
                });
        }

        var validDecisions = new[] { "Approved", "Rejected", "NeedsMoreInfo" };
        if (!validDecisions.Contains(request.Decision.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return httpContext.ValidationError(
                "Conflict decision request is invalid.",
                new Dictionary<string, string[]>
                {
                    ["decision"] = ["Decision must be Approved, Rejected, or NeedsMoreInfo."]
                });
        }

        var check = await dbContext.ConflictChecks
            .AsNoTracking()
            .FirstOrDefaultAsync(existingCheck => existingCheck.Id == checkId, cancellationToken);
        if (check is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("conflict_check_not_found", "Conflict check was not found.")));
        }

        var actorId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "unknown";
        var actorDisplayName = user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var decision = new ConflictDecision
        {
            Id = Guid.NewGuid(),
            ConflictCheckId = check.Id,
            Decision = request.Decision.Trim(),
            Rationale = request.Rationale.Trim(),
            DecidedByActorId = actorId,
            DecidedByActorDisplayName = actorDisplayName,
            MatchSnapshotJson = check.MatchSnapshotJson,
            DecidedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.ConflictDecisions.Add(decision);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "conflict.decision_recorded",
            objectType: "conflict_decision",
            objectId: decision.Id.ToString(),
            metadata: new { decision.ConflictCheckId, decision.Decision },
            cancellationToken);

        return Results.Ok(new ConflictDecisionResponse(
            decision.Id,
            decision.ConflictCheckId,
            decision.Decision,
            decision.Rationale,
            decision.DecidedByActorDisplayName,
            decision.DecidedAtUtc));
    }

    private static string? ResolveMatchReason(Party party, string loweredQuery)
    {
        if (party.DisplayName.ToLowerInvariant().Contains(loweredQuery))
        {
            return "Display name partial/exact match";
        }

        if (!string.IsNullOrWhiteSpace(party.PrimaryEmail) &&
            party.PrimaryEmail.ToLowerInvariant().Contains(loweredQuery))
        {
            return "Primary email match";
        }

        var aliasMatch = party.Aliases.FirstOrDefault(alias => alias.Alias.ToLowerInvariant().Contains(loweredQuery));
        if (aliasMatch is not null)
        {
            return $"Alias match ({aliasMatch.AliasType})";
        }

        var nameScore = ComputeSimilarityScore(loweredQuery, party.DisplayName.ToLowerInvariant());
        if (nameScore >= 0.72)
        {
            return "Approximate name match";
        }

        return null;
    }

    private static double ComputeSimilarityScore(string valueA, string valueB)
    {
        if (string.Equals(valueA, valueB, StringComparison.OrdinalIgnoreCase))
        {
            return 1d;
        }

        var intersection = BuildBigrams(valueA).Intersect(BuildBigrams(valueB)).Count();
        var total = BuildBigrams(valueA).Count + BuildBigrams(valueB).Count;
        if (total == 0)
        {
            return 0d;
        }

        return (2d * intersection) / total;
    }

    private static HashSet<string> BuildBigrams(string value)
    {
        var cleaned = new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        if (cleaned.Length < 2)
        {
            return [cleaned];
        }

        var set = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < cleaned.Length - 1; index++)
        {
            set.Add(cleaned.Substring(index, 2));
        }

        return set;
    }
}

public sealed record ConflictCheckRequest(string Query);

public sealed record ConflictDecisionRequest(
    string Decision,
    string Rationale);

public sealed record ConflictCheckResponse(
    Guid CheckId,
    string Query,
    DateTimeOffset RequestedAtUtc,
    IReadOnlyCollection<ConflictMatchResponse> Matches,
    string CorrelationId);

public sealed record ConflictMatchResponse(
    Guid PartyId,
    string DisplayName,
    string PartyType,
    bool IsRestricted,
    double Score,
    string MatchReason);

public sealed record ConflictDecisionResponse(
    Guid ConflictDecisionId,
    Guid ConflictCheckId,
    string Decision,
    string Rationale,
    string DecidedBy,
    DateTimeOffset DecidedAtUtc);
