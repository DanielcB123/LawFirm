namespace EnterpriseKnowledgeAssistant.Api.Features.Conflicts;

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
