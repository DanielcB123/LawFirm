namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

public sealed class AuditEvent
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ObjectType { get; set; } = string.Empty;
    public string ObjectId { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string ActorDisplayName { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string MetadataJson { get; set; } = "{}";
    public DateTimeOffset OccurredAtUtc { get; set; }
}
