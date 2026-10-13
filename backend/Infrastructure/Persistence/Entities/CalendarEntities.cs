namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public sealed class CalendarEntry
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string EntryType { get; set; } = "Task";
    public string DeadlineType { get; set; } = "InternalTarget";
    public string OwnerActorId { get; set; } = string.Empty;
    public string OwnerDisplayName { get; set; } = string.Empty;
    public string? BackupActorId { get; set; }
    public string? BackupDisplayName { get; set; }
    public DateTimeOffset ScheduledAtUtc { get; set; }
    public bool IsAllDay { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string? MatterReference { get; set; }
    public string? SourceReference { get; set; }
    public DateTimeOffset? VerifiedAtUtc { get; set; }
    public string? VerifiedByActorId { get; set; }
    public string? VerifiedByDisplayName { get; set; }
    public string? OverrideReason { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTimeOffset? AcknowledgedAtUtc { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string RemindersJson { get; set; } = "[]";
    public string CreatedByActorId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
