namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public sealed class MatterRecord
{
    public Guid Id { get; set; }
    public string MatterNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PracticeArea { get; set; } = string.Empty;
    public string Stage { get; set; } = "IntakeAccepted";
    public string Status { get; set; } = "Active";
    public string ResponsibleAttorneyActorId { get; set; } = string.Empty;
    public string ResponsibleAttorneyDisplayName { get; set; } = string.Empty;
    public string? ParalegalActorId { get; set; }
    public string? ParalegalDisplayName { get; set; }
    public Guid? LinkedIntakeRecordId { get; set; }
    public string CreatedByActorId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public List<MatterStageHistory> StageHistory { get; set; } = [];
    public List<MatterTask> Tasks { get; set; } = [];
    public List<MatterDocument> Documents { get; set; } = [];
    public List<MatterTimelineAction> TimelineActions { get; set; } = [];
}

public sealed class MatterStageHistory
{
    public Guid Id { get; set; }
    public Guid MatterRecordId { get; set; }
    public MatterRecord MatterRecord { get; set; } = null!;
    public string FromStage { get; set; } = string.Empty;
    public string ToStage { get; set; } = string.Empty;
    public string ChangedByActorId { get; set; } = string.Empty;
    public string ChangedByDisplayName { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
}

public sealed class MatterTask
{
    public Guid Id { get; set; }
    public Guid MatterRecordId { get; set; }
    public MatterRecord MatterRecord { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Normal";
    public string? AssigneeActorId { get; set; }
    public string? AssigneeDisplayName { get; set; }
    public DateTimeOffset? DueAtUtc { get; set; }
    public bool IsDeadlineVerified { get; set; }
    public DateTimeOffset? DeadlineVerifiedAtUtc { get; set; }
    public string? DeadlineVerifiedByActorId { get; set; }
    public string? DeadlineVerifiedByDisplayName { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string CreatedByActorId { get; set; } = string.Empty;
    public string CreatedByDisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class MatterDocument
{
    public Guid Id { get; set; }
    public Guid MatterRecordId { get; set; }
    public MatterRecord MatterRecord { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = "General";
    public string? StorageKey { get; set; }
    public string UploadedByActorId { get; set; } = string.Empty;
    public string UploadedByDisplayName { get; set; } = string.Empty;
    public DateTimeOffset UploadedAtUtc { get; set; }
    public string ReviewStatus { get; set; } = "Pending";
    public string? ReviewNotes { get; set; }
    public string? ReviewedByActorId { get; set; }
    public string? ReviewedByDisplayName { get; set; }
    public DateTimeOffset? ReviewedAtUtc { get; set; }
}

public sealed class MatterTimelineAction
{
    public Guid Id { get; set; }
    public Guid MatterRecordId { get; set; }
    public MatterRecord MatterRecord { get; set; } = null!;
    public string ActionType { get; set; } = "Note";
    public string Summary { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string CreatedByActorId { get; set; } = string.Empty;
    public string CreatedByDisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public static class MatterAccessGrantTypes
{
    public const string Client = "Client";
    public const string Staff = "Staff";
}

public sealed class MatterAccessGrant
{
    public Guid Id { get; set; }
    public Guid MatterRecordId { get; set; }
    public MatterRecord MatterRecord { get; set; } = null!;
    public string ActorId { get; set; } = string.Empty;
    public string GrantType { get; set; } = MatterAccessGrantTypes.Staff;
    public string GrantedByActorId { get; set; } = string.Empty;
    public DateTimeOffset GrantedAtUtc { get; set; }
    public string? RevokedByActorId { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public int? ActiveMarker { get; private set; }
}
