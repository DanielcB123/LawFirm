using System.Security.Claims;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Auth;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Endpoints;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Features.Consultations;

public sealed class ConsultationsEndpoints : IEndpointModule
{
    private static readonly HashSet<string> AllowedStatuses =
    [
        ConsultationStatuses.New,
        ConsultationStatuses.InReview,
        ConsultationStatuses.Contacted,
        ConsultationStatuses.Scheduled,
        ConsultationStatuses.Closed,
        ConsultationStatuses.Cancelled
    ];

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/consultations").WithTags("Consultations");

        group.MapGet("/available-slots", GetAvailableSlots)
            .WithSummary("Returns currently available consultation appointment slots.");

        group.MapPost(string.Empty, CreateConsultation)
            .WithSummary("Creates a public consultation request from the contact page.");

        group.MapGet(string.Empty, ListConsultationRequests)
            .RequireAuthorization(AuthPolicies.IntakeRead)
            .WithSummary("Lists consultation requests for staff queue.");

        group.MapPost("/{consultationId:guid}/status", UpdateConsultationStatus)
            .RequireAuthorization(AuthPolicies.IntakeManage)
            .WithSummary("Updates consultation request status and assignment details.");
    }

    private static async Task<IResult> GetAvailableSlots(
        DateOnly? startDate,
        int? days,
        LawFirmDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var slotDays = Math.Clamp(days ?? 14, 1, 28);
        var utcNow = DateTimeOffset.UtcNow;
        var dateCursor = startDate ?? DateOnly.FromDateTime(utcNow.UtcDateTime.Date);
        var fromUtc = new DateTimeOffset(dateCursor.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var untilUtc = fromUtc.AddDays(slotDays + 1);

        var busyCalendarTimes = await dbContext.CalendarEntries
            .AsNoTracking()
            .Where(entry => entry.ScheduledAtUtc >= fromUtc && entry.ScheduledAtUtc <= untilUtc && !entry.IsCompleted)
            .Select(entry => entry.ScheduledAtUtc)
            .ToListAsync(cancellationToken);

        var reservedConsultationTimes = await dbContext.ConsultationRequests
            .AsNoTracking()
            .Where(request =>
                request.PreferredAtUtc >= fromUtc &&
                request.PreferredAtUtc <= untilUtc &&
                request.Status != ConsultationStatuses.Cancelled &&
                request.Status != ConsultationStatuses.Closed)
            .Select(request => request.PreferredAtUtc)
            .ToListAsync(cancellationToken);

        var slots = new List<ConsultationSlotResponse>();
        var busyTimes = busyCalendarTimes.Concat(reservedConsultationTimes).ToArray();

        for (var dayOffset = 0; dayOffset < slotDays; dayOffset++)
        {
            var day = fromUtc.AddDays(dayOffset);
            if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            for (var hour = 9; hour <= 16; hour++)
            {
                foreach (var minute in new[] { 0, 30 })
                {
                    var slotStart = new DateTimeOffset(day.Year, day.Month, day.Day, hour, minute, 0, TimeSpan.Zero);
                    if (slotStart <= utcNow.AddHours(2))
                    {
                        continue;
                    }

                    var slotEnd = slotStart.AddMinutes(30);
                    var overlapsBusy = busyTimes.Any(busy => Math.Abs((busy - slotStart).TotalMinutes) < 30);
                    if (overlapsBusy)
                    {
                        continue;
                    }

                    slots.Add(new ConsultationSlotResponse(slotStart, slotEnd));
                }
            }
        }

        return Results.Ok(slots.OrderBy(slot => slot.StartsAtUtc).Take(200).ToArray());
    }

    private static async Task<IResult> CreateConsultation(
        CreateConsultationRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validationErrors = await ValidateCreateRequestAsync(request, dbContext, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return httpContext.ValidationError("Consultation request is invalid.", validationErrors);
        }

        var consultation = new ConsultationRequest
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone?.Trim(),
            PracticeArea = request.PracticeArea.Trim(),
            Message = request.Message?.Trim(),
            PreferredAtUtc = request.PreferredAtUtc,
            TimeZone = request.TimeZone.Trim(),
            Status = ConsultationStatuses.New,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.ConsultationRequests.Add(consultation);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            httpContext,
            action: "consultation.request_created",
            objectType: "consultation_request",
            objectId: consultation.Id.ToString(),
            metadata: new { consultation.PracticeArea, consultation.PreferredAtUtc, consultation.Status },
            cancellationToken);

        return Results.Created(
            $"/api/consultations/{consultation.Id}",
            new ConsultationRequestResponse(
                consultation.Id,
                consultation.FullName,
                consultation.Email,
                consultation.Phone,
                consultation.PracticeArea,
                consultation.Message,
                consultation.PreferredAtUtc,
                consultation.TimeZone,
                consultation.Status,
                consultation.AssignedToActorId,
                consultation.AssignedToDisplayName,
                consultation.InternalNotes,
                consultation.CreatedAtUtc,
                consultation.UpdatedAtUtc));
    }

    private static async Task<IResult> ListConsultationRequests(
        string? status,
        LawFirmDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ConsultationRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(request => request.Status == status.Trim());
        }

        var items = await query
            .OrderBy(request => request.Status == ConsultationStatuses.New ? 0 : 1)
            .ThenBy(request => request.PreferredAtUtc)
            .Take(300)
            .Select(request => new ConsultationRequestResponse(
                request.Id,
                request.FullName,
                request.Email,
                request.Phone,
                request.PracticeArea,
                request.Message,
                request.PreferredAtUtc,
                request.TimeZone,
                request.Status,
                request.AssignedToActorId,
                request.AssignedToDisplayName,
                request.InternalNotes,
                request.CreatedAtUtc,
                request.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(items);
    }

    private static async Task<IResult> UpdateConsultationStatus(
        Guid consultationId,
        UpdateConsultationStatusRequest request,
        LawFirmDbContext dbContext,
        IAuditTrailService auditTrailService,
        ClaimsPrincipal user,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var consultation = await dbContext.ConsultationRequests.FirstOrDefaultAsync(item => item.Id == consultationId, cancellationToken);
        if (consultation is null)
        {
            return Results.NotFound(new ApiErrorResponse(
                httpContext.GetCorrelationId(),
                new ApiError("consultation_not_found", "Consultation request was not found.")));
        }

        var nextStatus = request.Status?.Trim() ?? consultation.Status;
        if (!AllowedStatuses.Contains(nextStatus))
        {
            return httpContext.ValidationError("Consultation status update is invalid.", new Dictionary<string, string[]>
            {
                ["status"] = ["Status must be one of: New, InReview, Contacted, Scheduled, Closed, Cancelled."]
            });
        }

        consultation.Status = nextStatus;
        consultation.AssignedToActorId = request.AssignedToActorId?.Trim();
        consultation.AssignedToDisplayName = request.AssignedToDisplayName?.Trim();
        consultation.InternalNotes = request.InternalNotes?.Trim();
        consultation.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var actorId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "unknown";
        await auditTrailService.RecordAsync(
            httpContext,
            action: "consultation.status_updated",
            objectType: "consultation_request",
            objectId: consultation.Id.ToString(),
            metadata: new { consultation.Status, consultation.AssignedToActorId, updatedBy = actorId },
            cancellationToken);

        return Results.Ok(new ConsultationRequestResponse(
            consultation.Id,
            consultation.FullName,
            consultation.Email,
            consultation.Phone,
            consultation.PracticeArea,
            consultation.Message,
            consultation.PreferredAtUtc,
            consultation.TimeZone,
            consultation.Status,
            consultation.AssignedToActorId,
            consultation.AssignedToDisplayName,
            consultation.InternalNotes,
            consultation.CreatedAtUtc,
            consultation.UpdatedAtUtc));
    }

    private static async Task<Dictionary<string, string[]>> ValidateCreateRequestAsync(
        CreateConsultationRequest request,
        LawFirmDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors["fullName"] = ["Full name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }

        if (string.IsNullOrWhiteSpace(request.PracticeArea))
        {
            errors["practiceArea"] = ["Practice area is required."];
        }

        if (string.IsNullOrWhiteSpace(request.TimeZone))
        {
            errors["timeZone"] = ["Time zone is required."];
        }

        if (request.PreferredAtUtc <= DateTimeOffset.UtcNow.AddHours(2))
        {
            errors["preferredAtUtc"] = ["Selected time must be at least two hours from now."];
        }

        var conflictsWithCalendar = await dbContext.CalendarEntries
            .AsNoTracking()
            .AnyAsync(
                entry =>
                    !entry.IsCompleted &&
                    entry.ScheduledAtUtc >= request.PreferredAtUtc.AddMinutes(-29) &&
                    entry.ScheduledAtUtc <= request.PreferredAtUtc.AddMinutes(29),
                cancellationToken);

        if (conflictsWithCalendar)
        {
            errors["preferredAtUtc"] = ["Selected time is no longer available. Please choose another slot."];
        }

        var conflictsWithConsultation = await dbContext.ConsultationRequests
            .AsNoTracking()
            .AnyAsync(
                entry =>
                    entry.Status != ConsultationStatuses.Cancelled &&
                    entry.Status != ConsultationStatuses.Closed &&
                    entry.PreferredAtUtc >= request.PreferredAtUtc.AddMinutes(-29) &&
                    entry.PreferredAtUtc <= request.PreferredAtUtc.AddMinutes(29),
                cancellationToken);

        if (conflictsWithConsultation)
        {
            errors["preferredAtUtc"] = ["Selected time is no longer available. Please choose another slot."];
        }

        return errors;
    }
}

public sealed record CreateConsultationRequest(
    string FullName,
    string Email,
    string? Phone,
    string PracticeArea,
    string? Message,
    DateTimeOffset PreferredAtUtc,
    string TimeZone);

public sealed record UpdateConsultationStatusRequest(
    string? Status,
    string? AssignedToActorId,
    string? AssignedToDisplayName,
    string? InternalNotes);

public sealed record ConsultationSlotResponse(
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);

public sealed record ConsultationRequestResponse(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    string PracticeArea,
    string? Message,
    DateTimeOffset PreferredAtUtc,
    string TimeZone,
    string Status,
    string? AssignedToActorId,
    string? AssignedToDisplayName,
    string? InternalNotes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
