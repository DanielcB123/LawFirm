namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public static class ConsultationStatuses
{
    public const string New = "New";
    public const string Contacted = "Contacted";
    public const string Scheduled = "Scheduled";
    public const string InReview = "InReview";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";
}

public sealed class ConsultationRequest
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PracticeArea { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTimeOffset PreferredAtUtc { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Status { get; set; } = ConsultationStatuses.New;
    public string? AssignedToActorId { get; set; }
    public string? AssignedToDisplayName { get; set; }
    public string? InternalNotes { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
