using System.Net;
using System.Net.Http.Json;
using EnterpriseKnowledgeAssistant.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class PartyVisibilityTests(MySqlApiFixture fixture)
{
    [Fact]
    public async Task Party_directory_excludes_restricted_parties_without_the_restricted_permission()
    {
        var marker = "Zq" + Guid.NewGuid().ToString("N")[..10];
        var restricted = await TestData.CreatePartyAsync(fixture, $"{marker} Sealed Witness", restricted: true);
        var open = await TestData.CreatePartyAsync(fixture, $"{marker} Open Vendor", restricted: false);

        var paralegalBody = await (await fixture.CreateClient(TestData.Paralegal()).GetAsync($"/api/parties?query={marker}")).Content.ReadAsStringAsync();
        Assert.Contains(open.Id.ToString(), paralegalBody);
        Assert.DoesNotContain(restricted.Id.ToString(), paralegalBody);
        Assert.DoesNotContain("Sealed Witness", paralegalBody);

        var attorneyBody = await (await fixture.CreateClient(TestData.Attorney(firmWide: true)).GetAsync($"/api/parties?query={marker}")).Content.ReadAsStringAsync();
        Assert.Contains(restricted.Id.ToString(), attorneyBody);
    }

    [Fact]
    public async Task Conflict_search_masks_restricted_matches_including_the_party_id()
    {
        var marker = "Zq" + Guid.NewGuid().ToString("N")[..10];
        var restricted = await TestData.CreatePartyAsync(fixture, $"{marker} Sealed Witness", restricted: true);

        var response = await fixture.CreateClient(TestData.Paralegal()).PostAsJsonAsync("/api/conflicts/checks", new { query = marker });
        await HttpAssert.StatusAsync(HttpStatusCode.OK, response);
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(restricted.Id.ToString(), body);
        Assert.DoesNotContain("Sealed Witness", body);
        Assert.Contains("Restricted Party", body);

        await fixture.WithDbAsync(async db =>
        {
            var snapshots = await db.ConflictChecks.AsNoTracking().Where(check => check.Query == marker).Select(check => check.MatchSnapshotJson).ToListAsync();
            Assert.All(snapshots, snapshot => Assert.DoesNotContain(restricted.Id.ToString(), snapshot));
        });
    }

    [Fact]
    public async Task Conflict_search_shows_restricted_details_to_permission_holders()
    {
        var marker = "Zq" + Guid.NewGuid().ToString("N")[..10];
        var restricted = await TestData.CreatePartyAsync(fixture, $"{marker} Sealed Witness", restricted: true);

        var body = await (await fixture.CreateClient(TestData.Attorney(firmWide: true)).PostAsJsonAsync("/api/conflicts/checks", new { query = marker }))
            .Content.ReadAsStringAsync();

        Assert.Contains(restricted.Id.ToString(), body);
        Assert.Contains("Sealed Witness", body);
    }

    [Fact]
    public async Task Restricted_party_cannot_be_modified_or_linked_without_the_restricted_permission()
    {
        var restricted = await TestData.CreatePartyAsync(fixture, "Zq Sealed " + Guid.NewGuid().ToString("N")[..6], restricted: true);
        var open = await TestData.CreatePartyAsync(fixture, "Zq Open " + Guid.NewGuid().ToString("N")[..6], restricted: false);
        var client = fixture.CreateClient(TestData.Paralegal());

        await HttpAssert.NotFoundWithCodeAsync("party_not_found",
            await client.PostAsJsonAsync($"/api/parties/{restricted.Id}/aliases", new { alias = "Probe", aliasType = "Former" }));
        await HttpAssert.NotFoundWithCodeAsync("party_not_found",
            await client.PostAsJsonAsync("/api/parties/relationships", new { fromPartyId = open.Id, toPartyId = restricted.Id, relationshipType = "Affiliate" }));

        await fixture.WithDbAsync(async db =>
        {
            Assert.False(await db.PartyAliases.AnyAsync(alias => alias.PartyId == restricted.Id));
            Assert.False(await db.PartyRelationships.AnyAsync(link => link.ToPartyId == restricted.Id));
        });
    }
}
