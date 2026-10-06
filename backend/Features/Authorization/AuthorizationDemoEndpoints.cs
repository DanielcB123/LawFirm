using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;

namespace EnterpriseKnowledgeAssistant.Api.Features.Authorization;

public sealed class AuthorizationDemoEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var portal = app.MapGroup("/portal").WithTags("Portal");

        portal.MapGet("/client", () => Results.Ok(new
            {
                section = "Client Portal",
                actions = new[] { "View own matters", "Upload documents", "View invoices" }
            }))
            .RequireAuthorization(AuthPolicies.ClientPortal)
            .WithSummary("Client-facing portal access.");

        portal.MapGet("/staff", () => Results.Ok(new
            {
                section = "Staff Portal",
                actions = new[] { "Manage assigned matters", "Review case documents", "Coordinate filings" }
            }))
            .RequireAuthorization(AuthPolicies.StaffPortal)
            .WithSummary("Attorney and paralegal portal access.");

        var matters = app.MapGroup("/matters").WithTags("Matters");

        matters.MapGet("/mine", () => Results.Ok(new { dataScope = "Own client matters" }))
            .RequireAuthorization(AuthPolicies.MatterReadOwn);

        matters.MapGet("/assigned", () => Results.Ok(new { dataScope = "Assigned matters for legal staff" }))
            .RequireAuthorization(AuthPolicies.MatterReadAssigned);

        matters.MapGet("/all", () => Results.Ok(new { dataScope = "All firm matters" }))
            .RequireAuthorization(AuthPolicies.MatterReadAll);

        matters.MapPost("/assigned/update", () => Results.Ok(new { result = "Assigned matter updated" }))
            .RequireAuthorization(AuthPolicies.MatterUpdateAssigned);

        matters.MapPost("/all/update", () => Results.Ok(new { result = "Firm-wide matter updated" }))
            .RequireAuthorization(AuthPolicies.MatterUpdateAll);

        app.MapPost("/documents/upload", () => Results.Ok(new { result = "Document uploaded" }))
            .RequireAuthorization(AuthPolicies.DocumentUpload)
            .WithTags("Documents");

        app.MapPost("/documents/review", () => Results.Ok(new { result = "Document reviewed" }))
            .RequireAuthorization(AuthPolicies.DocumentReview)
            .WithTags("Documents");

        app.MapGet("/billing/me", () => Results.Ok(new { dataScope = "Own invoices and payments" }))
            .RequireAuthorization(AuthPolicies.BillingReadOwn)
            .WithTags("Billing");

        app.MapGet("/billing/manage", () => Results.Ok(new { dataScope = "Firm billing controls" }))
            .RequireAuthorization(AuthPolicies.BillingManage)
            .WithTags("Billing");

        app.MapPost("/users/manage", () => Results.Ok(new { result = "User administration action completed" }))
            .RequireAuthorization(AuthPolicies.UsersManage)
            .WithTags("Users");
    }
}
