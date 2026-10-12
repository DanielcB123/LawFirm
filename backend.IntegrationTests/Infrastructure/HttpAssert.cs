using System.Net;
using System.Text.Json;

namespace EnterpriseKnowledgeAssistant.IntegrationTests.Infrastructure;

public static class HttpAssert
{
    public static async Task StatusAsync(HttpStatusCode expected, HttpResponseMessage response)
    {
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected {(int)expected} {expected} but got {(int)response.StatusCode} {response.StatusCode} " +
                        $"for {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri?.PathAndQuery}. Body: {body}");
        }
    }

    /// <summary>Inaccessible and nonexistent resources must be indistinguishable to the caller.</summary>
    public static async Task NotFoundWithCodeAsync(string expectedCode, HttpResponseMessage response)
    {
        await StatusAsync(HttpStatusCode.NotFound, response);
        Assert.Equal(expectedCode, await ErrorCodeAsync(response));
    }

    public static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.TryGetProperty("error", out var error) &&
               error.TryGetProperty("code", out var code)
            ? code.GetString()
            : null;
    }

    public static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }

    public static IEnumerable<Guid> Ids(JsonElement array, string property = "id")
    {
        return array.EnumerateArray().Select(item => item.GetProperty(property).GetGuid());
    }
}
