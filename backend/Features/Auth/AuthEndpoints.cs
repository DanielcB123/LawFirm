using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;

namespace EnterpriseKnowledgeAssistant.Api.Features.Auth;

public sealed class AuthEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Authentication");

        group.MapPost("/login", Login)
            .WithName("PostLogin")
            .AllowAnonymous()
            .WithSummary("Authenticates a user and returns a bearer token.");

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .RequireAuthorization()
            .WithSummary("Returns current authenticated user details.");
    }

    private static IResult Login(
        LoginRequest request,
        HttpContext httpContext,
        ITokenService tokenService)
    {
        var userStore = httpContext.RequestServices.GetService<IDemoUserStore>();
        if (userStore is null)
        {
            return Results.Json(
                new ApiErrorResponse(
                    httpContext.GetCorrelationId(),
                    new ApiError("sign_in_unavailable", "Password sign-in is not configured for this environment.")),
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var user = userStore.FindByEmail(request.Email);
        if (user is null || !userStore.VerifyPassword(user, request.Password))
        {
            return Results.Unauthorized();
        }

        var token = tokenService.CreateAccessToken(user);
        var response = new LoginResponse(
            token.AccessToken,
            token.TokenType,
            token.ExpiresAtUtc,
            new UserProfileResponse(user.Id, user.Email, user.DisplayName, user.Role, user.Permissions));

        return Results.Ok(response);
    }

    private static IResult GetCurrentUser(ClaimsPrincipal user)
    {
        var permissions = user.Claims
            .Where(claim => claim.Type == AuthConstants.PermissionClaimType)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var response = new UserProfileResponse(
            user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? string.Empty,
            user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email") ?? string.Empty,
            user.FindFirstValue("name") ?? string.Empty,
            user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("role") ?? string.Empty,
            permissions);

        return Results.Ok(response);
    }
}
