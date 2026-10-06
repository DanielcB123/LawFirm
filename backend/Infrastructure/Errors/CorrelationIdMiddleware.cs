using System.Diagnostics;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";
    public const string HttpItemName = "CorrelationId";

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var requestedValue) &&
                            !string.IsNullOrWhiteSpace(requestedValue)
            ? requestedValue.ToString()
            : Activity.Current?.Id ?? Guid.NewGuid().ToString("N");

        context.Items[HttpItemName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await next(context);
    }
}
