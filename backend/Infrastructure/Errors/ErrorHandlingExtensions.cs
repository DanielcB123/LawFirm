using Microsoft.AspNetCore.Mvc;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;

public static class ErrorHandlingExtensions
{
    public static IServiceCollection AddLawFirmErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }

    public static IApplicationBuilder UseLawFirmErrorHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }

    public static string GetCorrelationId(this HttpContext context)
    {
        return context.Items.TryGetValue(CorrelationIdMiddleware.HttpItemName, out var value) &&
               value is string correlationId
            ? correlationId
            : "unknown";
    }

    public static IResult ValidationError(this HttpContext context, string message, IReadOnlyDictionary<string, string[]> details)
    {
        return Results.Json(
            new ApiErrorResponse(
                context.GetCorrelationId(),
                new ApiError("validation_error", message, details)),
            statusCode: StatusCodes.Status400BadRequest);
    }
}
