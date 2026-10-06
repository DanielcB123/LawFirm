using EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Features.Parties;

public sealed class PartiesEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var parties = app.MapGroup("/parties")
            .WithTags("Parties");

        parties.MapGet(string.Empty, SearchParties)
            .RequireAuthorization(AuthPolicies.ContactsRead)
            .WithSummary("Searches contacts/organizations with aliases.");

        parties.MapPost(string.Empty, CreateParty)
            .RequireAuthorization(AuthPolicies.ContactsManage)
            .WithSummary("Creates a person or organization party record.");

        parties.MapPost("/{partyId:guid}/aliases", AddAlias)
            .RequireAuthorization(AuthPolicies.ContactsManage)
            .WithSummary("Adds an alias, former name, or trade name to a party.");

        parties.MapPost("/relationships", AddRelationship)
            .RequireAuthorization(AuthPolicies.ContactsManage)
            .WithSummary("Creates a relationship between two parties.");
    }

    private static async Task<IResult> SearchParties(
        string? query,
        LawFirmDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var normalizedQuery = query?.Trim();

        var partyQuery = dbContext.Parties
            .AsNoTracking()
            .Include(party => party.Aliases)
            .OrderBy(party => party.DisplayName)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            var lowered = normalizedQuery.ToLowerInvariant();
            partyQuery = partyQuery.Where(party =>
                party.DisplayName.ToLower().Contains(lowered) ||
                (party.PrimaryEmail != null && party.PrimaryEmail.ToLower().Contains(lowered)) ||
                party.Aliases.Any(alias => alias.Alias.ToLower().Contains(lowered)));
        }

        var parties = await partyQuery
            .Take(100)
            .Select(party => new PartyResponse(
                party.Id,
                party.PartyType,
                party.DisplayName,
                party.PrimaryEmail,
                party.PrimaryPhone,
                party.IsRestricted,
                party.Aliases
                    .OrderBy(alias => alias.Alias)
                    .Select(alias => new PartyAliasResponse(alias.Id, alias.Alias, alias.AliasType))
                    .ToArray()))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(parties);
    }

    private static async Task<IResult> CreateParty(
        CreatePartyRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateCreatePartyRequest(request);
        if (validationErrors.Count > 0)
        {
            return httpContext.ValidationError("Party data is invalid.", validationErrors);
        }

        var party = new Party
        {
            Id = Guid.NewGuid(),
            PartyType = request.PartyType.Trim(),
            DisplayName = request.DisplayName.Trim(),
            PrimaryEmail = request.PrimaryEmail?.Trim(),
            PrimaryPhone = request.PrimaryPhone?.Trim(),
            IsRestricted = request.IsRestricted,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.Parties.Add(party);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "party.created",
            objectType: "party",
            objectId: party.Id.ToString(),
            metadata: new { party.DisplayName, party.PartyType, party.IsRestricted },
            cancellationToken);

        return Results.Created($"/api/parties/{party.Id}", new PartyResponse(
            party.Id,
            party.PartyType,
            party.DisplayName,
            party.PrimaryEmail,
            party.PrimaryPhone,
            party.IsRestricted,
            []));
    }

    private static async Task<IResult> AddAlias(
        Guid partyId,
        AddPartyAliasRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Alias))
        {
            errors["alias"] = ["Alias is required."];
        }

        if (string.IsNullOrWhiteSpace(request.AliasType))
        {
            errors["aliasType"] = ["Alias type is required."];
        }

        if (errors.Count > 0)
        {
            return httpContext.ValidationError("Alias data is invalid.", errors);
        }

        var party = await dbContext.Parties
            .Include(existingParty => existingParty.Aliases)
            .FirstOrDefaultAsync(existingParty => existingParty.Id == partyId, cancellationToken);
        if (party is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("party_not_found", "Party was not found.")));
        }

        var alias = new PartyAlias
        {
            Id = Guid.NewGuid(),
            PartyId = party.Id,
            Alias = request.Alias.Trim(),
            AliasType = request.AliasType.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.PartyAliases.Add(alias);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "party.alias_added",
            objectType: "party",
            objectId: party.Id.ToString(),
            metadata: new { alias.Alias, alias.AliasType },
            cancellationToken);

        return Results.Created($"/api/parties/{party.Id}/aliases/{alias.Id}", new PartyAliasResponse(alias.Id, alias.Alias, alias.AliasType));
    }

    private static async Task<IResult> AddRelationship(
        AddPartyRelationshipRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.FromPartyId == Guid.Empty)
        {
            errors["fromPartyId"] = ["From party is required."];
        }

        if (request.ToPartyId == Guid.Empty)
        {
            errors["toPartyId"] = ["To party is required."];
        }

        if (request.FromPartyId == request.ToPartyId)
        {
            errors["toPartyId"] = ["Cannot create a relationship from a party to itself."];
        }

        if (string.IsNullOrWhiteSpace(request.RelationshipType))
        {
            errors["relationshipType"] = ["Relationship type is required."];
        }

        if (errors.Count > 0)
        {
            return httpContext.ValidationError("Relationship data is invalid.", errors);
        }

        var fromExists = await dbContext.Parties.AnyAsync(party => party.Id == request.FromPartyId, cancellationToken);
        var toExists = await dbContext.Parties.AnyAsync(party => party.Id == request.ToPartyId, cancellationToken);
        if (!fromExists || !toExists)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("party_not_found", "One or both parties were not found.")));
        }

        var relationship = new PartyRelationship
        {
            Id = Guid.NewGuid(),
            FromPartyId = request.FromPartyId,
            ToPartyId = request.ToPartyId,
            RelationshipType = request.RelationshipType.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.PartyRelationships.Add(relationship);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "party.relationship_added",
            objectType: "party_relationship",
            objectId: relationship.Id.ToString(),
            metadata: new { relationship.FromPartyId, relationship.ToPartyId, relationship.RelationshipType },
            cancellationToken);

        return Results.Created($"/api/parties/relationships/{relationship.Id}", new PartyRelationshipResponse(
            relationship.Id,
            relationship.FromPartyId,
            relationship.ToPartyId,
            relationship.RelationshipType));
    }

    private static Dictionary<string, string[]> ValidateCreatePartyRequest(CreatePartyRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors["displayName"] = ["Display name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.PartyType))
        {
            errors["partyType"] = ["Party type is required."];
        }
        else if (!string.Equals(request.PartyType, "Person", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(request.PartyType, "Organization", StringComparison.OrdinalIgnoreCase))
        {
            errors["partyType"] = ["Party type must be either Person or Organization."];
        }

        return errors;
    }
}

public sealed record CreatePartyRequest(
    string DisplayName,
    string PartyType,
    string? PrimaryEmail,
    string? PrimaryPhone,
    bool IsRestricted);

public sealed record AddPartyAliasRequest(
    string Alias,
    string AliasType);

public sealed record AddPartyRelationshipRequest(
    Guid FromPartyId,
    Guid ToPartyId,
    string RelationshipType);

public sealed record PartyResponse(
    Guid Id,
    string PartyType,
    string DisplayName,
    string? PrimaryEmail,
    string? PrimaryPhone,
    bool IsRestricted,
    IReadOnlyCollection<PartyAliasResponse> Aliases);

public sealed record PartyAliasResponse(
    Guid Id,
    string Alias,
    string AliasType);

public sealed record PartyRelationshipResponse(
    Guid Id,
    Guid FromPartyId,
    Guid ToPartyId,
    string RelationshipType);
