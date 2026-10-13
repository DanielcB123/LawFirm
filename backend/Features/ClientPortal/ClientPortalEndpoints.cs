using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Features.Matters;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Features.ClientPortal;

/// <summary>
/// Client-facing matter routes. Clients never use the staff /matters-v2 routes; these return a reduced
/// projection and only matters reached through an unrevoked Client grant.
/// </summary>
public sealed class ClientPortalEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/portal/matters")
            .WithTags("Portal")
            .RequireAuthorization(AuthPolicies.ClientMatterPortal);

        group.MapGet(string.Empty, ListMatters)
            .WithSummary("Lists the caller's own matters.");
        group.MapGet("/{matterId:guid}", GetMatter)
            .WithSummary("Gets a client-safe view of one of the caller's matters.");
        group.MapGet("/{matterId:guid}/documents", ListOwnDocuments)
            .WithSummary("Lists documents the caller submitted to the matter.");
        group.MapPost("/{matterId:guid}/documents", SubmitDocument)
            .RequireAuthorization(AuthPolicies.DocumentUpload)
            .WithSummary("Submits document metadata to one of the caller's matters for staff review.");
    }

    private static async Task<IResult> ListMatters(
        IMatterAccessService access,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var matters = await access.ClientMatters(user)
            .AsNoTracking()
            .OrderByDescending(matter => matter.CreatedAtUtc)
            .Select(matter => new ClientMatterResponse(matter.Id, matter.MatterNumber, matter.Title, matter.PracticeArea, matter.Status))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(matters);
    }

    private static async Task<IResult> GetMatter(
        Guid matterId,
        IMatterAccessService access,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var matter = await access.ClientMatters(user)
            .AsNoTracking()
            .Where(item => item.Id == matterId)
            .Select(item => new ClientMatterResponse(item.Id, item.MatterNumber, item.Title, item.PracticeArea, item.Status))
            .FirstOrDefaultAsync(cancellationToken);

        return matter is null ? MatterAccessResults.MatterNotFound(httpContext) : Results.Ok(matter);
    }

    private static async Task<IResult> ListOwnDocuments(
        Guid matterId,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await access.IsClientOfAsync(user, matterId, cancellationToken))
        {
            return MatterAccessResults.MatterNotFound(httpContext);
        }

        var actorId = ActorClaims.GetActorId(user);
        var documents = await dbContext.MatterDocuments
            .AsNoTracking()
            .Where(document => document.MatterRecordId == matterId && document.UploadedByActorId == actorId)
            .OrderByDescending(document => document.UploadedAtUtc)
            .Select(document => new ClientDocumentResponse(
                document.Id, document.FileName, document.DocumentType, document.UploadedAtUtc, document.ReviewStatus))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(documents);
    }

    private static async Task<IResult> SubmitDocument(
        Guid matterId,
        CreateMatterDocumentRequest request,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await access.IsClientOfAsync(user, matterId, cancellationToken))
        {
            return MatterAccessResults.MatterNotFound(httpContext);
        }

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            errors["fileName"] = ["File name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.DocumentType))
        {
            errors["documentType"] = ["Document type is required."];
        }

        if (errors.Count > 0)
        {
            return httpContext.ValidationError("Document request is invalid.", errors);
        }

        var (actorId, actorDisplayName) = MattersEndpoints.GetActor(user);
        var document = new MatterDocument
        {
            Id = Guid.NewGuid(),
            MatterRecordId = matterId,
            FileName = request.FileName.Trim(),
            DocumentType = request.DocumentType.Trim(),
            // Clients may not point metadata at arbitrary storage; a server-side upload flow assigns keys (G23).
            StorageKey = null,
            UploadedByActorId = actorId,
            UploadedByDisplayName = actorDisplayName,
            UploadedAtUtc = DateTimeOffset.UtcNow,
            ReviewStatus = "Pending"
        };
        dbContext.MatterDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "portal.document_submitted",
            objectType: "matter_document",
            objectId: document.Id.ToString(),
            metadata: new { matterId, document.DocumentType },
            cancellationToken);

        return Results.Created(
            $"/api/portal/matters/{matterId}/documents/{document.Id}",
            new ClientDocumentResponse(document.Id, document.FileName, document.DocumentType, document.UploadedAtUtc, document.ReviewStatus));
    }
}
