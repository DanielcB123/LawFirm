namespace EnterpriseKnowledgeAssistant.Api.Features.Consultations;

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
