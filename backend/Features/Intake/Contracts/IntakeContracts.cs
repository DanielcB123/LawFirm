namespace EnterpriseKnowledgeAssistant.Api.Features.Intake;

public sealed record IntakeListItemResponse(
    Guid Id,
    string ProspectiveClientName,
    string PracticeArea,
    string IntakeType,
    string Stage,
    string OwnerActorId,
    string OwnerDisplayName,
    string? NextAction,
    DateTimeOffset? NextActionDueAtUtc,
    string? ReferralSource,
    Guid? ConflictCheckId,
    Guid? ApprovedConflictDecisionId,
    DateTimeOffset CreatedAtUtc);

public sealed record IntakeStageHistoryResponse(
    Guid Id,
    string FromStage,
    string ToStage,
    string ChangedByActorId,
    string ChangedByDisplayName,
    string? ChangeReason,
    DateTimeOffset ChangedAtUtc);

public sealed record IntakeNoteResponse(
    Guid Id,
    string Note,
    string CreatedByActorId,
    string CreatedByDisplayName,
    DateTimeOffset CreatedAtUtc);

public sealed record IntakeDetailResponse(
    Guid Id,
    string ProspectiveClientName,
    string? ProspectiveClientEmail,
    string? ProspectiveClientPhone,
    string PracticeArea,
    string IntakeType,
    string Stage,
    string OwnerActorId,
    string OwnerDisplayName,
    string? NextAction,
    DateTimeOffset? NextActionDueAtUtc,
    string? ReferralSource,
    Guid? ConflictCheckId,
    Guid? ApprovedConflictDecisionId,
    string? DeclineReason,
    string? NotesSummary,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyCollection<IntakeStageHistoryResponse> StageHistory,
    IReadOnlyCollection<IntakeNoteResponse> Notes);

public sealed record CreateIntakeRequest(
    string ProspectiveClientName,
    string? ProspectiveClientEmail,
    string? ProspectiveClientPhone,
    string PracticeArea,
    string IntakeType,
    string OwnerActorId,
    string OwnerDisplayName,
    string? NextAction,
    DateTimeOffset? NextActionDueAtUtc,
    string? ReferralSource,
    Guid? ConflictCheckId,
    string? NotesSummary);

public sealed record ChangeIntakeStageRequest(
    string ToStage,
    string? ChangeReason,
    string? NextAction,
    DateTimeOffset? NextActionDueAtUtc,
    string? DeclineReason,
    Guid? ApprovedConflictDecisionId);

public sealed record AddIntakeNoteRequest(string Note);

public sealed record ConvertIntakeToMatterRequest(
    string MatterTitle,
    string ResponsibleAttorneyActorId,
    string? ResponsibleAttorneyDisplayName,
    string? ParalegalActorId,
    string? ParalegalDisplayName);

public sealed record ConvertIntakeToMatterResponse(Guid MatterId, string MatterNumber);
