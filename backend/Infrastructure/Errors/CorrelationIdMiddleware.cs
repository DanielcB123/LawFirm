using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";
    public const string HttpItemName = "CorrelationId";
    private const int MaxCorrelationIdLength = 128;

    public async Task Invoke(HttpContext context)
    {
        var requested = context.Request.Headers.TryGetValue(HeaderName, out var requestedValue)
            ? requestedValue.ToString()
            : null;
        var correlationId = BuildCorrelationId(requested);

        context.Items[HttpItemName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await next(context);
    }

    private static string BuildCorrelationId(string? requested)
    {
        if (string.IsNullOrWhiteSpace(requested))
        {
            return Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
        }

        var cleaned = new string(requested.Trim().Where(IsAllowed).ToArray());
        if (cleaned.Length == 0)
        {
            return Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
        }

        if (cleaned.Length <= MaxCorrelationIdLength)
        {
            return cleaned;
        }

        // Keep correlation stable while bounding untrusted input length.
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(cleaned));
        return "cid-" + Convert.ToHexString(digest)[..28].ToLowerInvariant();
    }

    private static bool IsAllowed(char character)
    {
        return char.IsLetterOrDigit(character) || character is '-' or '_' or '.' or ':';
    }
}
