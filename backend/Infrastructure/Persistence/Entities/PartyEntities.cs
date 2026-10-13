namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public sealed class Party
{
    public Guid Id { get; set; }
    public string PartyType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public bool IsRestricted { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public List<PartyAlias> Aliases { get; set; } = [];
}

public sealed class PartyAlias
{
    public Guid Id { get; set; }
    public Guid PartyId { get; set; }
    public Party Party { get; set; } = null!;
    public string Alias { get; set; } = string.Empty;
    public string AliasType { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class PartyRelationship
{
    public Guid Id { get; set; }
    public Guid FromPartyId { get; set; }
    public Party FromParty { get; set; } = null!;
    public Guid ToPartyId { get; set; }
    public Party ToParty { get; set; } = null!;
    public string RelationshipType { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
