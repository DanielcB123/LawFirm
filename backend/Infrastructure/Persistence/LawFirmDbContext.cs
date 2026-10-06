using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public sealed class LawFirmDbContext(DbContextOptions<LawFirmDbContext> options) : DbContext(options)
{
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<PartyAlias> PartyAliases => Set<PartyAlias>();
    public DbSet<PartyRelationship> PartyRelationships => Set<PartyRelationship>();
    public DbSet<ConflictCheck> ConflictChecks => Set<ConflictCheck>();
    public DbSet<ConflictDecision> ConflictDecisions => Set<ConflictDecision>();
    public DbSet<CalendarEntry> CalendarEntries => Set<CalendarEntry>();
    public DbSet<IntakeRecord> IntakeRecords => Set<IntakeRecord>();
    public DbSet<IntakeStageHistory> IntakeStageHistories => Set<IntakeStageHistory>();
    public DbSet<IntakeNote> IntakeNotes => Set<IntakeNote>();
    public DbSet<MatterRecord> MatterRecords => Set<MatterRecord>();
    public DbSet<MatterStageHistory> MatterStageHistories => Set<MatterStageHistory>();
    public DbSet<MatterTask> MatterTasks => Set<MatterTask>();
    public DbSet<MatterDocument> MatterDocuments => Set<MatterDocument>();
    public DbSet<MatterTimelineAction> MatterTimelineActions => Set<MatterTimelineAction>();
    public DbSet<ConsultationRequest> ConsultationRequests => Set<ConsultationRequest>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Party>(entity =>
        {
            entity.ToTable("parties");
            entity.HasKey(party => party.Id);
            entity.Property(party => party.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(party => party.PartyType).HasMaxLength(32).IsRequired();
            entity.Property(party => party.PrimaryEmail).HasMaxLength(256);
            entity.Property(party => party.PrimaryPhone).HasMaxLength(64);
            entity.Property(party => party.CreatedAtUtc).IsRequired();
            entity.HasIndex(party => party.DisplayName);
        });

        modelBuilder.Entity<PartyAlias>(entity =>
        {
            entity.ToTable("party_aliases");
            entity.HasKey(alias => alias.Id);
            entity.Property(alias => alias.Alias).HasMaxLength(200).IsRequired();
            entity.Property(alias => alias.AliasType).HasMaxLength(64).IsRequired();
            entity.Property(alias => alias.CreatedAtUtc).IsRequired();
            entity.HasOne(alias => alias.Party)
                .WithMany(party => party.Aliases)
                .HasForeignKey(alias => alias.PartyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(alias => new { alias.PartyId, alias.Alias }).IsUnique();
        });

        modelBuilder.Entity<PartyRelationship>(entity =>
        {
            entity.ToTable("party_relationships");
            entity.HasKey(relationship => relationship.Id);
            entity.Property(relationship => relationship.RelationshipType).HasMaxLength(64).IsRequired();
            entity.Property(relationship => relationship.CreatedAtUtc).IsRequired();
            entity.HasOne(relationship => relationship.FromParty)
                .WithMany()
                .HasForeignKey(relationship => relationship.FromPartyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(relationship => relationship.ToParty)
                .WithMany()
                .HasForeignKey(relationship => relationship.ToPartyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(relationship => new
            {
                relationship.FromPartyId,
                relationship.ToPartyId,
                relationship.RelationshipType
            }).IsUnique();
        });

        modelBuilder.Entity<ConflictCheck>(entity =>
        {
            entity.ToTable("conflict_checks");
            entity.HasKey(check => check.Id);
            entity.Property(check => check.Query).HasMaxLength(200).IsRequired();
            entity.Property(check => check.RequestedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(check => check.RequestedByActorDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(check => check.MatchSnapshotJson).IsRequired();
            entity.Property(check => check.RequestedAtUtc).IsRequired();
        });

        modelBuilder.Entity<ConflictDecision>(entity =>
        {
            entity.ToTable("conflict_decisions");
            entity.HasKey(decision => decision.Id);
            entity.Property(decision => decision.Decision).HasMaxLength(64).IsRequired();
            entity.Property(decision => decision.Rationale).HasMaxLength(2000).IsRequired();
            entity.Property(decision => decision.DecidedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(decision => decision.DecidedByActorDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(decision => decision.MatchSnapshotJson).IsRequired();
            entity.Property(decision => decision.DecidedAtUtc).IsRequired();
            entity.HasOne(decision => decision.ConflictCheck)
                .WithMany(check => check.Decisions)
                .HasForeignKey(decision => decision.ConflictCheckId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("audit_events");
            entity.HasKey(auditEvent => auditEvent.Id);
            entity.Property(auditEvent => auditEvent.Action).HasMaxLength(128).IsRequired();
            entity.Property(auditEvent => auditEvent.ObjectType).HasMaxLength(128).IsRequired();
            entity.Property(auditEvent => auditEvent.ObjectId).HasMaxLength(128).IsRequired();
            entity.Property(auditEvent => auditEvent.ActorId).HasMaxLength(128).IsRequired();
            entity.Property(auditEvent => auditEvent.ActorDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(auditEvent => auditEvent.CorrelationId).HasMaxLength(128).IsRequired();
            entity.Property(auditEvent => auditEvent.MetadataJson).IsRequired();
            entity.Property(auditEvent => auditEvent.OccurredAtUtc).IsRequired();
            entity.HasIndex(auditEvent => auditEvent.OccurredAtUtc);
        });

        modelBuilder.Entity<CalendarEntry>(entity =>
        {
            entity.ToTable("calendar_entries");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Title).HasMaxLength(200).IsRequired();
            entity.Property(entry => entry.EntryType).HasMaxLength(64).IsRequired();
            entity.Property(entry => entry.DeadlineType).HasMaxLength(64).IsRequired();
            entity.Property(entry => entry.OwnerActorId).HasMaxLength(128).IsRequired();
            entity.Property(entry => entry.OwnerDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(entry => entry.BackupActorId).HasMaxLength(128);
            entity.Property(entry => entry.BackupDisplayName).HasMaxLength(200);
            entity.Property(entry => entry.TimeZone).HasMaxLength(64).IsRequired();
            entity.Property(entry => entry.MatterReference).HasMaxLength(128);
            entity.Property(entry => entry.SourceReference).HasMaxLength(200);
            entity.Property(entry => entry.OverrideReason).HasMaxLength(2000);
            entity.Property(entry => entry.RemindersJson).IsRequired();
            entity.Property(entry => entry.CreatedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(entry => entry.CreatedAtUtc).IsRequired();
            entity.HasIndex(entry => entry.ScheduledAtUtc);
            entity.HasIndex(entry => entry.OwnerActorId);
        });

        modelBuilder.Entity<IntakeRecord>(entity =>
        {
            entity.ToTable("intake_records");
            entity.HasKey(record => record.Id);
            entity.Property(record => record.PracticeArea).HasMaxLength(64).IsRequired();
            entity.Property(record => record.IntakeType).HasMaxLength(64).IsRequired();
            entity.Property(record => record.Stage).HasMaxLength(64).IsRequired();
            entity.Property(record => record.ProspectiveClientName).HasMaxLength(200).IsRequired();
            entity.Property(record => record.ProspectiveClientEmail).HasMaxLength(256);
            entity.Property(record => record.ProspectiveClientPhone).HasMaxLength(64);
            entity.Property(record => record.OwnerActorId).HasMaxLength(128).IsRequired();
            entity.Property(record => record.OwnerDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(record => record.NextAction).HasMaxLength(200);
            entity.Property(record => record.NextActionDueAtUtc);
            entity.Property(record => record.ReferralSource).HasMaxLength(128);
            entity.Property(record => record.ConflictCheckId);
            entity.Property(record => record.ApprovedConflictDecisionId);
            entity.Property(record => record.DeclineReason).HasMaxLength(500);
            entity.Property(record => record.NotesSummary).HasMaxLength(2000);
            entity.Property(record => record.CreatedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(record => record.CreatedAtUtc).IsRequired();
            entity.HasIndex(record => record.Stage);
            entity.HasIndex(record => record.OwnerActorId);
            entity.HasIndex(record => record.PracticeArea);
        });

        modelBuilder.Entity<IntakeStageHistory>(entity =>
        {
            entity.ToTable("intake_stage_history");
            entity.HasKey(history => history.Id);
            entity.Property(history => history.FromStage).HasMaxLength(64).IsRequired();
            entity.Property(history => history.ToStage).HasMaxLength(64).IsRequired();
            entity.Property(history => history.ChangedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(history => history.ChangedByDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(history => history.ChangeReason).HasMaxLength(1000);
            entity.Property(history => history.ChangedAtUtc).IsRequired();
            entity.HasOne(history => history.IntakeRecord)
                .WithMany(record => record.StageHistory)
                .HasForeignKey(history => history.IntakeRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(history => history.IntakeRecordId);
        });

        modelBuilder.Entity<IntakeNote>(entity =>
        {
            entity.ToTable("intake_notes");
            entity.HasKey(note => note.Id);
            entity.Property(note => note.Note).HasMaxLength(3000).IsRequired();
            entity.Property(note => note.CreatedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(note => note.CreatedByDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(note => note.CreatedAtUtc).IsRequired();
            entity.HasOne(note => note.IntakeRecord)
                .WithMany(record => record.Notes)
                .HasForeignKey(note => note.IntakeRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(note => note.IntakeRecordId);
        });

        modelBuilder.Entity<MatterRecord>(entity =>
        {
            entity.ToTable("matters");
            entity.HasKey(record => record.Id);
            entity.Property(record => record.MatterNumber).HasMaxLength(64).IsRequired();
            entity.Property(record => record.Title).HasMaxLength(200).IsRequired();
            entity.Property(record => record.PracticeArea).HasMaxLength(64).IsRequired();
            entity.Property(record => record.Stage).HasMaxLength(64).IsRequired();
            entity.Property(record => record.Status).HasMaxLength(64).IsRequired();
            entity.Property(record => record.ResponsibleAttorneyActorId).HasMaxLength(128).IsRequired();
            entity.Property(record => record.ResponsibleAttorneyDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(record => record.ParalegalActorId).HasMaxLength(128);
            entity.Property(record => record.ParalegalDisplayName).HasMaxLength(200);
            entity.Property(record => record.LinkedIntakeRecordId);
            entity.Property(record => record.CreatedAtUtc).IsRequired();
            entity.Property(record => record.CreatedByActorId).HasMaxLength(128).IsRequired();
            entity.HasIndex(record => record.MatterNumber).IsUnique();
            entity.HasIndex(record => record.Stage);
            entity.HasIndex(record => record.ResponsibleAttorneyActorId);
        });

        modelBuilder.Entity<MatterStageHistory>(entity =>
        {
            entity.ToTable("matter_stage_history");
            entity.HasKey(history => history.Id);
            entity.Property(history => history.FromStage).HasMaxLength(64).IsRequired();
            entity.Property(history => history.ToStage).HasMaxLength(64).IsRequired();
            entity.Property(history => history.ChangedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(history => history.ChangedByDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(history => history.ChangeReason).HasMaxLength(1000);
            entity.Property(history => history.ChangedAtUtc).IsRequired();
            entity.HasOne(history => history.MatterRecord)
                .WithMany(record => record.StageHistory)
                .HasForeignKey(history => history.MatterRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(history => history.MatterRecordId);
        });

        modelBuilder.Entity<MatterTask>(entity =>
        {
            entity.ToTable("matter_tasks");
            entity.HasKey(task => task.Id);
            entity.Property(task => task.MatterRecordId).IsRequired();
            entity.Property(task => task.Title).HasMaxLength(240).IsRequired();
            entity.Property(task => task.Description).HasMaxLength(4000);
            entity.Property(task => task.Status).HasMaxLength(64).IsRequired();
            entity.Property(task => task.Priority).HasMaxLength(32).IsRequired();
            entity.Property(task => task.AssigneeActorId).HasMaxLength(128);
            entity.Property(task => task.AssigneeDisplayName).HasMaxLength(200);
            entity.Property(task => task.DueAtUtc);
            entity.Property(task => task.IsDeadlineVerified).IsRequired();
            entity.Property(task => task.DeadlineVerifiedAtUtc);
            entity.Property(task => task.DeadlineVerifiedByActorId).HasMaxLength(128);
            entity.Property(task => task.DeadlineVerifiedByDisplayName).HasMaxLength(200);
            entity.Property(task => task.CompletedAtUtc);
            entity.Property(task => task.CreatedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(task => task.CreatedByDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(task => task.CreatedAtUtc).IsRequired();
            entity.HasOne(task => task.MatterRecord)
                .WithMany(record => record.Tasks)
                .HasForeignKey(task => task.MatterRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(task => task.MatterRecordId);
            entity.HasIndex(task => task.DueAtUtc);
        });

        modelBuilder.Entity<MatterDocument>(entity =>
        {
            entity.ToTable("matter_documents");
            entity.HasKey(document => document.Id);
            entity.Property(document => document.MatterRecordId).IsRequired();
            entity.Property(document => document.FileName).HasMaxLength(240).IsRequired();
            entity.Property(document => document.DocumentType).HasMaxLength(80).IsRequired();
            entity.Property(document => document.StorageKey).HasMaxLength(512);
            entity.Property(document => document.UploadedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(document => document.UploadedByDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(document => document.UploadedAtUtc).IsRequired();
            entity.Property(document => document.ReviewStatus).HasMaxLength(32).IsRequired();
            entity.Property(document => document.ReviewNotes).HasMaxLength(2000);
            entity.Property(document => document.ReviewedByActorId).HasMaxLength(128);
            entity.Property(document => document.ReviewedByDisplayName).HasMaxLength(200);
            entity.Property(document => document.ReviewedAtUtc);
            entity.HasOne(document => document.MatterRecord)
                .WithMany(record => record.Documents)
                .HasForeignKey(document => document.MatterRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(document => document.MatterRecordId);
            entity.HasIndex(document => document.DocumentType);
        });

        modelBuilder.Entity<MatterTimelineAction>(entity =>
        {
            entity.ToTable("matter_timeline_actions");
            entity.HasKey(action => action.Id);
            entity.Property(action => action.MatterRecordId).IsRequired();
            entity.Property(action => action.ActionType).HasMaxLength(80).IsRequired();
            entity.Property(action => action.Summary).HasMaxLength(400).IsRequired();
            entity.Property(action => action.Details).HasMaxLength(4000);
            entity.Property(action => action.OccurredAtUtc).IsRequired();
            entity.Property(action => action.CreatedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(action => action.CreatedByDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(action => action.CreatedAtUtc).IsRequired();
            entity.HasOne(action => action.MatterRecord)
                .WithMany(record => record.TimelineActions)
                .HasForeignKey(action => action.MatterRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(action => action.MatterRecordId);
            entity.HasIndex(action => action.OccurredAtUtc);
        });

        modelBuilder.Entity<ConsultationRequest>(entity =>
        {
            entity.ToTable("consultation_requests");
            entity.HasKey(request => request.Id);
            entity.Property(request => request.FullName).HasMaxLength(200).IsRequired();
            entity.Property(request => request.Email).HasMaxLength(256).IsRequired();
            entity.Property(request => request.Phone).HasMaxLength(64);
            entity.Property(request => request.PracticeArea).HasMaxLength(64).IsRequired();
            entity.Property(request => request.Message).HasMaxLength(3000);
            entity.Property(request => request.TimeZone).HasMaxLength(64).IsRequired();
            entity.Property(request => request.Status).HasMaxLength(32).IsRequired();
            entity.Property(request => request.AssignedToActorId).HasMaxLength(128);
            entity.Property(request => request.AssignedToDisplayName).HasMaxLength(200);
            entity.Property(request => request.InternalNotes).HasMaxLength(2000);
            entity.Property(request => request.CreatedAtUtc).IsRequired();
            entity.Property(request => request.UpdatedAtUtc).IsRequired();
            entity.HasIndex(request => request.Status);
            entity.HasIndex(request => request.PreferredAtUtc);
            entity.HasIndex(request => request.CreatedAtUtc);
        });
    }
}

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

public sealed class AuditEvent
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ObjectType { get; set; } = string.Empty;
    public string ObjectId { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string ActorDisplayName { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string MetadataJson { get; set; } = "{}";
    public DateTimeOffset OccurredAtUtc { get; set; }
}

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

public static class ConsultationStatuses
{
    public const string New = "New";
    public const string Contacted = "Contacted";
    public const string Scheduled = "Scheduled";
    public const string InReview = "InReview";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";
}

public sealed class ConsultationRequest
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PracticeArea { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTimeOffset PreferredAtUtc { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Status { get; set; } = ConsultationStatuses.New;
    public string? AssignedToActorId { get; set; }
    public string? AssignedToDisplayName { get; set; }
    public string? InternalNotes { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
