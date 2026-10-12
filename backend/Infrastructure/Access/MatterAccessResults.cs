using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;

/// <summary>
/// Uniform denial responses. A matter the caller cannot read is reported exactly like a nonexistent one;
/// 403 is returned only when the caller can already read the matter and lacks write rights on it.
/// </summary>
public static class MatterAccessResults
{
    public static IResult MatterNotFound(HttpContext httpContext)
    {
        return Results.NotFound(new ApiErrorResponse(
            httpContext.GetCorrelationId(),
            new ApiError("matter_not_found", "Matter was not found.")));
    }

    public static IResult MatterWriteForbidden(HttpContext httpContext)
    {
        return Results.Json(
            new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("matter_write_forbidden", "You do not have permission to change this matter.")),
            statusCode: StatusCodes.Status403Forbidden);
    }

    public static async Task<IResult?> DenyUnlessWritableAsync(
        IMatterAccessService access,
        ClaimsPrincipal user,
        Guid matterId,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (await access.CanWriteAsync(user, matterId, cancellationToken))
        {
            return null;
        }

        return await access.CanReadAsync(user, matterId, cancellationToken)
            ? MatterWriteForbidden(httpContext)
            : MatterNotFound(httpContext);
    }
}
