using System.Net;
using System.Net.Http.Json;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using EnterpriseKnowledgeAssistant.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class MatterAccessTests(MySqlApiFixture fixture)
{
    private static readonly string[] ReadRoutes = ["", "/tasks", "/documents", "/timeline"];

    [Fact]
    public async Task Assigned_paralegal_can_read_matter_children_and_sees_it_in_the_list()
    {
        var paralegal = TestData.Paralegal();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"), paralegal.Id);
        var client = fixture.CreateClient(paralegal);

        foreach (var route in ReadRoutes)
        {
            await HttpAssert.StatusAsync(HttpStatusCode.OK, await client.GetAsync($"/api/matters-v2/{matter.Id}{route}"));
        }

        var list = await HttpAssert.JsonAsync(await client.GetAsync("/api/matters-v2"));
        Assert.Contains(matter.Id, HttpAssert.Ids(list));
    }

    [Fact]
    public async Task Unassigned_paralegal_gets_the_same_404_as_for_a_nonexistent_matter_and_list_excludes_it()
    {
        var paralegal = TestData.Paralegal();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"), TestData.NewId("t-paralegal"));
        var client = fixture.CreateClient(paralegal);

        foreach (var route in ReadRoutes)
        {
            await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await client.GetAsync($"/api/matters-v2/{matter.Id}{route}"));
            await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await client.GetAsync($"/api/matters-v2/{Guid.NewGuid()}{route}"));
        }

        var list = await HttpAssert.JsonAsync(await client.GetAsync("/api/matters-v2"));
        Assert.DoesNotContain(matter.Id, HttpAssert.Ids(list));
    }

    [Fact]
    public async Task Unassigned_paralegal_cannot_write_to_matter_children()
    {
        var paralegal = TestData.Paralegal();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var client = fixture.CreateClient(paralegal);

        await HttpAssert.NotFoundWithCodeAsync("matter_not_found",
            await client.PostAsJsonAsync($"/api/matters-v2/{matter.Id}/tasks", new { title = "Injected", isDeadlineVerified = false }));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found",
            await client.PostAsJsonAsync($"/api/matters-v2/{matter.Id}/documents", new { fileName = "x.pdf", documentType = "General" }));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found",
            await client.PostAsJsonAsync($"/api/matters-v2/{matter.Id}/timeline-actions", new { actionType = "Note", summary = "Injected" }));

        await fixture.WithDbAsync(async db =>
        {
            Assert.False(await db.MatterTasks.AnyAsync(task => task.MatterRecordId == matter.Id));
            Assert.False(await db.MatterDocuments.AnyAsync(document => document.MatterRecordId == matter.Id));
            Assert.False(await db.MatterTimelineActions.AnyAsync(action => action.MatterRecordId == matter.Id));
        });
    }

    [Fact]
    public async Task Direct_child_resource_ids_on_an_inaccessible_matter_are_rejected_and_unchanged()
    {
        var paralegal = TestData.Paralegal();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var task = await TestData.CreateTaskAsync(fixture, matter.Id);
        var document = await TestData.CreateDocumentAsync(fixture, matter.Id);
        var client = fixture.CreateClient(paralegal);

        await HttpAssert.NotFoundWithCodeAsync("matter_not_found",
            await client.PostAsJsonAsync($"/api/matters-v2/{matter.Id}/tasks/{task.Id}", new { title = "Changed", status = "Completed" }));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await client.DeleteAsync($"/api/matters-v2/{matter.Id}/tasks/{task.Id}"));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found",
            await client.PostAsJsonAsync($"/api/matters-v2/{matter.Id}/documents/{document.Id}/review", new { reviewStatus = "Approved" }));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await client.DeleteAsync($"/api/matters-v2/{matter.Id}/documents/{document.Id}"));

        await fixture.WithDbAsync(async db =>
        {
            var storedTask = await db.MatterTasks.AsNoTracking().SingleAsync(item => item.Id == task.Id);
            Assert.Equal("Synthetic task", storedTask.Title);
            var storedDocument = await db.MatterDocuments.AsNoTracking().SingleAsync(item => item.Id == document.Id);
            Assert.Equal("Pending", storedDocument.ReviewStatus);
        });
    }

    [Fact]
    public async Task Child_of_an_inaccessible_matter_cannot_be_reached_through_an_accessible_parent_route()
    {
        var paralegal = TestData.Paralegal();
        var ownMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"), paralegal.Id);
        var otherMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var otherTask = await TestData.CreateTaskAsync(fixture, otherMatter.Id);
        var otherDocument = await TestData.CreateDocumentAsync(fixture, otherMatter.Id);
        var client = fixture.CreateClient(paralegal);

        await HttpAssert.NotFoundWithCodeAsync("matter_task_not_found",
            await client.PostAsJsonAsync($"/api/matters-v2/{ownMatter.Id}/tasks/{otherTask.Id}", new { title = "Moved" }));
        await HttpAssert.NotFoundWithCodeAsync("matter_task_not_found", await client.DeleteAsync($"/api/matters-v2/{ownMatter.Id}/tasks/{otherTask.Id}"));
        await HttpAssert.NotFoundWithCodeAsync("matter_document_not_found",
            await client.PostAsJsonAsync($"/api/matters-v2/{ownMatter.Id}/documents/{otherDocument.Id}/review", new { reviewStatus = "Approved" }));
        await HttpAssert.NotFoundWithCodeAsync("matter_document_not_found", await client.DeleteAsync($"/api/matters-v2/{ownMatter.Id}/documents/{otherDocument.Id}"));

        await fixture.WithDbAsync(async db =>
        {
            var storedTask = await db.MatterTasks.AsNoTracking().SingleAsync(item => item.Id == otherTask.Id);
            Assert.Equal(otherMatter.Id, storedTask.MatterRecordId);
            Assert.Equal("Synthetic task", storedTask.Title);
            Assert.True(await db.MatterDocuments.AnyAsync(item => item.Id == otherDocument.Id && item.MatterRecordId == otherMatter.Id));
        });
    }

    [Fact]
    public async Task Staff_grant_allows_access_and_revocation_removes_it()
    {
        var paralegal = TestData.Paralegal();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var grant = await TestData.GrantAsync(fixture, matter.Id, paralegal.Id, MatterAccessGrantTypes.Staff);
        var client = fixture.CreateClient(paralegal);

        await HttpAssert.StatusAsync(HttpStatusCode.OK, await client.GetAsync($"/api/matters-v2/{matter.Id}"));

        await fixture.WithDbAsync(async db =>
        {
            var stored = await db.MatterAccessGrants.SingleAsync(item => item.Id == grant.Id);
            stored.RevokedAtUtc = DateTimeOffset.UtcNow;
            stored.RevokedByActorId = "t-seed";
            await db.SaveChangesAsync();
        });

        await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await client.GetAsync($"/api/matters-v2/{matter.Id}"));
        var list = await HttpAssert.JsonAsync(await client.GetAsync("/api/matters-v2"));
        Assert.DoesNotContain(matter.Id, HttpAssert.Ids(list));
    }

    [Fact]
    public async Task Client_grant_type_does_not_give_staff_route_access()
    {
        var paralegal = TestData.Paralegal();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        await TestData.GrantAsync(fixture, matter.Id, paralegal.Id, MatterAccessGrantTypes.Client);

        await HttpAssert.NotFoundWithCodeAsync("matter_not_found",
            await fixture.CreateClient(paralegal).GetAsync($"/api/matters-v2/{matter.Id}"));
    }

    [Fact]
    public async Task Admin_has_no_implicit_matter_access_but_can_be_granted_it()
    {
        var admin = TestData.Admin();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var client = fixture.CreateClient(admin);

        await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await client.GetAsync($"/api/matters-v2/{matter.Id}"));
        Assert.DoesNotContain(matter.Id, HttpAssert.Ids(await HttpAssert.JsonAsync(await client.GetAsync("/api/matters-v2"))));

        await TestData.GrantAsync(fixture, matter.Id, admin.Id, MatterAccessGrantTypes.Staff);
        await HttpAssert.StatusAsync(HttpStatusCode.OK, await client.GetAsync($"/api/matters-v2/{matter.Id}"));
    }

    [Fact]
    public async Task Firm_wide_attorney_can_read_any_matter()
    {
        var attorney = TestData.Attorney(firmWide: true);
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));

        await HttpAssert.StatusAsync(HttpStatusCode.OK, await fixture.CreateClient(attorney).GetAsync($"/api/matters-v2/{matter.Id}"));
    }

    [Fact]
    public async Task Assigned_staff_without_update_permission_is_forbidden_from_writes()
    {
        var paralegal = TestData.ReadOnlyParalegal();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"), paralegal.Id);
        var client = fixture.CreateClient(paralegal);

        await HttpAssert.StatusAsync(HttpStatusCode.OK, await client.GetAsync($"/api/matters-v2/{matter.Id}"));
        await HttpAssert.StatusAsync(HttpStatusCode.Forbidden,
            await client.PostAsJsonAsync($"/api/matters-v2/{matter.Id}/tasks", new { title = "Nope", isDeadlineVerified = false }));
    }

    [Fact]
    public async Task Client_role_is_forbidden_on_staff_matter_routes_and_cannot_create_documents_there()
    {
        var clientActor = TestData.Client();
        var ownMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var otherMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        await TestData.GrantAsync(fixture, ownMatter.Id, clientActor.Id, MatterAccessGrantTypes.Client);
        var client = fixture.CreateClient(clientActor);

        await HttpAssert.StatusAsync(HttpStatusCode.Forbidden, await client.GetAsync("/api/matters-v2"));
        await HttpAssert.StatusAsync(HttpStatusCode.Forbidden, await client.GetAsync($"/api/matters-v2/{ownMatter.Id}"));
        await HttpAssert.StatusAsync(HttpStatusCode.Forbidden,
            await client.PostAsJsonAsync($"/api/matters-v2/{otherMatter.Id}/documents",
                new { fileName = "planted.pdf", documentType = "General", storageKey = "s3://attacker/planted.pdf" }));

        await fixture.WithDbAsync(async db =>
            Assert.False(await db.MatterDocuments.AnyAsync(document => document.MatterRecordId == otherMatter.Id)));
    }

    [Fact]
    public async Task Responsible_attorney_manages_grants_with_duplicate_rejection_and_revocation()
    {
        var attorney = TestData.Attorney();
        var paralegal = TestData.Paralegal();
        var matter = await TestData.CreateMatterAsync(fixture, attorney.Id);
        var attorneyClient = fixture.CreateClient(attorney);

        var created = await attorneyClient.PostAsJsonAsync($"/api/matters-v2/{matter.Id}/access-grants",
            new { actorId = paralegal.Id, grantType = "Staff" });
        await HttpAssert.StatusAsync(HttpStatusCode.Created, created);
        var grantId = (await HttpAssert.JsonAsync(created)).GetProperty("id").GetGuid();

        await HttpAssert.StatusAsync(HttpStatusCode.Conflict,
            await attorneyClient.PostAsJsonAsync($"/api/matters-v2/{matter.Id}/access-grants", new { actorId = paralegal.Id, grantType = "Staff" }));
        await HttpAssert.StatusAsync(HttpStatusCode.OK, await fixture.CreateClient(paralegal).GetAsync($"/api/matters-v2/{matter.Id}"));

        await HttpAssert.StatusAsync(HttpStatusCode.OK,
            await attorneyClient.PostAsync($"/api/matters-v2/{matter.Id}/access-grants/{grantId}/revoke", null));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await fixture.CreateClient(paralegal).GetAsync($"/api/matters-v2/{matter.Id}"));

        await fixture.WithDbAsync(async db =>
        {
            var stored = await db.MatterAccessGrants.AsNoTracking().SingleAsync(item => item.Id == grantId);
            Assert.NotNull(stored.RevokedAtUtc);
            Assert.Equal(attorney.Id, stored.RevokedByActorId);
            Assert.True(await db.AuditEvents.AnyAsync(item => item.ObjectId == grantId.ToString() && item.Action == "matter.access_grant_revoked"));
        });

        await HttpAssert.StatusAsync(HttpStatusCode.Created,
            await attorneyClient.PostAsJsonAsync($"/api/matters-v2/{matter.Id}/access-grants", new { actorId = paralegal.Id, grantType = "Staff" }));
    }

    [Fact]
    public async Task Staff_who_are_not_the_responsible_attorney_cannot_manage_grants()
    {
        var paralegal = TestData.Paralegal();
        var otherAttorney = TestData.Attorney();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"), paralegal.Id);
        var outsider = TestData.Paralegal();

        await HttpAssert.StatusAsync(HttpStatusCode.Forbidden,
            await fixture.CreateClient(paralegal).PostAsJsonAsync($"/api/matters-v2/{matter.Id}/access-grants", new { actorId = outsider.Id, grantType = "Staff" }));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found",
            await fixture.CreateClient(otherAttorney).PostAsJsonAsync($"/api/matters-v2/{matter.Id}/access-grants", new { actorId = outsider.Id, grantType = "Staff" }));

        await fixture.WithDbAsync(async db =>
            Assert.False(await db.MatterAccessGrants.AnyAsync(grant => grant.MatterRecordId == matter.Id)));
    }

    [Fact]
    public async Task Database_allows_only_one_unrevoked_grant_per_matter_and_actor()
    {
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var actorId = TestData.NewId("t-paralegal");
        await TestData.GrantAsync(fixture, matter.Id, actorId, MatterAccessGrantTypes.Staff, revoked: true);
        await TestData.GrantAsync(fixture, matter.Id, actorId, MatterAccessGrantTypes.Staff, revoked: true);
        await TestData.GrantAsync(fixture, matter.Id, actorId, MatterAccessGrantTypes.Staff);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            TestData.GrantAsync(fixture, matter.Id, actorId, MatterAccessGrantTypes.Client));
    }

    [Fact]
    public async Task Database_rejects_unknown_grant_types_and_grants_for_missing_matters()
    {
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            TestData.GrantAsync(fixture, matter.Id, TestData.NewId("t-x"), "Owner"));
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            TestData.GrantAsync(fixture, Guid.NewGuid(), TestData.NewId("t-x"), MatterAccessGrantTypes.Staff));
    }
}
