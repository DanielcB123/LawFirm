using System.Net;
using System.Net.Http.Json;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using EnterpriseKnowledgeAssistant.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class ClientPortalTests(MySqlApiFixture fixture)
{
    [Fact]
    public async Task Client_lists_and_reads_only_granted_matters_with_a_minimal_projection()
    {
        var clientActor = TestData.Client();
        var ownMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var otherMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        await TestData.GrantAsync(fixture, ownMatter.Id, clientActor.Id, MatterAccessGrantTypes.Client);
        var client = fixture.CreateClient(clientActor);

        var listResponse = await client.GetAsync("/api/portal/matters");
        await HttpAssert.StatusAsync(HttpStatusCode.OK, listResponse);
        var ids = HttpAssert.Ids(await HttpAssert.JsonAsync(listResponse)).ToArray();
        Assert.Equal([ownMatter.Id], ids);

        var detailResponse = await client.GetAsync($"/api/portal/matters/{ownMatter.Id}");
        await HttpAssert.StatusAsync(HttpStatusCode.OK, detailResponse);
        var detail = await HttpAssert.JsonAsync(detailResponse);
        Assert.Equal(ownMatter.MatterNumber, detail.GetProperty("matterNumber").GetString());
        Assert.False(detail.TryGetProperty("responsibleAttorneyActorId", out _));
        Assert.False(detail.TryGetProperty("stageHistory", out _));

        Assert.DoesNotContain(otherMatter.Id, ids);
    }

    [Fact]
    public async Task Client_guessing_another_matter_id_gets_the_same_404_as_a_nonexistent_id()
    {
        var clientActor = TestData.Client();
        var otherClient = TestData.Client();
        var otherMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        await TestData.GrantAsync(fixture, otherMatter.Id, otherClient.Id, MatterAccessGrantTypes.Client);
        var client = fixture.CreateClient(clientActor);

        await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await client.GetAsync($"/api/portal/matters/{otherMatter.Id}"));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await client.GetAsync($"/api/portal/matters/{Guid.NewGuid()}"));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await client.GetAsync($"/api/portal/matters/{otherMatter.Id}/documents"));
    }

    [Fact]
    public async Task Client_cannot_create_a_document_on_another_clients_matter()
    {
        var clientActor = TestData.Client();
        var otherMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        await TestData.GrantAsync(fixture, otherMatter.Id, TestData.NewId("t-client"), MatterAccessGrantTypes.Client);

        var response = await fixture.CreateClient(clientActor).PostAsJsonAsync(
            $"/api/portal/matters/{otherMatter.Id}/documents",
            new { fileName = "planted.pdf", documentType = "General", storageKey = "s3://attacker/planted.pdf" });

        await HttpAssert.NotFoundWithCodeAsync("matter_not_found", response);
        await fixture.WithDbAsync(async db =>
            Assert.False(await db.MatterDocuments.AnyAsync(document => document.MatterRecordId == otherMatter.Id)));
    }

    [Fact]
    public async Task Client_upload_on_own_matter_is_pending_and_ignores_caller_storage_key()
    {
        var clientActor = TestData.Client();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        await TestData.GrantAsync(fixture, matter.Id, clientActor.Id, MatterAccessGrantTypes.Client);
        await TestData.CreateDocumentAsync(fixture, matter.Id, uploadedBy: "t-staff-internal");
        var client = fixture.CreateClient(clientActor);

        var created = await client.PostAsJsonAsync($"/api/portal/matters/{matter.Id}/documents",
            new { fileName = "photos.zip", documentType = "Evidence", storageKey = "s3://attacker/other-matter/secret.pdf" });
        await HttpAssert.StatusAsync(HttpStatusCode.Created, created);
        var documentId = (await HttpAssert.JsonAsync(created)).GetProperty("id").GetGuid();

        await fixture.WithDbAsync(async db =>
        {
            var stored = await db.MatterDocuments.AsNoTracking().SingleAsync(item => item.Id == documentId);
            Assert.Null(stored.StorageKey);
            Assert.Equal("Pending", stored.ReviewStatus);
            Assert.Equal(clientActor.Id, stored.UploadedByActorId);
        });

        var listed = HttpAssert.Ids(await HttpAssert.JsonAsync(await client.GetAsync($"/api/portal/matters/{matter.Id}/documents"))).ToArray();
        Assert.Equal([documentId], listed);
    }

    [Fact]
    public async Task Revoked_client_grant_removes_list_detail_and_upload_access()
    {
        var clientActor = TestData.Client();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        await TestData.GrantAsync(fixture, matter.Id, clientActor.Id, MatterAccessGrantTypes.Client, revoked: true);
        var client = fixture.CreateClient(clientActor);

        Assert.DoesNotContain(matter.Id, HttpAssert.Ids(await HttpAssert.JsonAsync(await client.GetAsync("/api/portal/matters"))));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found", await client.GetAsync($"/api/portal/matters/{matter.Id}"));
        await HttpAssert.NotFoundWithCodeAsync("matter_not_found",
            await client.PostAsJsonAsync($"/api/portal/matters/{matter.Id}/documents", new { fileName = "x.pdf", documentType = "General" }));
    }

    [Fact]
    public async Task Staff_grant_type_does_not_give_a_client_portal_access()
    {
        var clientActor = TestData.Client();
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        await TestData.GrantAsync(fixture, matter.Id, clientActor.Id, MatterAccessGrantTypes.Staff);

        await HttpAssert.NotFoundWithCodeAsync("matter_not_found",
            await fixture.CreateClient(clientActor).GetAsync($"/api/portal/matters/{matter.Id}"));
    }

    [Fact]
    public async Task Staff_cannot_use_client_portal_routes()
    {
        var paralegal = TestData.Paralegal();
        await HttpAssert.StatusAsync(HttpStatusCode.Forbidden, await fixture.CreateClient(paralegal).GetAsync("/api/portal/matters"));
    }
}
