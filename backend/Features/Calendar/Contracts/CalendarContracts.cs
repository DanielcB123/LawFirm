namespace EnterpriseKnowledgeAssistant.Api.Features.Calendar;

public interface IWriteCalendarEntryRequest
{
    string Title { get; }
    string EntryType { get; }
    string DeadlineType { get; }
    string OwnerActorId { get; }
    string OwnerDisplayName { get; }
    string? BackupActorId { get; }
    string? BackupDisplayName { get; }
    DateTimeOffset ScheduledAtUtc { get; }
    bool IsAllDay { get; }
    string TimeZone { get; }
    string? MatterReference { get; }
    string? SourceReference { get; }
    DateTimeOffset? VerifiedAtUtc { get; }
    string? VerifiedByActorId { get; }
    string? VerifiedByDisplayName { get; }
    string? OverrideReason { get; }
    int[]? ReminderOffsetsMinutes { get; }
}

public sealed record CreateCalendarEntryRequest(
    string Title,
    string EntryType,
    string DeadlineType,
    string OwnerActorId,
    string OwnerDisplayName,
    string? BackupActorId,
    string? BackupDisplayName,
    DateTimeOffset ScheduledAtUtc,
    bool IsAllDay,
    string TimeZone,
    string? MatterReference,
    string? SourceReference,
    DateTimeOffset? VerifiedAtUtc,
    string? VerifiedByActorId,
    string? VerifiedByDisplayName,
    string? OverrideReason,
    int[]? ReminderOffsetsMinutes) : IWriteCalendarEntryRequest;

public sealed record UpdateCalendarEntryRequest(
    string Title,
    string EntryType,
    string DeadlineType,
    string OwnerActorId,
    string OwnerDisplayName,
    string? BackupActorId,
    string? BackupDisplayName,
    DateTimeOffset ScheduledAtUtc,
    bool IsAllDay,
    string TimeZone,
    string? MatterReference,
    string? SourceReference,
    DateTimeOffset? VerifiedAtUtc,
    string? VerifiedByActorId,
    string? VerifiedByDisplayName,
    string? OverrideReason,
    int[]? ReminderOffsetsMinutes) : IWriteCalendarEntryRequest;

public sealed record CalendarEntryResponse(
    Guid Id,
    string Title,
    string EntryType,
    string DeadlineType,
    string OwnerActorId,
    string OwnerDisplayName,
    string? BackupActorId,
    string? BackupDisplayName,
    DateTimeOffset ScheduledAtUtc,
    bool IsAllDay,
    string TimeZone,
    string? MatterReference,
    string? SourceReference,
    DateTimeOffset? VerifiedAtUtc,
    string? VerifiedByActorId,
    string? VerifiedByDisplayName,
    string? OverrideReason,
    bool IsAcknowledged,
    DateTimeOffset? AcknowledgedAtUtc,
    bool IsCompleted,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyCollection<int> ReminderOffsetsMinutes,
    DateTimeOffset CreatedAtUtc);
