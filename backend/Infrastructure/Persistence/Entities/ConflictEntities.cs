namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public sealed class ConflictCheck
{
    public Guid Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public string RequestedByActorId { get; set; } = string.Empty;
    public string RequestedByActorDisplayName { get; set; } = string.Empty;
    public string MatchSnapshotJson { get; set; } = string.Empty;
    public DateTimeOffset RequestedAtUtc { get; set; }
    public List<ConflictDecision> Decisions { get; set; } = [];
}

public sealed class ConflictDecision
{
    public Guid Id { get; set; }
    public Guid ConflictCheckId { get; set; }
    public ConflictCheck ConflictCheck { get; set; } = null!;
    public string Decision { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string DecidedByActorId { get; set; } = string.Empty;
    public string DecidedByActorDisplayName { get; set; } = string.Empty;
    public string MatchSnapshotJson { get; set; } = string.Empty;
    public DateTimeOffset DecidedAtUtc { get; set; }
}
