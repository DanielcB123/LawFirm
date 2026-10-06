using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Features.Matters;

public sealed class MattersEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/matters-v2").WithTags("Matters");

        group.MapGet(string.Empty, ListMatters)
            .RequireAuthorization(AuthPolicies.MatterReadAssigned)
            .WithSummary("Lists matters in the new workspace shell.");

        group.MapGet("/{matterId:guid}", GetMatterById)
            .RequireAuthorization(AuthPolicies.MatterReadAssigned)
            .WithSummary("Gets matter shell details.");

        group.MapGet("/{matterId:guid}/tasks", ListTasks)
            .RequireAuthorization(AuthPolicies.MatterReadAssigned)
            .WithSummary("Lists matter tasks and deadlines.");
        group.MapPost("/{matterId:guid}/tasks", CreateTask)
            .RequireAuthorization(AuthPolicies.MatterUpdateAssigned)
            .WithSummary("Creates a matter task/deadline.");
        group.MapPost("/{matterId:guid}/tasks/{taskId:guid}", UpdateTask)
            .RequireAuthorization(AuthPolicies.MatterUpdateAssigned)
            .WithSummary("Updates a matter task/deadline.");
        group.MapDelete("/{matterId:guid}/tasks/{taskId:guid}", DeleteTask)
            .RequireAuthorization(AuthPolicies.MatterUpdateAssigned)
            .WithSummary("Deletes a matter task/deadline.");

        group.MapGet("/{matterId:guid}/documents", ListDocuments)
            .RequireAuthorization(AuthPolicies.MatterReadAssigned)
            .WithSummary("Lists matter document index metadata.");
        group.MapPost("/{matterId:guid}/documents", CreateDocument)
            .RequireAuthorization(AuthPolicies.DocumentUpload)
            .WithSummary("Adds matter document metadata.");
        group.MapPost("/{matterId:guid}/documents/{documentId:guid}/review", ReviewDocument)
            .RequireAuthorization(AuthPolicies.DocumentReview)
            .WithSummary("Updates review status on matter document metadata.");
        group.MapDelete("/{matterId:guid}/documents/{documentId:guid}", DeleteDocument)
            .RequireAuthorization(AuthPolicies.DocumentReview)
            .WithSummary("Deletes matter document metadata.");

        group.MapGet("/{matterId:guid}/timeline", ListTimelineActions)
            .RequireAuthorization(AuthPolicies.MatterReadAssigned)
            .WithSummary("Lists custom matter timeline actions.");
        group.MapPost("/{matterId:guid}/timeline-actions", CreateTimelineAction)
            .RequireAuthorization(AuthPolicies.MatterUpdateAssigned)
            .WithSummary("Creates custom matter timeline action.");
    }

    private static async Task<IResult> ListMatters(
        string? stage,
        string? practiceArea,
        string? status,
        LawFirmDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.MatterRecords.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(stage))
        {
            query = query.Where(record => record.Stage == stage.Trim());
        }

        if (!string.IsNullOrWhiteSpace(practiceArea))
        {
            query = query.Where(record => record.PracticeArea == practiceArea.Trim());
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(record => record.Status == status.Trim());
        }

        var matters = await query
            .OrderByDescending(record => record.CreatedAtUtc)
            .Take(200)
            .Select(record => new MatterListItemResponse(
                record.Id,
                record.MatterNumber,
                record.Title,
                record.PracticeArea,
                record.Stage,
                record.Status,
                record.ResponsibleAttorneyActorId,
                record.ResponsibleAttorneyDisplayName,
                record.ParalegalActorId,
                record.ParalegalDisplayName,
                record.LinkedIntakeRecordId,
                record.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(matters);
    }

    private static async Task<IResult> GetMatterById(
        Guid matterId,
        LawFirmDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var matter = await dbContext.MatterRecords
            .AsNoTracking()
            .Include(record => record.StageHistory)
            .FirstOrDefaultAsync(record => record.Id == matterId, cancellationToken);

        if (matter is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("matter_not_found", "Matter was not found.")));
        }

        return Results.Ok(new MatterDetailResponse(
            matter.Id,
            matter.MatterNumber,
            matter.Title,
            matter.PracticeArea,
            matter.Stage,
            matter.Status,
            matter.ResponsibleAttorneyActorId,
            matter.ResponsibleAttorneyDisplayName,
            matter.ParalegalActorId,
            matter.ParalegalDisplayName,
            matter.LinkedIntakeRecordId,
            matter.CreatedAtUtc,
            matter.StageHistory
                .OrderBy(item => item.ChangedAtUtc)
                .Select(item => new MatterStageHistoryResponse(
                    item.Id,
                    item.FromStage,
                    item.ToStage,
                    item.ChangedByActorId,
                    item.ChangedByDisplayName,
                    item.ChangeReason,
                    item.ChangedAtUtc))
                .ToArray()));
    }

    private static async Task<IResult> ListTasks(
        Guid matterId,
        LawFirmDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await MatterExists(dbContext, matterId, cancellationToken))
        {
            return MatterNotFound(httpContext);
        }

        var tasks = await dbContext.MatterTasks
            .AsNoTracking()
            .Where(task => task.MatterRecordId == matterId)
            .OrderBy(task => task.DueAtUtc.HasValue)
            .ThenBy(task => task.DueAtUtc)
            .ThenByDescending(task => task.CreatedAtUtc)
            .Select(task => new MatterTaskResponse(
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.Priority,
                task.AssigneeActorId,
                task.AssigneeDisplayName,
                task.DueAtUtc,
                task.IsDeadlineVerified,
                task.DeadlineVerifiedAtUtc,
                task.DeadlineVerifiedByActorId,
                task.DeadlineVerifiedByDisplayName,
                task.CompletedAtUtc,
                task.CreatedByActorId,
                task.CreatedByDisplayName,
                task.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(tasks);
    }

    private static async Task<IResult> CreateTask(
        Guid matterId,
        CreateMatterTaskRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await MatterExists(dbContext, matterId, cancellationToken))
        {
            return MatterNotFound(httpContext);
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return httpContext.ValidationError("Task request is invalid.", new Dictionary<string, string[]>
            {
                ["title"] = ["Task title is required."]
            });
        }

        var (actorId, actorDisplayName) = GetActor(user);
        var task = new MatterTask
        {
            Id = Guid.NewGuid(),
            MatterRecordId = matterId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Open" : request.Status.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Normal" : request.Priority.Trim(),
            AssigneeActorId = request.AssigneeActorId?.Trim(),
            AssigneeDisplayName = request.AssigneeDisplayName?.Trim(),
            DueAtUtc = request.DueAtUtc,
            IsDeadlineVerified = request.IsDeadlineVerified,
            DeadlineVerifiedAtUtc = request.IsDeadlineVerified ? DateTimeOffset.UtcNow : null,
            DeadlineVerifiedByActorId = request.IsDeadlineVerified ? actorId : null,
            DeadlineVerifiedByDisplayName = request.IsDeadlineVerified ? actorDisplayName : null,
            CompletedAtUtc = string.Equals(request.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                ? DateTimeOffset.UtcNow
                : null,
            CreatedByActorId = actorId,
            CreatedByDisplayName = actorDisplayName,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        dbContext.MatterTasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "matter.task_created",
            objectType: "matter_task",
            objectId: task.Id.ToString(),
            metadata: new { matterId, task.Status, task.Priority },
            cancellationToken);

        return Results.Created($"/api/matters-v2/{matterId}/tasks/{task.Id}", ToTaskResponse(task));
    }

    private static async Task<IResult> UpdateTask(
        Guid matterId,
        Guid taskId,
        UpdateMatterTaskRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.MatterTasks
            .FirstOrDefaultAsync(item => item.MatterRecordId == matterId && item.Id == taskId, cancellationToken);
        if (task is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("matter_task_not_found", "Matter task was not found.")));
        }

        var (actorId, actorDisplayName) = GetActor(user);
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            task.Title = request.Title.Trim();
        }

        task.Description = request.Description?.Trim();
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            task.Status = request.Status.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            task.Priority = request.Priority.Trim();
        }

        task.AssigneeActorId = request.AssigneeActorId?.Trim();
        task.AssigneeDisplayName = request.AssigneeDisplayName?.Trim();
        task.DueAtUtc = request.DueAtUtc;

        if (request.IsDeadlineVerified.HasValue)
        {
            task.IsDeadlineVerified = request.IsDeadlineVerified.Value;
            task.DeadlineVerifiedAtUtc = request.IsDeadlineVerified.Value ? DateTimeOffset.UtcNow : null;
            task.DeadlineVerifiedByActorId = request.IsDeadlineVerified.Value ? actorId : null;
            task.DeadlineVerifiedByDisplayName = request.IsDeadlineVerified.Value ? actorDisplayName : null;
        }

        task.CompletedAtUtc = string.Equals(task.Status, "Completed", StringComparison.OrdinalIgnoreCase)
            ? DateTimeOffset.UtcNow
            : null;

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "matter.task_updated",
            objectType: "matter_task",
            objectId: task.Id.ToString(),
            metadata: new { matterId, task.Status, task.IsDeadlineVerified },
            cancellationToken);

        return Results.Ok(ToTaskResponse(task));
    }

    private static async Task<IResult> DeleteTask(
        Guid matterId,
        Guid taskId,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.MatterTasks
            .FirstOrDefaultAsync(item => item.MatterRecordId == matterId && item.Id == taskId, cancellationToken);
        if (task is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("matter_task_not_found", "Matter task was not found.")));
        }

        dbContext.MatterTasks.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "matter.task_deleted",
            objectType: "matter_task",
            objectId: taskId.ToString(),
            metadata: new { matterId },
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ListDocuments(
        Guid matterId,
        LawFirmDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await MatterExists(dbContext, matterId, cancellationToken))
        {
            return MatterNotFound(httpContext);
        }

        var documents = await dbContext.MatterDocuments
            .AsNoTracking()
            .Where(document => document.MatterRecordId == matterId)
            .OrderByDescending(document => document.UploadedAtUtc)
            .Select(document => new MatterDocumentResponse(
                document.Id,
                document.FileName,
                document.DocumentType,
                document.StorageKey,
                document.UploadedByActorId,
                document.UploadedByDisplayName,
                document.UploadedAtUtc,
                document.ReviewStatus,
                document.ReviewNotes,
                document.ReviewedByActorId,
                document.ReviewedByDisplayName,
                document.ReviewedAtUtc))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(documents);
    }

    private static async Task<IResult> CreateDocument(
        Guid matterId,
        CreateMatterDocumentRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await MatterExists(dbContext, matterId, cancellationToken))
        {
            return MatterNotFound(httpContext);
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return httpContext.ValidationError("Document request is invalid.", new Dictionary<string, string[]>
            {
                ["fileName"] = ["File name is required."]
            });
        }

        if (string.IsNullOrWhiteSpace(request.DocumentType))
        {
            return httpContext.ValidationError("Document request is invalid.", new Dictionary<string, string[]>
            {
                ["documentType"] = ["Document type is required."]
            });
        }

        var (actorId, actorDisplayName) = GetActor(user);
        var document = new MatterDocument
        {
            Id = Guid.NewGuid(),
            MatterRecordId = matterId,
            FileName = request.FileName.Trim(),
            DocumentType = request.DocumentType.Trim(),
            StorageKey = request.StorageKey?.Trim(),
            UploadedByActorId = actorId,
            UploadedByDisplayName = actorDisplayName,
            UploadedAtUtc = DateTimeOffset.UtcNow,
            ReviewStatus = "Pending"
        };
        dbContext.MatterDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "matter.document_added",
            objectType: "matter_document",
            objectId: document.Id.ToString(),
            metadata: new { matterId, document.DocumentType },
            cancellationToken);

        return Results.Created($"/api/matters-v2/{matterId}/documents/{document.Id}", ToDocumentResponse(document));
    }

    private static async Task<IResult> ReviewDocument(
        Guid matterId,
        Guid documentId,
        ReviewMatterDocumentRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.MatterDocuments
            .FirstOrDefaultAsync(item => item.MatterRecordId == matterId && item.Id == documentId, cancellationToken);
        if (document is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("matter_document_not_found", "Matter document was not found.")));
        }

        if (string.IsNullOrWhiteSpace(request.ReviewStatus))
        {
            return httpContext.ValidationError("Document review request is invalid.", new Dictionary<string, string[]>
            {
                ["reviewStatus"] = ["Review status is required."]
            });
        }

        var (actorId, actorDisplayName) = GetActor(user);
        document.ReviewStatus = request.ReviewStatus.Trim();
        document.ReviewNotes = request.ReviewNotes?.Trim();
        document.ReviewedByActorId = actorId;
        document.ReviewedByDisplayName = actorDisplayName;
        document.ReviewedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "matter.document_reviewed",
            objectType: "matter_document",
            objectId: document.Id.ToString(),
            metadata: new { matterId, document.ReviewStatus },
            cancellationToken);

        return Results.Ok(ToDocumentResponse(document));
    }

    private static async Task<IResult> DeleteDocument(
        Guid matterId,
        Guid documentId,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var document = await dbContext.MatterDocuments
            .FirstOrDefaultAsync(item => item.MatterRecordId == matterId && item.Id == documentId, cancellationToken);
        if (document is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("matter_document_not_found", "Matter document was not found.")));
        }

        dbContext.MatterDocuments.Remove(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "matter.document_deleted",
            objectType: "matter_document",
            objectId: documentId.ToString(),
            metadata: new { matterId },
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ListTimelineActions(
        Guid matterId,
        LawFirmDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await MatterExists(dbContext, matterId, cancellationToken))
        {
            return MatterNotFound(httpContext);
        }

        var actions = await dbContext.MatterTimelineActions
            .AsNoTracking()
            .Where(action => action.MatterRecordId == matterId)
            .OrderByDescending(action => action.OccurredAtUtc)
            .Select(action => new MatterTimelineActionResponse(
                action.Id,
                action.ActionType,
                action.Summary,
                action.Details,
                action.OccurredAtUtc,
                action.CreatedByActorId,
                action.CreatedByDisplayName,
                action.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(actions);
    }

    private static async Task<IResult> CreateTimelineAction(
        Guid matterId,
        CreateMatterTimelineActionRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!await MatterExists(dbContext, matterId, cancellationToken))
        {
            return MatterNotFound(httpContext);
        }

        if (string.IsNullOrWhiteSpace(request.ActionType) || string.IsNullOrWhiteSpace(request.Summary))
        {
            return httpContext.ValidationError("Timeline action request is invalid.", new Dictionary<string, string[]>
            {
                ["actionType"] = ["Action type is required."],
                ["summary"] = ["Summary is required."]
            });
        }

        var (actorId, actorDisplayName) = GetActor(user);
        var action = new MatterTimelineAction
        {
            Id = Guid.NewGuid(),
            MatterRecordId = matterId,
            ActionType = request.ActionType.Trim(),
            Summary = request.Summary.Trim(),
            Details = request.Details?.Trim(),
            OccurredAtUtc = request.OccurredAtUtc ?? DateTimeOffset.UtcNow,
            CreatedByActorId = actorId,
            CreatedByDisplayName = actorDisplayName,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        dbContext.MatterTimelineActions.Add(action);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "matter.timeline_action_added",
            objectType: "matter_timeline_action",
            objectId: action.Id.ToString(),
            metadata: new { matterId, action.ActionType },
            cancellationToken);

        return Results.Created($"/api/matters-v2/{matterId}/timeline-actions/{action.Id}", ToTimelineResponse(action));
    }

    private static async Task<bool> MatterExists(LawFirmDbContext dbContext, Guid matterId, CancellationToken cancellationToken)
    {
        return await dbContext.MatterRecords.AnyAsync(record => record.Id == matterId, cancellationToken);
    }

    private static IResult MatterNotFound(HttpContext httpContext)
    {
        return Results.NotFound(new ApiErrorResponse(
            httpContext.GetCorrelationId(),
            new ApiError("matter_not_found", "Matter was not found.")));
    }

    private static (string actorId, string actorDisplayName) GetActor(ClaimsPrincipal user)
    {
        var actorId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? string.Empty;
        var actorDisplayName = user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        return (actorId, actorDisplayName);
    }

    private static MatterTaskResponse ToTaskResponse(MatterTask task)
    {
        return new MatterTaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.AssigneeActorId,
            task.AssigneeDisplayName,
            task.DueAtUtc,
            task.IsDeadlineVerified,
            task.DeadlineVerifiedAtUtc,
            task.DeadlineVerifiedByActorId,
            task.DeadlineVerifiedByDisplayName,
            task.CompletedAtUtc,
            task.CreatedByActorId,
            task.CreatedByDisplayName,
            task.CreatedAtUtc);
    }

    private static MatterDocumentResponse ToDocumentResponse(MatterDocument document)
    {
        return new MatterDocumentResponse(
            document.Id,
            document.FileName,
            document.DocumentType,
            document.StorageKey,
            document.UploadedByActorId,
            document.UploadedByDisplayName,
            document.UploadedAtUtc,
            document.ReviewStatus,
            document.ReviewNotes,
            document.ReviewedByActorId,
            document.ReviewedByDisplayName,
            document.ReviewedAtUtc);
    }

    private static MatterTimelineActionResponse ToTimelineResponse(MatterTimelineAction action)
    {
        return new MatterTimelineActionResponse(
            action.Id,
            action.ActionType,
            action.Summary,
            action.Details,
            action.OccurredAtUtc,
            action.CreatedByActorId,
            action.CreatedByDisplayName,
            action.CreatedAtUtc);
    }
}

public sealed record MatterListItemResponse(
    Guid Id,
    string MatterNumber,
    string Title,
    string PracticeArea,
    string Stage,
    string Status,
    string ResponsibleAttorneyActorId,
    string ResponsibleAttorneyDisplayName,
    string? ParalegalActorId,
    string? ParalegalDisplayName,
    Guid? LinkedIntakeRecordId,
    DateTimeOffset CreatedAtUtc);

public sealed record MatterStageHistoryResponse(
    Guid Id,
    string FromStage,
    string ToStage,
    string ChangedByActorId,
    string ChangedByDisplayName,
    string? ChangeReason,
    DateTimeOffset ChangedAtUtc);

public sealed record MatterDetailResponse(
    Guid Id,
    string MatterNumber,
    string Title,
    string PracticeArea,
    string Stage,
    string Status,
    string ResponsibleAttorneyActorId,
    string ResponsibleAttorneyDisplayName,
    string? ParalegalActorId,
    string? ParalegalDisplayName,
    Guid? LinkedIntakeRecordId,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyCollection<MatterStageHistoryResponse> StageHistory);

public sealed record MatterTaskResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    string? AssigneeActorId,
    string? AssigneeDisplayName,
    DateTimeOffset? DueAtUtc,
    bool IsDeadlineVerified,
    DateTimeOffset? DeadlineVerifiedAtUtc,
    string? DeadlineVerifiedByActorId,
    string? DeadlineVerifiedByDisplayName,
    DateTimeOffset? CompletedAtUtc,
    string CreatedByActorId,
    string CreatedByDisplayName,
    DateTimeOffset CreatedAtUtc);

public sealed record CreateMatterTaskRequest(
    string Title,
    string? Description,
    string? Status,
    string? Priority,
    string? AssigneeActorId,
    string? AssigneeDisplayName,
    DateTimeOffset? DueAtUtc,
    bool IsDeadlineVerified);

public sealed record UpdateMatterTaskRequest(
    string? Title,
    string? Description,
    string? Status,
    string? Priority,
    string? AssigneeActorId,
    string? AssigneeDisplayName,
    DateTimeOffset? DueAtUtc,
    bool? IsDeadlineVerified);

public sealed record MatterDocumentResponse(
    Guid Id,
    string FileName,
    string DocumentType,
    string? StorageKey,
    string UploadedByActorId,
    string UploadedByDisplayName,
    DateTimeOffset UploadedAtUtc,
    string ReviewStatus,
    string? ReviewNotes,
    string? ReviewedByActorId,
    string? ReviewedByDisplayName,
    DateTimeOffset? ReviewedAtUtc);

public sealed record CreateMatterDocumentRequest(
    string FileName,
    string DocumentType,
    string? StorageKey);

public sealed record ReviewMatterDocumentRequest(
    string ReviewStatus,
    string? ReviewNotes);

public sealed record MatterTimelineActionResponse(
    Guid Id,
    string ActionType,
    string Summary,
    string? Details,
    DateTimeOffset OccurredAtUtc,
    string CreatedByActorId,
    string CreatedByDisplayName,
    DateTimeOffset CreatedAtUtc);

public sealed record CreateMatterTimelineActionRequest(
    string ActionType,
    string Summary,
    string? Details,
    DateTimeOffset? OccurredAtUtc);
