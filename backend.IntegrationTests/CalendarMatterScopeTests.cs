using System.Net;
using System.Net.Http.Json;
using EnterpriseKnowledgeAssistant.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class CalendarMatterScopeTests(MySqlApiFixture fixture)
{
    [Fact]
    public async Task Calendar_list_hides_entries_linked_to_inaccessible_matters()
    {
        var paralegal = TestData.Paralegal();
        var ownMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"), paralegal.Id);
        var otherMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var visible = await TestData.CreateCalendarEntryAsync(fixture, ownMatter.MatterNumber, TestData.NewId("t-owner"));
        var hidden = await TestData.CreateCalendarEntryAsync(fixture, otherMatter.MatterNumber, TestData.NewId("t-owner"));

        var ids = HttpAssert.Ids(await HttpAssert.JsonAsync(await fixture.CreateClient(paralegal).GetAsync("/api/calendar/entries"))).ToArray();

        Assert.Contains(visible.Id, ids);
        Assert.DoesNotContain(hidden.Id, ids);
    }

    [Fact]
    public async Task Calendar_entry_on_inaccessible_matter_cannot_be_changed_by_id()
    {
        var paralegal = TestData.Paralegal();
        var otherMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var entry = await TestData.CreateCalendarEntryAsync(fixture, otherMatter.MatterNumber, TestData.NewId("t-owner"));
        var client = fixture.CreateClient(paralegal);

        await HttpAssert.NotFoundWithCodeAsync("calendar_entry_not_found",
            await client.PutAsJsonAsync($"/api/calendar/entries/{entry.Id}", EntryBody("Changed", otherMatter.MatterNumber)));
        await HttpAssert.NotFoundWithCodeAsync("calendar_entry_not_found", await client.PostAsync($"/api/calendar/entries/{entry.Id}/complete", null));
        await HttpAssert.NotFoundWithCodeAsync("calendar_entry_not_found", await client.PostAsync($"/api/calendar/entries/{entry.Id}/acknowledge", null));
        await HttpAssert.NotFoundWithCodeAsync("calendar_entry_not_found", await client.DeleteAsync($"/api/calendar/entries/{entry.Id}"));

        await fixture.WithDbAsync(async db =>
        {
            var stored = await db.CalendarEntries.AsNoTracking().SingleAsync(item => item.Id == entry.Id);
            Assert.Equal(entry.Title, stored.Title);
            Assert.False(stored.IsCompleted);
            Assert.False(stored.IsAcknowledged);
        });
    }

    [Fact]
    public async Task Calendar_entry_cannot_be_created_on_or_moved_to_an_inaccessible_matter()
    {
        var paralegal = TestData.Paralegal();
        var ownMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"), paralegal.Id);
        var otherMatter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var ownEntry = await TestData.CreateCalendarEntryAsync(fixture, ownMatter.MatterNumber, paralegal.Id);
        var client = fixture.CreateClient(paralegal);

        var createOther = await client.PostAsJsonAsync("/api/calendar/entries", EntryBody("Planted", otherMatter.MatterNumber));
        var createMissing = await client.PostAsJsonAsync("/api/calendar/entries", EntryBody("Planted", "NO-SUCH-MATTER"));
        await HttpAssert.StatusAsync(HttpStatusCode.BadRequest, createOther);
        await HttpAssert.StatusAsync(HttpStatusCode.BadRequest, createMissing);
        Assert.Equal(Strip(await createMissing.Content.ReadAsStringAsync()), Strip(await createOther.Content.ReadAsStringAsync()));

        await HttpAssert.StatusAsync(HttpStatusCode.BadRequest,
            await client.PutAsJsonAsync($"/api/calendar/entries/{ownEntry.Id}", EntryBody("Moved", otherMatter.MatterNumber)));

        await fixture.WithDbAsync(async db =>
        {
            Assert.False(await db.CalendarEntries.AnyAsync(item => item.MatterReference == otherMatter.MatterNumber && item.Title == "Planted"));
            var stored = await db.CalendarEntries.AsNoTracking().SingleAsync(item => item.Id == ownEntry.Id);
            Assert.Equal(ownMatter.MatterNumber, stored.MatterReference);
        });

        await HttpAssert.StatusAsync(HttpStatusCode.Created,
            await client.PostAsJsonAsync("/api/calendar/entries", EntryBody("Allowed", ownMatter.MatterNumber)));
    }

    private static object EntryBody(string title, string matterReference) => new
    {
        title,
        entryType = "Event",
        deadlineType = "InternalTarget",
        ownerActorId = "t-owner",
        ownerDisplayName = "Owner",
        scheduledAtUtc = DateTimeOffset.UtcNow.AddDays(5),
        isAllDay = false,
        timeZone = "America/Chicago",
        matterReference
    };

    private static string Strip(string body)
    {
        // Correlation ids differ per request; compare the rest of the error.
        using var document = System.Text.Json.JsonDocument.Parse(body);
        return document.RootElement.GetProperty("error").GetRawText();
    }
}
