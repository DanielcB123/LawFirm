namespace EnterpriseKnowledgeAssistant.Api.Features.Matters;

public sealed record MatterListItemResponse(
    Guid Id,
    string MatterNumber,
    string Title,
    string PracticeArea,
    string Stage,
    string Status,
    string ResponsibleAttorneyActorId,
    string ResponsibleAttorneyDisplayName,
    string? ParalegalActorId,
    string? ParalegalDisplayName,
    Guid? LinkedIntakeRecordId,
    DateTimeOffset CreatedAtUtc);

public sealed record MatterStageHistoryResponse(
    Guid Id,
    string FromStage,
    string ToStage,
    string ChangedByActorId,
    string ChangedByDisplayName,
    string? ChangeReason,
    DateTimeOffset ChangedAtUtc);

public sealed record MatterDetailResponse(
    Guid Id,
    string MatterNumber,
    string Title,
    string PracticeArea,
    string Stage,
    string Status,
    string ResponsibleAttorneyActorId,
    string ResponsibleAttorneyDisplayName,
    string? ParalegalActorId,
    string? ParalegalDisplayName,
    Guid? LinkedIntakeRecordId,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyCollection<MatterStageHistoryResponse> StageHistory);

public sealed record MatterTaskResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    string? AssigneeActorId,
    string? AssigneeDisplayName,
    DateTimeOffset? DueAtUtc,
    bool IsDeadlineVerified,
    DateTimeOffset? DeadlineVerifiedAtUtc,
    string? DeadlineVerifiedByActorId,
    string? DeadlineVerifiedByDisplayName,
    DateTimeOffset? CompletedAtUtc,
    string CreatedByActorId,
    string CreatedByDisplayName,
    DateTimeOffset CreatedAtUtc);

public sealed record CreateMatterTaskRequest(
    string Title,
    string? Description,
    string? Status,
    string? Priority,
    string? AssigneeActorId,
    string? AssigneeDisplayName,
    DateTimeOffset? DueAtUtc,
    bool IsDeadlineVerified);

public sealed record UpdateMatterTaskRequest(
    string? Title,
    string? Description,
    string? Status,
    string? Priority,
    string? AssigneeActorId,
    string? AssigneeDisplayName,
    DateTimeOffset? DueAtUtc,
    bool? IsDeadlineVerified);

public sealed record MatterDocumentResponse(
    Guid Id,
    string FileName,
    string DocumentType,
    string? StorageKey,
    string UploadedByActorId,
    string UploadedByDisplayName,
    DateTimeOffset UploadedAtUtc,
    string ReviewStatus,
    string? ReviewNotes,
    string? ReviewedByActorId,
    string? ReviewedByDisplayName,
    DateTimeOffset? ReviewedAtUtc);

public sealed record CreateMatterDocumentRequest(
    string FileName,
    string DocumentType,
    string? StorageKey);

public sealed record ReviewMatterDocumentRequest(
    string ReviewStatus,
    string? ReviewNotes);

public sealed record MatterTimelineActionResponse(
    Guid Id,
    string ActionType,
    string Summary,
    string? Details,
    DateTimeOffset OccurredAtUtc,
    string CreatedByActorId,
    string CreatedByDisplayName,
    DateTimeOffset CreatedAtUtc);

public sealed record CreateMatterTimelineActionRequest(
    string ActionType,
    string Summary,
    string? Details,
    DateTimeOffset? OccurredAtUtc);
