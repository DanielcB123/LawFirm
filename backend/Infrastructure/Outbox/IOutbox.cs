namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Outbox;

/// <summary>
/// Payload carries identifiers only, never legal narratives or client names (data dictionary, <c>outbox_events</c>).
/// </summary>
public sealed record OutboxEvent(string EventType, string PayloadJson, DateTimeOffset OccurredAtUtc);

/// <summary>
/// Adds an event to the current unit of work so it commits in the same transaction as the domain change (gap G43).
/// Not implemented or registered until the <c>outbox_events</c> table exists.
/// </summary>
public interface IOutbox
{
    Task EnqueueAsync(OutboxEvent outboxEvent, CancellationToken cancellationToken = default);
}
