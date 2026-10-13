using System.Security.Claims;
using System.Text.Json;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Access;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Features.Calendar;

public sealed class CalendarEndpoints : IEndpointModule
{
    private static readonly HashSet<string> AllowedEntryTypes = ["Task", "Event", "Deadline"];
    private static readonly HashSet<string> AllowedDeadlineTypes = ["OrdinaryTask", "InternalTarget", "CourtOrdered", "LegallySignificant"];

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var calendar = app.MapGroup("/calendar")
            .WithTags("Calendar");

        calendar.MapGet("/entries", GetEntries)
            .RequireAuthorization(AuthPolicies.CalendarRead)
            .WithSummary("Gets calendar entries for authorized staff.");

        calendar.MapPost("/entries", CreateEntry)
            .RequireAuthorization(AuthPolicies.CalendarManage)
            .WithSummary("Creates a calendar/task/deadline entry.");

        calendar.MapPut("/entries/{entryId:guid}", UpdateEntry)
            .RequireAuthorization(AuthPolicies.CalendarManage)
            .WithSummary("Updates an existing calendar/task/deadline entry.");

        calendar.MapDelete("/entries/{entryId:guid}", DeleteEntry)
            .RequireAuthorization(AuthPolicies.CalendarManage)
            .WithSummary("Deletes a calendar entry.");

        calendar.MapPost("/entries/{entryId:guid}/acknowledge", AcknowledgeEntry)
            .RequireAuthorization(AuthPolicies.CalendarManage)
            .WithSummary("Records explicit acknowledgment of a critical entry.");

        calendar.MapPost("/entries/{entryId:guid}/complete", CompleteEntry)
            .RequireAuthorization(AuthPolicies.CalendarManage)
            .WithSummary("Marks a calendar entry complete with audit trail.");
    }

    private static async Task<IResult> GetEntries(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var actorId = ActorClaims.GetActorId(user);
        var canReadAllStaffEntries = ActorClaims.IsStaff(user);
        var readableMatterNumbers = access.ReadableMatters(user).Select(matter => matter.MatterNumber);
        // Entries whose reference names no matter (legacy free text) stay visible; linked entries follow matter access.
        var query = dbContext.CalendarEntries
            .AsNoTracking()
            .Where(entry =>
                entry.MatterReference == null ||
                entry.MatterReference == string.Empty ||
                readableMatterNumbers.Contains(entry.MatterReference) ||
                !dbContext.MatterRecords.Any(matter => matter.MatterNumber == entry.MatterReference));

        if (fromUtc.HasValue)
        {
            query = query.Where(entry => entry.ScheduledAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(entry => entry.ScheduledAtUtc <= toUtc.Value);
        }

        if (!canReadAllStaffEntries)
        {
            query = query.Where(entry =>
                entry.OwnerActorId == actorId ||
                entry.BackupActorId == actorId ||
                entry.CreatedByActorId == actorId);
        }

        var persistedEntries = await query
            .Take(300)
            .ToListAsync(cancellationToken);

        var entries = persistedEntries
            .OrderBy(entry => entry.ScheduledAtUtc)
            .Select(entry => new CalendarEntryResponse(
                entry.Id,
                entry.Title,
                entry.EntryType,
                entry.DeadlineType,
                entry.OwnerActorId,
                entry.OwnerDisplayName,
                entry.BackupActorId,
                entry.BackupDisplayName,
                entry.ScheduledAtUtc,
                entry.IsAllDay,
                entry.TimeZone,
                entry.MatterReference,
                entry.SourceReference,
                entry.VerifiedAtUtc,
                entry.VerifiedByActorId,
                entry.VerifiedByDisplayName,
                entry.OverrideReason,
                entry.IsAcknowledged,
                entry.AcknowledgedAtUtc,
                entry.IsCompleted,
                entry.CompletedAtUtc,
                JsonSerializer.Deserialize<int[]>(entry.RemindersJson) ?? Array.Empty<int>(),
                entry.CreatedAtUtc))
            .ToArray();

        return Results.Ok(entries);
    }

    private static async Task<IResult> CreateEntry(
        CreateCalendarEntryRequest request,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateEntryRequest(request);
        var matterReference = await ResolveWritableMatterReferenceAsync(request.MatterReference, access, user, validationErrors, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return httpContext.ValidationError("Calendar entry request is invalid.", validationErrors);
        }

        var actorId = ActorClaims.GetActorId(user);
        var actorName = user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
        var entry = new CalendarEntry
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            EntryType = request.EntryType.Trim(),
            DeadlineType = request.DeadlineType.Trim(),
            OwnerActorId = request.OwnerActorId.Trim(),
            OwnerDisplayName = request.OwnerDisplayName.Trim(),
            BackupActorId = request.BackupActorId?.Trim(),
            BackupDisplayName = request.BackupDisplayName?.Trim(),
            ScheduledAtUtc = request.ScheduledAtUtc,
            IsAllDay = request.IsAllDay,
            TimeZone = request.TimeZone.Trim(),
            MatterReference = matterReference,
            SourceReference = request.SourceReference?.Trim(),
            VerifiedAtUtc = request.VerifiedAtUtc,
            VerifiedByActorId = request.VerifiedByActorId?.Trim(),
            VerifiedByDisplayName = request.VerifiedByDisplayName?.Trim(),
            OverrideReason = request.OverrideReason?.Trim(),
            RemindersJson = JsonSerializer.Serialize((request.ReminderOffsetsMinutes ?? []).Distinct().OrderBy(minutes => minutes).ToArray()),
            CreatedByActorId = actorId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.CalendarEntries.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "calendar.entry_created",
            objectType: "calendar_entry",
            objectId: entry.Id.ToString(),
            metadata: new
            {
                entry.EntryType,
                entry.DeadlineType,
                entry.OwnerActorId,
                createdBy = actorName
            },
            cancellationToken);

        return Results.Created($"/api/calendar/entries/{entry.Id}", new CalendarEntryResponse(
            entry.Id,
            entry.Title,
            entry.EntryType,
            entry.DeadlineType,
            entry.OwnerActorId,
            entry.OwnerDisplayName,
            entry.BackupActorId,
            entry.BackupDisplayName,
            entry.ScheduledAtUtc,
            entry.IsAllDay,
            entry.TimeZone,
            entry.MatterReference,
            entry.SourceReference,
            entry.VerifiedAtUtc,
            entry.VerifiedByActorId,
            entry.VerifiedByDisplayName,
            entry.OverrideReason,
            entry.IsAcknowledged,
            entry.AcknowledgedAtUtc,
            entry.IsCompleted,
            entry.CompletedAtUtc,
            JsonSerializer.Deserialize<int[]>(entry.RemindersJson) ?? Array.Empty<int>(),
            entry.CreatedAtUtc));
    }

    private static async Task<IResult> UpdateEntry(
        Guid entryId,
        UpdateCalendarEntryRequest request,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var (entry, denied) = await FindWritableEntryAsync(entryId, dbContext, access, user, httpContext, cancellationToken);
        if (entry is null)
        {
            return denied!;
        }

        var validationErrors = ValidateEntryRequest(request);
        var matterReference = await ResolveWritableMatterReferenceAsync(request.MatterReference, access, user, validationErrors, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return httpContext.ValidationError("Calendar entry request is invalid.", validationErrors);
        }

        entry.Title = request.Title.Trim();
        entry.EntryType = request.EntryType.Trim();
        entry.DeadlineType = request.DeadlineType.Trim();
        entry.OwnerActorId = request.OwnerActorId.Trim();
        entry.OwnerDisplayName = request.OwnerDisplayName.Trim();
        entry.BackupActorId = request.BackupActorId?.Trim();
        entry.BackupDisplayName = request.BackupDisplayName?.Trim();
        entry.ScheduledAtUtc = request.ScheduledAtUtc;
        entry.IsAllDay = request.IsAllDay;
        entry.TimeZone = request.TimeZone.Trim();
        entry.MatterReference = matterReference;
        entry.SourceReference = request.SourceReference?.Trim();
        entry.VerifiedAtUtc = request.VerifiedAtUtc;
        entry.VerifiedByActorId = request.VerifiedByActorId?.Trim();
        entry.VerifiedByDisplayName = request.VerifiedByDisplayName?.Trim();
        entry.OverrideReason = request.OverrideReason?.Trim();
        entry.RemindersJson = JsonSerializer.Serialize((request.ReminderOffsetsMinutes ?? []).Distinct().OrderBy(minutes => minutes).ToArray());

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "calendar.entry_updated",
            objectType: "calendar_entry",
            objectId: entry.Id.ToString(),
            metadata: new { entry.EntryType, entry.DeadlineType, entry.OwnerActorId },
            cancellationToken);

        return Results.Ok(new CalendarEntryResponse(
            entry.Id,
            entry.Title,
            entry.EntryType,
            entry.DeadlineType,
            entry.OwnerActorId,
            entry.OwnerDisplayName,
            entry.BackupActorId,
            entry.BackupDisplayName,
            entry.ScheduledAtUtc,
            entry.IsAllDay,
            entry.TimeZone,
            entry.MatterReference,
            entry.SourceReference,
            entry.VerifiedAtUtc,
            entry.VerifiedByActorId,
            entry.VerifiedByDisplayName,
            entry.OverrideReason,
            entry.IsAcknowledged,
            entry.AcknowledgedAtUtc,
            entry.IsCompleted,
            entry.CompletedAtUtc,
            JsonSerializer.Deserialize<int[]>(entry.RemindersJson) ?? Array.Empty<int>(),
            entry.CreatedAtUtc));
    }

    private static async Task<IResult> DeleteEntry(
        Guid entryId,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var (entry, denied) = await FindWritableEntryAsync(entryId, dbContext, access, user, httpContext, cancellationToken);
        if (entry is null)
        {
            return denied!;
        }

        dbContext.CalendarEntries.Remove(entry);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "calendar.entry_deleted",
            objectType: "calendar_entry",
            objectId: entry.Id.ToString(),
            metadata: new { entry.Title, entry.OwnerActorId, entry.ScheduledAtUtc },
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AcknowledgeEntry(
        Guid entryId,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var (entry, denied) = await FindWritableEntryAsync(entryId, dbContext, access, user, httpContext, cancellationToken);
        if (entry is null)
        {
            return denied!;
        }

        if (entry.IsAcknowledged)
        {
            return Results.Ok();
        }

        entry.IsAcknowledged = true;
        entry.AcknowledgedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "calendar.entry_acknowledged",
            objectType: "calendar_entry",
            objectId: entry.Id.ToString(),
            metadata: new { entry.OwnerActorId, entry.EntryType },
            cancellationToken);

        return Results.Ok();
    }

    private static async Task<IResult> CompleteEntry(
        Guid entryId,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var (entry, denied) = await FindWritableEntryAsync(entryId, dbContext, access, user, httpContext, cancellationToken);
        if (entry is null)
        {
            return denied!;
        }

        if (entry.IsCompleted)
        {
            return Results.Ok();
        }

        entry.IsCompleted = true;
        entry.CompletedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "calendar.entry_completed",
            objectType: "calendar_entry",
            objectId: entry.Id.ToString(),
            metadata: new { entry.OwnerActorId, entry.EntryType },
            cancellationToken);

        return Results.Ok();
    }

    /// <summary>
    /// Loads an entry for modification. Entries linked to a matter the caller cannot read are reported as not
    /// found; linked to a matter the caller can read but not change, as forbidden.
    /// </summary>
    private static async Task<(CalendarEntry? Entry, IResult? Denied)> FindWritableEntryAsync(
        Guid entryId,
        LawFirmDbContext dbContext,
        IMatterAccessService access,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var notFound = Results.NotFound(new ApiErrorResponse(
            httpContext.GetCorrelationId(),
            new ApiError("calendar_entry_not_found", "Calendar entry was not found.")));

        var entry = await dbContext.CalendarEntries.FirstOrDefaultAsync(existingEntry => existingEntry.Id == entryId, cancellationToken);
        if (entry is null)
        {
            return (null, notFound);
        }

        if (string.IsNullOrEmpty(entry.MatterReference))
        {
            return (entry, null);
        }

        var linkedMatterId = await dbContext.MatterRecords
            .Where(matter => matter.MatterNumber == entry.MatterReference)
            .Select(matter => (Guid?)matter.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (linkedMatterId is null || await access.CanWriteAsync(user, linkedMatterId.Value, cancellationToken))
        {
            return (entry, null);
        }

        return await access.CanReadAsync(user, linkedMatterId.Value, cancellationToken)
            ? (null, MatterAccessResults.MatterWriteForbidden(httpContext))
            : (null, notFound);
    }

    /// <summary>
    /// New and updated entries may only reference a matter the caller can change. A matter the caller cannot
    /// reach is reported the same way as a nonexistent one. Returns the canonical matter number.
    /// </summary>
    private static async Task<string?> ResolveWritableMatterReferenceAsync(
        string? requestedReference,
        IMatterAccessService access,
        ClaimsPrincipal user,
        Dictionary<string, string[]> validationErrors,
        CancellationToken cancellationToken)
    {
        var trimmed = requestedReference?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        var matterNumber = await access.WritableMatters(user)
            .Where(matter => matter.MatterNumber == trimmed)
            .Select(matter => matter.MatterNumber)
            .FirstOrDefaultAsync(cancellationToken);
        if (matterNumber is null)
        {
            validationErrors["matterReference"] = ["Matter reference was not found."];
        }

        return matterNumber;
    }

    private static Dictionary<string, string[]> ValidateEntryRequest(IWriteCalendarEntryRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors["title"] = ["Title is required."];
        }

        if (string.IsNullOrWhiteSpace(request.OwnerActorId) || string.IsNullOrWhiteSpace(request.OwnerDisplayName))
        {
            errors["owner"] = ["Owner actor id and display name are required."];
        }

        if (string.IsNullOrWhiteSpace(request.EntryType) || !AllowedEntryTypes.Contains(request.EntryType.Trim()))
        {
            errors["entryType"] = ["Entry type must be Task, Event, or Deadline."];
        }

        if (string.IsNullOrWhiteSpace(request.DeadlineType) || !AllowedDeadlineTypes.Contains(request.DeadlineType.Trim()))
        {
            errors["deadlineType"] = ["Deadline type must be OrdinaryTask, InternalTarget, CourtOrdered, or LegallySignificant."];
        }

        if (string.IsNullOrWhiteSpace(request.TimeZone))
        {
            errors["timeZone"] = ["Time zone is required."];
        }

        return errors;
    }
}
