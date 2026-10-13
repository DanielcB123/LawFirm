namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public static class IntakeStages
{
    public const string New = "New";
    public const string Contacted = "Contacted";
    public const string ConsultationScheduled = "ConsultationScheduled";
    public const string UnderReview = "UnderReview";
    public const string Accepted = "Accepted";
    public const string Declined = "Declined";
    public const string ReferredElsewhere = "ReferredElsewhere";
}

public sealed class IntakeRecord
{
    public Guid Id { get; set; }
    public string PracticeArea { get; set; } = string.Empty;
    public string IntakeType { get; set; } = "StaffEntered";
    public string Stage { get; set; } = IntakeStages.New;
    public string ProspectiveClientName { get; set; } = string.Empty;
    public string? ProspectiveClientEmail { get; set; }
    public string? ProspectiveClientPhone { get; set; }
    public string OwnerActorId { get; set; } = string.Empty;
    public string OwnerDisplayName { get; set; } = string.Empty;
    public string? NextAction { get; set; }
    public DateTimeOffset? NextActionDueAtUtc { get; set; }
    public string? ReferralSource { get; set; }
    public Guid? ConflictCheckId { get; set; }
    public Guid? ApprovedConflictDecisionId { get; set; }
    public string? DeclineReason { get; set; }
    public string? NotesSummary { get; set; }
    public string CreatedByActorId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public List<IntakeStageHistory> StageHistory { get; set; } = [];
    public List<IntakeNote> Notes { get; set; } = [];
}

public sealed class IntakeStageHistory
{
    public Guid Id { get; set; }
    public Guid IntakeRecordId { get; set; }
    public IntakeRecord IntakeRecord { get; set; } = null!;
    public string FromStage { get; set; } = string.Empty;
    public string ToStage { get; set; } = string.Empty;
    public string ChangedByActorId { get; set; } = string.Empty;
    public string ChangedByDisplayName { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
}

public sealed class IntakeNote
{
    public Guid Id { get; set; }
    public Guid IntakeRecordId { get; set; }
    public IntakeRecord IntakeRecord { get; set; } = null!;
    public string Note { get; set; } = string.Empty;
    public string CreatedByActorId { get; set; } = string.Empty;
    public string CreatedByDisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
