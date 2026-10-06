using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Features.Intake;

public sealed class IntakeEndpoints : IEndpointModule
{
    private static readonly IReadOnlyDictionary<string, string[]> AllowedTransitions =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [IntakeStages.New] = [IntakeStages.Contacted, IntakeStages.UnderReview, IntakeStages.Declined],
            [IntakeStages.Contacted] = [IntakeStages.ConsultationScheduled, IntakeStages.UnderReview, IntakeStages.Declined],
            [IntakeStages.ConsultationScheduled] = [IntakeStages.UnderReview, IntakeStages.Declined],
            [IntakeStages.UnderReview] = [IntakeStages.Accepted, IntakeStages.Declined, IntakeStages.ReferredElsewhere],
            [IntakeStages.Accepted] = [],
            [IntakeStages.Declined] = [],
            [IntakeStages.ReferredElsewhere] = []
        };

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/intake").WithTags("Intake");

        group.MapGet(string.Empty, ListIntakes)
            .RequireAuthorization(AuthPolicies.IntakeRead)
            .WithSummary("Lists intake records for pipeline views.");

        group.MapGet("/{intakeId:guid}", GetIntakeById)
            .RequireAuthorization(AuthPolicies.IntakeRead)
            .WithSummary("Gets intake details.");

        group.MapPost(string.Empty, CreateIntake)
            .RequireAuthorization(AuthPolicies.IntakeManage)
            .WithSummary("Creates a new intake record.");

        group.MapPost("/{intakeId:guid}/stage", ChangeStage)
            .RequireAuthorization(AuthPolicies.IntakeManage)
            .WithSummary("Changes intake stage with transition checks and conflict gate.");

        group.MapPost("/{intakeId:guid}/notes", AddNote)
            .RequireAuthorization(AuthPolicies.IntakeManage)
            .WithSummary("Adds an intake note.");

        group.MapPost("/{intakeId:guid}/convert-to-matter", ConvertToMatter)
            .RequireAuthorization(AuthPolicies.IntakeManage)
            .WithSummary("Converts accepted intake into a matter shell.");
    }

    private static async Task<IResult> ListIntakes(
        string? stage,
        string? ownerActorId,
        string? practiceArea,
        LawFirmDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.IntakeRecords
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(stage))
        {
            query = query.Where(record => record.Stage == stage.Trim());
        }

        if (!string.IsNullOrWhiteSpace(ownerActorId))
        {
            query = query.Where(record => record.OwnerActorId == ownerActorId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(practiceArea))
        {
            query = query.Where(record => record.PracticeArea == practiceArea.Trim());
        }

        var records = await query
            .OrderByDescending(record => record.CreatedAtUtc)
            .Take(200)
            .Select(record => new IntakeListItemResponse(
                record.Id,
                record.ProspectiveClientName,
                record.PracticeArea,
                record.IntakeType,
                record.Stage,
                record.OwnerActorId,
                record.OwnerDisplayName,
                record.NextAction,
                record.NextActionDueAtUtc,
                record.ReferralSource,
                record.ConflictCheckId,
                record.ApprovedConflictDecisionId,
                record.CreatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(records);
    }

    private static async Task<IResult> GetIntakeById(
        Guid intakeId,
        LawFirmDbContext dbContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var record = await dbContext.IntakeRecords
            .AsNoTracking()
            .Include(existingRecord => existingRecord.StageHistory)
            .Include(existingRecord => existingRecord.Notes)
            .FirstOrDefaultAsync(existingRecord => existingRecord.Id == intakeId, cancellationToken);

        if (record is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("intake_not_found", "Intake record was not found.")));
        }

        var response = new IntakeDetailResponse(
            record.Id,
            record.ProspectiveClientName,
            record.ProspectiveClientEmail,
            record.ProspectiveClientPhone,
            record.PracticeArea,
            record.IntakeType,
            record.Stage,
            record.OwnerActorId,
            record.OwnerDisplayName,
            record.NextAction,
            record.NextActionDueAtUtc,
            record.ReferralSource,
            record.ConflictCheckId,
            record.ApprovedConflictDecisionId,
            record.DeclineReason,
            record.NotesSummary,
            record.CreatedAtUtc,
            record.StageHistory
                .OrderBy(history => history.ChangedAtUtc)
                .Select(history => new IntakeStageHistoryResponse(
                    history.Id,
                    history.FromStage,
                    history.ToStage,
                    history.ChangedByActorId,
                    history.ChangedByDisplayName,
                    history.ChangeReason,
                    history.ChangedAtUtc))
                .ToArray(),
            record.Notes
                .OrderByDescending(note => note.CreatedAtUtc)
                .Select(note => new IntakeNoteResponse(
                    note.Id,
                    note.Note,
                    note.CreatedByActorId,
                    note.CreatedByDisplayName,
                    note.CreatedAtUtc))
                .ToArray());

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateIntake(
        CreateIntakeRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateCreateRequest(request);
        if (validationErrors.Count > 0)
        {
            return httpContext.ValidationError("Intake request is invalid.", validationErrors);
        }

        var actorId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? string.Empty;
        var actorDisplayName = user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var record = new IntakeRecord
        {
            Id = Guid.NewGuid(),
            ProspectiveClientName = request.ProspectiveClientName.Trim(),
            ProspectiveClientEmail = request.ProspectiveClientEmail?.Trim(),
            ProspectiveClientPhone = request.ProspectiveClientPhone?.Trim(),
            PracticeArea = request.PracticeArea.Trim(),
            IntakeType = request.IntakeType.Trim(),
            Stage = IntakeStages.New,
            OwnerActorId = request.OwnerActorId.Trim(),
            OwnerDisplayName = request.OwnerDisplayName.Trim(),
            NextAction = request.NextAction?.Trim(),
            NextActionDueAtUtc = request.NextActionDueAtUtc,
            ReferralSource = request.ReferralSource?.Trim(),
            ConflictCheckId = request.ConflictCheckId,
            NotesSummary = request.NotesSummary?.Trim(),
            CreatedByActorId = actorId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.IntakeRecords.Add(record);
        dbContext.IntakeStageHistories.Add(new IntakeStageHistory
        {
            Id = Guid.NewGuid(),
            IntakeRecordId = record.Id,
            FromStage = IntakeStages.New,
            ToStage = IntakeStages.New,
            ChangedByActorId = actorId,
            ChangedByDisplayName = actorDisplayName,
            ChangeReason = "Record created.",
            ChangedAtUtc = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "intake.created",
            objectType: "intake_record",
            objectId: record.Id.ToString(),
            metadata: new { record.Stage, record.PracticeArea, record.OwnerActorId },
            cancellationToken);

        return Results.Created($"/api/intake/{record.Id}", new IntakeListItemResponse(
            record.Id,
            record.ProspectiveClientName,
            record.PracticeArea,
            record.IntakeType,
            record.Stage,
            record.OwnerActorId,
            record.OwnerDisplayName,
            record.NextAction,
            record.NextActionDueAtUtc,
            record.ReferralSource,
            record.ConflictCheckId,
            record.ApprovedConflictDecisionId,
            record.CreatedAtUtc));
    }

    private static async Task<IResult> ChangeStage(
        Guid intakeId,
        ChangeIntakeStageRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ToStage))
        {
            return httpContext.ValidationError("Intake stage request is invalid.", new Dictionary<string, string[]>
            {
                ["toStage"] = ["Target stage is required."]
            });
        }

        var record = await dbContext.IntakeRecords.FirstOrDefaultAsync(existingRecord => existingRecord.Id == intakeId, cancellationToken);
        if (record is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("intake_not_found", "Intake record was not found.")));
        }

        var targetStage = request.ToStage.Trim();
        if (!AllowedTransitions.TryGetValue(record.Stage, out var nextStages) ||
            !nextStages.Contains(targetStage, StringComparer.OrdinalIgnoreCase))
        {
            return httpContext.ValidationError("Intake stage transition is invalid.", new Dictionary<string, string[]>
            {
                ["toStage"] = [$"Transition from {record.Stage} to {targetStage} is not allowed."]
            });
        }

        if (string.Equals(targetStage, IntakeStages.Accepted, StringComparison.OrdinalIgnoreCase))
        {
            if (!request.ApprovedConflictDecisionId.HasValue)
            {
                return httpContext.ValidationError("Accepted transition requires approved conflict decision.", new Dictionary<string, string[]>
                {
                    ["approvedConflictDecisionId"] = ["Approved conflict decision id is required when accepting intake."]
                });
            }

            var conflictDecision = await dbContext.ConflictDecisions
                .AsNoTracking()
                .FirstOrDefaultAsync(decision => decision.Id == request.ApprovedConflictDecisionId.Value, cancellationToken);
            if (conflictDecision is null || !string.Equals(conflictDecision.Decision, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                return httpContext.ValidationError("Accepted transition requires approved conflict decision.", new Dictionary<string, string[]>
                {
                    ["approvedConflictDecisionId"] = ["Conflict decision must exist and be Approved."]
                });
            }

            record.ApprovedConflictDecisionId = request.ApprovedConflictDecisionId;
            record.ConflictCheckId = conflictDecision.ConflictCheckId;
        }

        var actorId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? string.Empty;
        var actorDisplayName = user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var previousStage = record.Stage;
        record.Stage = targetStage;
        record.NextAction = request.NextAction?.Trim();
        record.NextActionDueAtUtc = request.NextActionDueAtUtc;
        if (!string.IsNullOrWhiteSpace(request.DeclineReason))
        {
            record.DeclineReason = request.DeclineReason.Trim();
        }

        dbContext.IntakeStageHistories.Add(new IntakeStageHistory
        {
            Id = Guid.NewGuid(),
            IntakeRecordId = record.Id,
            FromStage = previousStage,
            ToStage = targetStage,
            ChangedByActorId = actorId,
            ChangedByDisplayName = actorDisplayName,
            ChangeReason = request.ChangeReason?.Trim(),
            ChangedAtUtc = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "intake.stage_changed",
            objectType: "intake_record",
            objectId: record.Id.ToString(),
            metadata: new { from = previousStage, to = record.Stage, record.ApprovedConflictDecisionId },
            cancellationToken);

        return Results.Ok(new { record.Id, record.Stage });
    }

    private static async Task<IResult> AddNote(
        Guid intakeId,
        AddIntakeNoteRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
        {
            return httpContext.ValidationError("Intake note is invalid.", new Dictionary<string, string[]>
            {
                ["note"] = ["Note is required."]
            });
        }

        var exists = await dbContext.IntakeRecords.AnyAsync(record => record.Id == intakeId, cancellationToken);
        if (!exists)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("intake_not_found", "Intake record was not found.")));
        }

        var actorId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? string.Empty;
        var actorDisplayName = user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var note = new IntakeNote
        {
            Id = Guid.NewGuid(),
            IntakeRecordId = intakeId,
            Note = request.Note.Trim(),
            CreatedByActorId = actorId,
            CreatedByDisplayName = actorDisplayName,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.IntakeNotes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "intake.note_added",
            objectType: "intake_record",
            objectId: intakeId.ToString(),
            metadata: new { note.Id },
            cancellationToken);

        return Results.Created($"/api/intake/{intakeId}", new IntakeNoteResponse(
            note.Id,
            note.Note,
            note.CreatedByActorId,
            note.CreatedByDisplayName,
            note.CreatedAtUtc));
    }

    private static async Task<IResult> ConvertToMatter(
        Guid intakeId,
        ConvertIntakeToMatterRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var intake = await dbContext.IntakeRecords.FirstOrDefaultAsync(record => record.Id == intakeId, cancellationToken);
        if (intake is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("intake_not_found", "Intake record was not found.")));
        }

        if (!string.Equals(intake.Stage, IntakeStages.Accepted, StringComparison.OrdinalIgnoreCase))
        {
            return httpContext.ValidationError("Only accepted intake records can be converted to matters.", new Dictionary<string, string[]>
            {
                ["stage"] = ["Intake must be in Accepted stage before conversion."]
            });
        }

        var existingMatter = await dbContext.MatterRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.LinkedIntakeRecordId == intake.Id, cancellationToken);
        if (existingMatter is not null)
        {
            return Results.Ok(new ConvertIntakeToMatterResponse(existingMatter.Id, existingMatter.MatterNumber));
        }

        if (string.IsNullOrWhiteSpace(request.MatterTitle) || string.IsNullOrWhiteSpace(request.ResponsibleAttorneyActorId))
        {
            return httpContext.ValidationError("Matter conversion request is invalid.", new Dictionary<string, string[]>
            {
                ["matterTitle"] = ["Matter title is required."],
                ["responsibleAttorneyActorId"] = ["Responsible attorney actor id is required."]
            });
        }

        var actorId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? string.Empty;
        var actorDisplayName = user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var matter = new MatterRecord
        {
            Id = Guid.NewGuid(),
            MatterNumber = await GenerateMatterNumberAsync(dbContext, cancellationToken),
            Title = request.MatterTitle.Trim(),
            PracticeArea = intake.PracticeArea,
            Stage = "Open",
            Status = "Active",
            ResponsibleAttorneyActorId = request.ResponsibleAttorneyActorId.Trim(),
            ResponsibleAttorneyDisplayName = request.ResponsibleAttorneyDisplayName?.Trim() ?? request.ResponsibleAttorneyActorId.Trim(),
            ParalegalActorId = request.ParalegalActorId?.Trim(),
            ParalegalDisplayName = request.ParalegalDisplayName?.Trim(),
            LinkedIntakeRecordId = intake.Id,
            CreatedByActorId = actorId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.MatterRecords.Add(matter);
        dbContext.MatterStageHistories.Add(new MatterStageHistory
        {
            Id = Guid.NewGuid(),
            MatterRecordId = matter.Id,
            FromStage = "Created",
            ToStage = matter.Stage,
            ChangedByActorId = actorId,
            ChangedByDisplayName = actorDisplayName,
            ChangeReason = "Converted from accepted intake.",
            ChangedAtUtc = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "intake.converted_to_matter",
            objectType: "intake_record",
            objectId: intake.Id.ToString(),
            metadata: new { matter.Id, matter.MatterNumber, matter.PracticeArea },
            cancellationToken);

        return Results.Ok(new ConvertIntakeToMatterResponse(matter.Id, matter.MatterNumber));
    }

    private static Dictionary<string, string[]> ValidateCreateRequest(CreateIntakeRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.ProspectiveClientName))
        {
            errors["prospectiveClientName"] = ["Prospective client name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.PracticeArea))
        {
            errors["practiceArea"] = ["Practice area is required."];
        }

        if (string.IsNullOrWhiteSpace(request.IntakeType))
        {
            errors["intakeType"] = ["Intake type is required."];
        }

        if (string.IsNullOrWhiteSpace(request.OwnerActorId) || string.IsNullOrWhiteSpace(request.OwnerDisplayName))
        {
            errors["owner"] = ["Owner actor id and display name are required."];
        }

        return errors;
    }

    private static async Task<string> GenerateMatterNumberAsync(LawFirmDbContext dbContext, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"MAT-{year}-";
        var existing = await dbContext.MatterRecords
            .AsNoTracking()
            .Where(record => record.MatterNumber.StartsWith(prefix))
            .Select(record => record.MatterNumber)
            .ToListAsync(cancellationToken);
        var next = existing
            .Select(number =>
            {
                var suffix = number.Replace(prefix, string.Empty);
                return int.TryParse(suffix, out var parsed) ? parsed : 0;
            })
            .DefaultIfEmpty(0)
            .Max() + 1;
        return $"{prefix}{next:D4}";
    }
}

public sealed record IntakeListItemResponse(
    Guid Id,
    string ProspectiveClientName,
    string PracticeArea,
    string IntakeType,
    string Stage,
    string OwnerActorId,
    string OwnerDisplayName,
    string? NextAction,
    DateTimeOffset? NextActionDueAtUtc,
    string? ReferralSource,
    Guid? ConflictCheckId,
    Guid? ApprovedConflictDecisionId,
    DateTimeOffset CreatedAtUtc);

public sealed record IntakeStageHistoryResponse(
    Guid Id,
    string FromStage,
    string ToStage,
    string ChangedByActorId,
    string ChangedByDisplayName,
    string? ChangeReason,
    DateTimeOffset ChangedAtUtc);

public sealed record IntakeNoteResponse(
    Guid Id,
    string Note,
    string CreatedByActorId,
    string CreatedByDisplayName,
    DateTimeOffset CreatedAtUtc);

public sealed record IntakeDetailResponse(
    Guid Id,
    string ProspectiveClientName,
    string? ProspectiveClientEmail,
    string? ProspectiveClientPhone,
    string PracticeArea,
    string IntakeType,
    string Stage,
    string OwnerActorId,
    string OwnerDisplayName,
    string? NextAction,
    DateTimeOffset? NextActionDueAtUtc,
    string? ReferralSource,
    Guid? ConflictCheckId,
    Guid? ApprovedConflictDecisionId,
    string? DeclineReason,
    string? NotesSummary,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyCollection<IntakeStageHistoryResponse> StageHistory,
    IReadOnlyCollection<IntakeNoteResponse> Notes);

public sealed record CreateIntakeRequest(
    string ProspectiveClientName,
    string? ProspectiveClientEmail,
    string? ProspectiveClientPhone,
    string PracticeArea,
    string IntakeType,
    string OwnerActorId,
    string OwnerDisplayName,
    string? NextAction,
    DateTimeOffset? NextActionDueAtUtc,
    string? ReferralSource,
    Guid? ConflictCheckId,
    string? NotesSummary);

public sealed record ChangeIntakeStageRequest(
    string ToStage,
    string? ChangeReason,
    string? NextAction,
    DateTimeOffset? NextActionDueAtUtc,
    string? DeclineReason,
    Guid? ApprovedConflictDecisionId);

public sealed record AddIntakeNoteRequest(string Note);

public sealed record ConvertIntakeToMatterRequest(
    string MatterTitle,
    string ResponsibleAttorneyActorId,
    string? ResponsibleAttorneyDisplayName,
    string? ParalegalActorId,
    string? ParalegalDisplayName);

public sealed record ConvertIntakeToMatterResponse(Guid MatterId, string MatterNumber);
