using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public static class DevelopmentDataSeeder
{
    public static async Task SeedDevelopmentDataAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LawFirmDbContext>();

        if (!await dbContext.Parties.AnyAsync())
        {
            var partyClient = new Party
            {
                Id = Guid.NewGuid(),
                PartyType = "Person",
                DisplayName = "Alicia Warren",
                PrimaryEmail = "alicia.warren@example.test",
                PrimaryPhone = "(555) 010-2241",
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            var partyDefendant = new Party
            {
                Id = Guid.NewGuid(),
                PartyType = "Organization",
                DisplayName = "Northgate Builders LLC",
                PrimaryEmail = "legal@northgate.example.test",
                PrimaryPhone = "(555) 010-5520",
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            dbContext.Parties.AddRange(partyClient, partyDefendant);
            dbContext.PartyAliases.Add(new PartyAlias
            {
                Id = Guid.NewGuid(),
                PartyId = partyDefendant.Id,
                Alias = "Northgate Builders",
                AliasType = "TradeName",
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }

        if (!await dbContext.ConflictChecks.AnyAsync())
        {
            var check = new ConflictCheck
            {
                Id = Guid.NewGuid(),
                Query = "Northgate Builders LLC",
                RequestedByActorId = "u-paralegal-001",
                RequestedByActorDisplayName = "Priya Paralegal",
                MatchSnapshotJson = "[]",
                RequestedAtUtc = DateTimeOffset.UtcNow.AddDays(-2)
            };
            dbContext.ConflictChecks.Add(check);
            dbContext.ConflictDecisions.Add(new ConflictDecision
            {
                Id = Guid.NewGuid(),
                ConflictCheckId = check.Id,
                Decision = "Approved",
                Rationale = "No disqualifying conflict found for adverse party role.",
                DecidedByActorId = "u-attorney-001",
                DecidedByActorDisplayName = "Avery Attorney",
                MatchSnapshotJson = "[]",
                DecidedAtUtc = DateTimeOffset.UtcNow.AddDays(-2).AddHours(2)
            });
        }

        if (!await dbContext.CalendarEntries.AnyAsync())
        {
            dbContext.CalendarEntries.AddRange(
                new CalendarEntry
                {
                    Id = Guid.NewGuid(),
                    Title = "Review intake package",
                    EntryType = "Task",
                    DeadlineType = "InternalTarget",
                    OwnerActorId = "u-paralegal-001",
                    OwnerDisplayName = "Priya Paralegal",
                    BackupActorId = "u-attorney-001",
                    BackupDisplayName = "Avery Attorney",
                    ScheduledAtUtc = DateTimeOffset.UtcNow.AddDays(1).Date.AddHours(14),
                    IsAllDay = false,
                    TimeZone = "America/New_York",
                    MatterReference = "MAT-2026-0001",
                    SourceReference = "Initial onboarding checklist",
                    RemindersJson = "[60,1440]",
                    CreatedByActorId = "u-paralegal-001",
                    CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
                },
                new CalendarEntry
                {
                    Id = Guid.NewGuid(),
                    Title = "Settlement strategy call",
                    EntryType = "Event",
                    DeadlineType = "LegallySignificant",
                    OwnerActorId = "u-attorney-001",
                    OwnerDisplayName = "Avery Attorney",
                    BackupActorId = "u-attorney-002",
                    BackupDisplayName = "Jordan Attorney",
                    ScheduledAtUtc = DateTimeOffset.UtcNow.AddDays(2).Date.AddHours(16),
                    IsAllDay = false,
                    TimeZone = "America/New_York",
                    MatterReference = "MAT-2026-0002",
                    SourceReference = "Client update requirement",
                    RemindersJson = "[30,120]",
                    CreatedByActorId = "u-attorney-001",
                    CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
                });
        }

        if (!await dbContext.IntakeRecords.AnyAsync())
        {
            var approvedDecisionId = await dbContext.ConflictDecisions
                .AsNoTracking()
                .OrderByDescending(item => item.DecidedAtUtc)
                .Select(item => item.Id)
                .FirstOrDefaultAsync();
            var approvedCheckId = await dbContext.ConflictChecks
                .AsNoTracking()
                .OrderByDescending(item => item.RequestedAtUtc)
                .Select(item => item.Id)
                .FirstOrDefaultAsync();

            var intakeOne = new IntakeRecord
            {
                Id = Guid.NewGuid(),
                PracticeArea = "PersonalInjury",
                IntakeType = "Website",
                Stage = IntakeStages.UnderReview,
                ProspectiveClientName = "Alicia Warren",
                ProspectiveClientEmail = "alicia.warren@example.test",
                ProspectiveClientPhone = "(555) 010-2241",
                OwnerActorId = "u-paralegal-001",
                OwnerDisplayName = "Priya Paralegal",
                NextAction = "Attorney call to review treatment timeline",
                NextActionDueAtUtc = DateTimeOffset.UtcNow.AddDays(1),
                ReferralSource = "Google Ads",
                ConflictCheckId = approvedCheckId,
                NotesSummary = "Rear-end collision, records partially received.",
                CreatedByActorId = "u-paralegal-001",
                CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-3)
            };

            var intakeTwo = new IntakeRecord
            {
                Id = Guid.NewGuid(),
                PracticeArea = "ConstructionLitigation",
                IntakeType = "Phone",
                Stage = IntakeStages.Accepted,
                ProspectiveClientName = "Magnolia Stoneworks Inc.",
                ProspectiveClientEmail = "ops@magnolia-stoneworks.example.test",
                ProspectiveClientPhone = "(555) 010-9981",
                OwnerActorId = "u-attorney-002",
                OwnerDisplayName = "Jordan Attorney",
                NextAction = "Open matter shell and issue preservation tasks",
                NextActionDueAtUtc = DateTimeOffset.UtcNow.AddHours(8),
                ReferralSource = "Architect referral",
                ConflictCheckId = approvedCheckId,
                ApprovedConflictDecisionId = approvedDecisionId == Guid.Empty ? null : approvedDecisionId,
                NotesSummary = "Disputed retainage and defect allegations.",
                CreatedByActorId = "u-attorney-002",
                CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-5)
            };

            dbContext.IntakeRecords.AddRange(intakeOne, intakeTwo);
            dbContext.IntakeStageHistories.AddRange(
                new IntakeStageHistory
                {
                    Id = Guid.NewGuid(),
                    IntakeRecordId = intakeOne.Id,
                    FromStage = IntakeStages.New,
                    ToStage = IntakeStages.UnderReview,
                    ChangedByActorId = "u-paralegal-001",
                    ChangedByDisplayName = "Priya Paralegal",
                    ChangeReason = "Collected initial documents and assigned attorney review.",
                    ChangedAtUtc = DateTimeOffset.UtcNow.AddDays(-2)
                },
                new IntakeStageHistory
                {
                    Id = Guid.NewGuid(),
                    IntakeRecordId = intakeTwo.Id,
                    FromStage = IntakeStages.UnderReview,
                    ToStage = IntakeStages.Accepted,
                    ChangedByActorId = "u-attorney-002",
                    ChangedByDisplayName = "Jordan Attorney",
                    ChangeReason = "Approved after conflict review and scope confirmation.",
                    ChangedAtUtc = DateTimeOffset.UtcNow.AddDays(-4)
                });

            dbContext.IntakeNotes.AddRange(
                new IntakeNote
                {
                    Id = Guid.NewGuid(),
                    IntakeRecordId = intakeOne.Id,
                    Note = "Need orthopedic follow-up records before demand planning.",
                    CreatedByActorId = "u-attorney-001",
                    CreatedByDisplayName = "Avery Attorney",
                    CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
                },
                new IntakeNote
                {
                    Id = Guid.NewGuid(),
                    IntakeRecordId = intakeTwo.Id,
                    Note = "Client provided contract exhibit set and payment ledger exports.",
                    CreatedByActorId = "u-paralegal-002",
                    CreatedByDisplayName = "Dylan Paralegal",
                    CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-3)
                });
        }

        if (!await dbContext.MatterRecords.AnyAsync())
        {
            var acceptedIntakeId = await dbContext.IntakeRecords
                .AsNoTracking()
                .Where(record => record.Stage == IntakeStages.Accepted)
                .Select(record => record.Id)
                .FirstOrDefaultAsync();
            var linkedIntake = acceptedIntakeId == Guid.Empty ? (Guid?)null : acceptedIntakeId;

            var matter = new MatterRecord
            {
                Id = Guid.NewGuid(),
                MatterNumber = "MAT-2026-0001",
                Title = "Magnolia Stoneworks v. Northgate Builders",
                PracticeArea = "ConstructionLitigation",
                Stage = "Open",
                Status = "Active",
                ResponsibleAttorneyActorId = "u-attorney-002",
                ResponsibleAttorneyDisplayName = "Jordan Attorney",
                ParalegalActorId = "u-paralegal-001",
                ParalegalDisplayName = "Priya Paralegal",
                LinkedIntakeRecordId = linkedIntake,
                CreatedByActorId = "u-attorney-002",
                CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-2)
            };

            dbContext.MatterRecords.Add(matter);
            dbContext.MatterStageHistories.Add(new MatterStageHistory
            {
                Id = Guid.NewGuid(),
                MatterRecordId = matter.Id,
                FromStage = "Created",
                ToStage = "Open",
                ChangedByActorId = "u-attorney-002",
                ChangedByDisplayName = "Jordan Attorney",
                ChangeReason = "Matter opened from accepted intake.",
                ChangedAtUtc = DateTimeOffset.UtcNow.AddDays(-2)
            });
        }

        if (!await dbContext.MatterTasks.AnyAsync())
        {
            var matterId = await dbContext.MatterRecords
                .AsNoTracking()
                .OrderBy(record => record.CreatedAtUtc)
                .Select(record => record.Id)
                .FirstOrDefaultAsync();

            if (matterId != Guid.Empty)
            {
                dbContext.MatterTasks.AddRange(
                    new MatterTask
                    {
                        Id = Guid.NewGuid(),
                        MatterRecordId = matterId,
                        Title = "Draft litigation hold notice",
                        Description = "Prepare and send preservation notice to opposing party within 48 hours.",
                        Status = "Open",
                        Priority = "High",
                        AssigneeActorId = "u-paralegal-001",
                        AssigneeDisplayName = "Priya Paralegal",
                        DueAtUtc = DateTimeOffset.UtcNow.AddDays(2),
                        IsDeadlineVerified = true,
                        DeadlineVerifiedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
                        DeadlineVerifiedByActorId = "u-attorney-002",
                        DeadlineVerifiedByDisplayName = "Jordan Attorney",
                        CreatedByActorId = "u-attorney-002",
                        CreatedByDisplayName = "Jordan Attorney",
                        CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
                    },
                    new MatterTask
                    {
                        Id = Guid.NewGuid(),
                        MatterRecordId = matterId,
                        Title = "Collect payment ledger exhibits",
                        Description = "Confirm and index ledger export attachments from client intake packet.",
                        Status = "InProgress",
                        Priority = "Normal",
                        AssigneeActorId = "u-paralegal-002",
                        AssigneeDisplayName = "Dylan Paralegal",
                        DueAtUtc = DateTimeOffset.UtcNow.AddDays(4),
                        IsDeadlineVerified = false,
                        CreatedByActorId = "u-attorney-002",
                        CreatedByDisplayName = "Jordan Attorney",
                        CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
                    });
            }
        }

        if (!await dbContext.MatterDocuments.AnyAsync())
        {
            var matterId = await dbContext.MatterRecords
                .AsNoTracking()
                .OrderBy(record => record.CreatedAtUtc)
                .Select(record => record.Id)
                .FirstOrDefaultAsync();

            if (matterId != Guid.Empty)
            {
                dbContext.MatterDocuments.AddRange(
                    new MatterDocument
                    {
                        Id = Guid.NewGuid(),
                        MatterRecordId = matterId,
                        FileName = "Retainer-Agreement-Signed.pdf",
                        DocumentType = "Retainer",
                        StorageKey = "matters/demo/retainer-agreement-signed.pdf",
                        UploadedByActorId = "u-paralegal-001",
                        UploadedByDisplayName = "Priya Paralegal",
                        UploadedAtUtc = DateTimeOffset.UtcNow.AddDays(-2),
                        ReviewStatus = "Approved",
                        ReviewNotes = "Executed copy verified against intake details.",
                        ReviewedByActorId = "u-attorney-002",
                        ReviewedByDisplayName = "Jordan Attorney",
                        ReviewedAtUtc = DateTimeOffset.UtcNow.AddDays(-2).AddHours(3)
                    },
                    new MatterDocument
                    {
                        Id = Guid.NewGuid(),
                        MatterRecordId = matterId,
                        FileName = "Damage-Photo-Set.zip",
                        DocumentType = "Evidence",
                        StorageKey = "matters/demo/damage-photo-set.zip",
                        UploadedByActorId = "u-client-001",
                        UploadedByDisplayName = "Casey Client",
                        UploadedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
                        ReviewStatus = "Pending"
                    });
            }
        }

        if (!await dbContext.MatterTimelineActions.AnyAsync())
        {
            var matterId = await dbContext.MatterRecords
                .AsNoTracking()
                .OrderBy(record => record.CreatedAtUtc)
                .Select(record => record.Id)
                .FirstOrDefaultAsync();

            if (matterId != Guid.Empty)
            {
                dbContext.MatterTimelineActions.AddRange(
                    new MatterTimelineAction
                    {
                        Id = Guid.NewGuid(),
                        MatterRecordId = matterId,
                        ActionType = "ClientCall",
                        Summary = "Initial litigation strategy call completed",
                        Details = "Discussed scope, target claims, and immediate preservation steps.",
                        OccurredAtUtc = DateTimeOffset.UtcNow.AddDays(-1).AddHours(-2),
                        CreatedByActorId = "u-attorney-002",
                        CreatedByDisplayName = "Jordan Attorney",
                        CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1).AddHours(-2)
                    },
                    new MatterTimelineAction
                    {
                        Id = Guid.NewGuid(),
                        MatterRecordId = matterId,
                        ActionType = "Filing",
                        Summary = "Draft complaint outline prepared",
                        Details = "Paralegal prepared claim chronology and exhibit references for attorney review.",
                        OccurredAtUtc = DateTimeOffset.UtcNow.AddHours(-10),
                        CreatedByActorId = "u-paralegal-001",
                        CreatedByDisplayName = "Priya Paralegal",
                        CreatedAtUtc = DateTimeOffset.UtcNow.AddHours(-10)
                    });
            }
        }

        await dbContext.SaveChangesAsync();
    }
}
