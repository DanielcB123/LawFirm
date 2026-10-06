using System.Text.Json;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var correlationId = context.GetCorrelationId();
            var payload = new ApiErrorResponse(
                correlationId,
                new ApiError("unexpected_error", "An unexpected error occurred. Refer to the correlation ID for support."));

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
