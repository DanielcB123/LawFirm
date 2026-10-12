using System.Net;
using System.Net.Http.Json;
using EnterpriseKnowledgeAssistant.IntegrationTests.Infrastructure;

namespace EnterpriseKnowledgeAssistant.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class AuthenticationTests(MySqlApiFixture fixture)
{
    [Theory]
    [InlineData("GET", "/api/matters-v2")]
    [InlineData("GET", "/api/matters-v2/{matter}")]
    [InlineData("GET", "/api/matters-v2/{matter}/documents")]
    [InlineData("POST", "/api/matters-v2/{matter}/documents")]
    [InlineData("GET", "/api/matters-v2/{matter}/access-grants")]
    [InlineData("GET", "/api/portal/matters")]
    [InlineData("GET", "/api/portal/matters/{matter}")]
    [InlineData("POST", "/api/portal/matters/{matter}/documents")]
    [InlineData("GET", "/api/calendar/entries")]
    [InlineData("GET", "/api/parties")]
    public async Task Unauthenticated_requests_are_rejected_with_401(string method, string template)
    {
        var matter = await TestData.CreateMatterAsync(fixture, TestData.NewId("t-attorney"));
        var client = fixture.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), template.Replace("{matter}", matter.Id.ToString()));
        if (method == "POST")
        {
            request.Content = JsonContent.Create(new { fileName = "x.pdf", documentType = "General" });
        }

        var response = await client.SendAsync(request);

        await HttpAssert.StatusAsync(HttpStatusCode.Unauthorized, response);
        await fixture.WithDbAsync(async db =>
            Assert.False(db.MatterDocuments.Any(document => document.MatterRecordId == matter.Id)));
    }

    [Fact]
    public async Task Password_sign_in_is_unavailable_when_demo_accounts_are_not_enabled()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "attorney@parkerreedlaw.com", password = "irrelevant" });

        await HttpAssert.StatusAsync(HttpStatusCode.ServiceUnavailable, response);
        Assert.Equal("sign_in_unavailable", await HttpAssert.ErrorCodeAsync(response));
    }
}
