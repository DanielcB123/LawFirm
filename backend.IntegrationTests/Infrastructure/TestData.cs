using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

namespace EnterpriseKnowledgeAssistant.IntegrationTests.Infrastructure;

public sealed record TestActor(string Id, string Role, string[] Permissions);

/// <summary>Synthetic actors and records. Every call uses fresh identifiers so tests share one database safely.</summary>
public static class TestData
{
    public static string NewId(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 13, 64)];

    public static TestActor Attorney(bool firmWide = false) => new(
        NewId("t-attorney"),
        AuthRoles.Attorney,
        firmWide
            ?
            [
                AuthPermissions.MattersReadAssigned, AuthPermissions.MattersReadAll, AuthPermissions.MattersUpdateAll,
                AuthPermissions.DocumentsUpload, AuthPermissions.DocumentsReview, AuthPermissions.CalendarRead,
                AuthPermissions.CalendarManage, AuthPermissions.ContactsRead, AuthPermissions.ContactsManage,
                AuthPermissions.ConflictsSearch, AuthPermissions.ConflictsReview, AuthPermissions.ContactsReadRestricted
            ]
            :
            [
                AuthPermissions.MattersReadAssigned, AuthPermissions.MattersUpdateAssigned,
                AuthPermissions.DocumentsUpload, AuthPermissions.DocumentsReview, AuthPermissions.CalendarRead,
                AuthPermissions.CalendarManage
            ]);

    public static TestActor Paralegal() => new(
        NewId("t-paralegal"),
        AuthRoles.Paralegal,
        [
            AuthPermissions.MattersReadAssigned, AuthPermissions.MattersUpdateAssigned, AuthPermissions.DocumentsUpload,
            AuthPermissions.DocumentsReview, AuthPermissions.CalendarRead, AuthPermissions.CalendarManage,
            AuthPermissions.ContactsRead, AuthPermissions.ContactsManage, AuthPermissions.ConflictsSearch
        ]);

    public static TestActor ReadOnlyParalegal() => new(
        NewId("t-paralegal-ro"),
        AuthRoles.Paralegal,
        [AuthPermissions.MattersReadAssigned, AuthPermissions.CalendarRead, AuthPermissions.CalendarManage]);

    public static TestActor Admin() => new(
        NewId("t-admin"),
        AuthRoles.Admin,
        [
            AuthPermissions.MattersReadAssigned, AuthPermissions.MattersUpdateAssigned, AuthPermissions.DocumentsReview,
            AuthPermissions.ContactsRead, AuthPermissions.ContactsManage, AuthPermissions.ContactsReadRestricted,
            AuthPermissions.ConflictsSearch, AuthPermissions.ConflictsReview, AuthPermissions.UsersManage
        ]);

    public static TestActor Client() => new(
        NewId("t-client"),
        AuthRoles.Client,
        [AuthPermissions.MattersReadOwn, AuthPermissions.DocumentsUpload, AuthPermissions.BillingReadOwn]);

    public static async Task<MatterRecord> CreateMatterAsync(
        MySqlApiFixture fixture,
        string responsibleAttorneyId,
        string? paralegalId = null)
    {
        var matter = new MatterRecord
        {
            Id = Guid.NewGuid(),
            MatterNumber = NewId("T"),
            Title = "Synthetic matter " + Guid.NewGuid().ToString("N")[..8],
            PracticeArea = "ConstructionLaw",
            Stage = "Active",
            Status = "Active",
            ResponsibleAttorneyActorId = responsibleAttorneyId,
            ResponsibleAttorneyDisplayName = responsibleAttorneyId,
            ParalegalActorId = paralegalId,
            ParalegalDisplayName = paralegalId,
            CreatedByActorId = "t-seed",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await fixture.WithDbAsync(async db =>
        {
            db.MatterRecords.Add(matter);
            await db.SaveChangesAsync();
        });
        return matter;
    }

    public static async Task<MatterAccessGrant> GrantAsync(
        MySqlApiFixture fixture,
        Guid matterId,
        string actorId,
        string grantType,
        bool revoked = false)
    {
        var grant = new MatterAccessGrant
        {
            Id = Guid.NewGuid(),
            MatterRecordId = matterId,
            ActorId = actorId,
            GrantType = grantType,
            GrantedByActorId = "t-seed",
            GrantedAtUtc = DateTimeOffset.UtcNow,
            RevokedAtUtc = revoked ? DateTimeOffset.UtcNow : null,
            RevokedByActorId = revoked ? "t-seed" : null
        };

        await fixture.WithDbAsync(async db =>
        {
            db.MatterAccessGrants.Add(grant);
            await db.SaveChangesAsync();
        });
        return grant;
    }

    public static async Task<MatterTask> CreateTaskAsync(MySqlApiFixture fixture, Guid matterId)
    {
        var task = new MatterTask
        {
            Id = Guid.NewGuid(),
            MatterRecordId = matterId,
            Title = "Synthetic task",
            CreatedByActorId = "t-seed",
            CreatedByDisplayName = "Seed",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        await fixture.WithDbAsync(async db =>
        {
            db.MatterTasks.Add(task);
            await db.SaveChangesAsync();
        });
        return task;
    }

    public static async Task<MatterDocument> CreateDocumentAsync(MySqlApiFixture fixture, Guid matterId, string uploadedBy = "t-seed")
    {
        var document = new MatterDocument
        {
            Id = Guid.NewGuid(),
            MatterRecordId = matterId,
            FileName = "synthetic.pdf",
            DocumentType = "General",
            UploadedByActorId = uploadedBy,
            UploadedByDisplayName = uploadedBy,
            UploadedAtUtc = DateTimeOffset.UtcNow
        };
        await fixture.WithDbAsync(async db =>
        {
            db.MatterDocuments.Add(document);
            await db.SaveChangesAsync();
        });
        return document;
    }

    public static async Task<Party> CreatePartyAsync(MySqlApiFixture fixture, string displayName, bool restricted)
    {
        var party = new Party
        {
            Id = Guid.NewGuid(),
            PartyType = "Person",
            DisplayName = displayName,
            IsRestricted = restricted,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        await fixture.WithDbAsync(async db =>
        {
            db.Parties.Add(party);
            await db.SaveChangesAsync();
        });
        return party;
    }

    public static async Task<CalendarEntry> CreateCalendarEntryAsync(MySqlApiFixture fixture, string? matterReference, string ownerId)
    {
        var entry = new CalendarEntry
        {
            Id = Guid.NewGuid(),
            Title = "Synthetic entry " + Guid.NewGuid().ToString("N")[..8],
            EntryType = "Event",
            DeadlineType = "InternalTarget",
            OwnerActorId = ownerId,
            OwnerDisplayName = ownerId,
            ScheduledAtUtc = DateTimeOffset.UtcNow.AddDays(3),
            TimeZone = "America/Chicago",
            MatterReference = matterReference,
            CreatedByActorId = "t-seed",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        await fixture.WithDbAsync(async db =>
        {
            db.CalendarEntries.Add(entry);
            await db.SaveChangesAsync();
        });
        return entry;
    }
}
