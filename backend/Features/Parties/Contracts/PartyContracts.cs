namespace EnterpriseKnowledgeAssistant.Api.Features.Parties;

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
