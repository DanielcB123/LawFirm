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
    public DbSet<MatterAccessGrant> MatterAccessGrants => Set<MatterAccessGrant>();
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

        modelBuilder.Entity<MatterAccessGrant>(entity =>
        {
            entity.ToTable("matter_access_grants", table =>
                table.HasCheckConstraint("CK_matter_access_grants_GrantType", "GrantType IN ('Client', 'Staff')"));
            entity.HasKey(grant => grant.Id);
            entity.Property(grant => grant.ActorId).HasMaxLength(128).IsRequired();
            entity.Property(grant => grant.GrantType).HasMaxLength(16).IsRequired();
            entity.Property(grant => grant.GrantedByActorId).HasMaxLength(128).IsRequired();
            entity.Property(grant => grant.GrantedAtUtc).IsRequired();
            entity.Property(grant => grant.RevokedByActorId).HasMaxLength(128);
            entity.Property(grant => grant.RevokedAtUtc);
            // MySQL has no partial unique index; NULLs never collide, so only one unrevoked grant per matter/actor.
            entity.Property(grant => grant.ActiveMarker)
                .HasComputedColumnSql("(CASE WHEN `RevokedAtUtc` IS NULL THEN 1 ELSE NULL END)", stored: true);
            entity.HasOne(grant => grant.MatterRecord)
                .WithMany()
                .HasForeignKey(grant => grant.MatterRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(grant => new { grant.MatterRecordId, grant.ActorId, grant.ActiveMarker }).IsUnique();
            entity.HasIndex(grant => new { grant.ActorId, grant.RevokedAtUtc });
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
