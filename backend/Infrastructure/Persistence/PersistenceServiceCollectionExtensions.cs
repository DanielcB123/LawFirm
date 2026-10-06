using EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddLawFirmPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LawFirm")
                               ?? "Server=localhost;Port=3307;Database=lawfirm_dev;User=lawfirm_app;Password=lawfirm_dev_pw;AllowPublicKeyRetrieval=True;SslMode=Preferred;";

        services.AddDbContext<LawFirmDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));
        services.AddScoped<IAuditTrailService, AuditTrailService>();

        return services;
    }

    public static async Task InitializeLawFirmPersistenceAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LawFirmDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS intake_records (
                Id char(36) NOT NULL,
                PracticeArea varchar(64) NOT NULL,
                IntakeType varchar(64) NOT NULL,
                Stage varchar(64) NOT NULL,
                ProspectiveClientName varchar(200) NOT NULL,
                ProspectiveClientEmail varchar(256) NULL,
                ProspectiveClientPhone varchar(64) NULL,
                OwnerActorId varchar(128) NOT NULL,
                OwnerDisplayName varchar(200) NOT NULL,
                NextAction varchar(200) NULL,
                NextActionDueAtUtc datetime(6) NULL,
                ReferralSource varchar(128) NULL,
                ConflictCheckId char(36) NULL,
                ApprovedConflictDecisionId char(36) NULL,
                DeclineReason varchar(500) NULL,
                NotesSummary varchar(2000) NULL,
                CreatedByActorId varchar(128) NOT NULL,
                CreatedAtUtc datetime(6) NOT NULL,
                PRIMARY KEY (Id),
                INDEX IX_intake_records_Stage (Stage),
                INDEX IX_intake_records_OwnerActorId (OwnerActorId),
                INDEX IX_intake_records_PracticeArea (PracticeArea)
            );
            """);
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS intake_stage_history (
                Id char(36) NOT NULL,
                IntakeRecordId char(36) NOT NULL,
                FromStage varchar(64) NOT NULL,
                ToStage varchar(64) NOT NULL,
                ChangedByActorId varchar(128) NOT NULL,
                ChangedByDisplayName varchar(200) NOT NULL,
                ChangeReason varchar(1000) NULL,
                ChangedAtUtc datetime(6) NOT NULL,
                PRIMARY KEY (Id),
                INDEX IX_intake_stage_history_IntakeRecordId (IntakeRecordId),
                CONSTRAINT FK_intake_stage_history_intake_records_IntakeRecordId
                    FOREIGN KEY (IntakeRecordId) REFERENCES intake_records (Id) ON DELETE CASCADE
            );
            """);
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS intake_notes (
                Id char(36) NOT NULL,
                IntakeRecordId char(36) NOT NULL,
                Note varchar(3000) NOT NULL,
                CreatedByActorId varchar(128) NOT NULL,
                CreatedByDisplayName varchar(200) NOT NULL,
                CreatedAtUtc datetime(6) NOT NULL,
                PRIMARY KEY (Id),
                INDEX IX_intake_notes_IntakeRecordId (IntakeRecordId),
                CONSTRAINT FK_intake_notes_intake_records_IntakeRecordId
                    FOREIGN KEY (IntakeRecordId) REFERENCES intake_records (Id) ON DELETE CASCADE
            );
            """);
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS matters (
                Id char(36) NOT NULL,
                MatterNumber varchar(64) NOT NULL,
                Title varchar(200) NOT NULL,
                PracticeArea varchar(64) NOT NULL,
                Stage varchar(64) NOT NULL,
                Status varchar(64) NOT NULL,
                ResponsibleAttorneyActorId varchar(128) NOT NULL,
                ResponsibleAttorneyDisplayName varchar(200) NOT NULL,
                ParalegalActorId varchar(128) NULL,
                ParalegalDisplayName varchar(200) NULL,
                LinkedIntakeRecordId char(36) NULL,
                CreatedByActorId varchar(128) NOT NULL,
                CreatedAtUtc datetime(6) NOT NULL,
                PRIMARY KEY (Id),
                UNIQUE INDEX IX_matters_MatterNumber (MatterNumber),
                INDEX IX_matters_Stage (Stage),
                INDEX IX_matters_ResponsibleAttorneyActorId (ResponsibleAttorneyActorId)
            );
            """);
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS matter_stage_history (
                Id char(36) NOT NULL,
                MatterRecordId char(36) NOT NULL,
                FromStage varchar(64) NOT NULL,
                ToStage varchar(64) NOT NULL,
                ChangedByActorId varchar(128) NOT NULL,
                ChangedByDisplayName varchar(200) NOT NULL,
                ChangeReason varchar(1000) NULL,
                ChangedAtUtc datetime(6) NOT NULL,
                PRIMARY KEY (Id),
                INDEX IX_matter_stage_history_MatterRecordId (MatterRecordId),
                CONSTRAINT FK_matter_stage_history_matters_MatterRecordId
                    FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE CASCADE
            );
            """);
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS matter_tasks (
                Id char(36) NOT NULL,
                MatterRecordId char(36) NOT NULL,
                Title varchar(240) NOT NULL,
                Description varchar(4000) NULL,
                Status varchar(64) NOT NULL,
                Priority varchar(32) NOT NULL,
                AssigneeActorId varchar(128) NULL,
                AssigneeDisplayName varchar(200) NULL,
                DueAtUtc datetime(6) NULL,
                IsDeadlineVerified tinyint(1) NOT NULL,
                DeadlineVerifiedAtUtc datetime(6) NULL,
                DeadlineVerifiedByActorId varchar(128) NULL,
                DeadlineVerifiedByDisplayName varchar(200) NULL,
                CompletedAtUtc datetime(6) NULL,
                CreatedByActorId varchar(128) NOT NULL,
                CreatedByDisplayName varchar(200) NOT NULL,
                CreatedAtUtc datetime(6) NOT NULL,
                PRIMARY KEY (Id),
                INDEX IX_matter_tasks_MatterRecordId (MatterRecordId),
                INDEX IX_matter_tasks_DueAtUtc (DueAtUtc),
                CONSTRAINT FK_matter_tasks_matters_MatterRecordId
                    FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE CASCADE
            );
            """);
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS matter_documents (
                Id char(36) NOT NULL,
                MatterRecordId char(36) NOT NULL,
                FileName varchar(240) NOT NULL,
                DocumentType varchar(80) NOT NULL,
                StorageKey varchar(512) NULL,
                UploadedByActorId varchar(128) NOT NULL,
                UploadedByDisplayName varchar(200) NOT NULL,
                UploadedAtUtc datetime(6) NOT NULL,
                ReviewStatus varchar(32) NOT NULL,
                ReviewNotes varchar(2000) NULL,
                ReviewedByActorId varchar(128) NULL,
                ReviewedByDisplayName varchar(200) NULL,
                ReviewedAtUtc datetime(6) NULL,
                PRIMARY KEY (Id),
                INDEX IX_matter_documents_MatterRecordId (MatterRecordId),
                INDEX IX_matter_documents_DocumentType (DocumentType),
                CONSTRAINT FK_matter_documents_matters_MatterRecordId
                    FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE CASCADE
            );
            """);
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS matter_timeline_actions (
                Id char(36) NOT NULL,
                MatterRecordId char(36) NOT NULL,
                ActionType varchar(80) NOT NULL,
                Summary varchar(400) NOT NULL,
                Details varchar(4000) NULL,
                OccurredAtUtc datetime(6) NOT NULL,
                CreatedByActorId varchar(128) NOT NULL,
                CreatedByDisplayName varchar(200) NOT NULL,
                CreatedAtUtc datetime(6) NOT NULL,
                PRIMARY KEY (Id),
                INDEX IX_matter_timeline_actions_MatterRecordId (MatterRecordId),
                INDEX IX_matter_timeline_actions_OccurredAtUtc (OccurredAtUtc),
                CONSTRAINT FK_matter_timeline_actions_matters_MatterRecordId
                    FOREIGN KEY (MatterRecordId) REFERENCES matters (Id) ON DELETE CASCADE
            );
            """);
    }
}
