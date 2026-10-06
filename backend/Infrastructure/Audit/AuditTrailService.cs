using System.Security.Claims;
using System.Text.Json;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Errors;
using EnterpriseKnowledgeAssistant.Api.Infrastructure.Persistence;

namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Audit;

public interface IAuditTrailService
{
    Task RecordAsync(
        HttpContext context,
        string action,
        string objectType,
        string objectId,
        object metadata,
        CancellationToken cancellationToken);
}

public sealed class AuditTrailService(LawFirmDbContext dbContext) : IAuditTrailService
{
    public async Task RecordAsync(
        HttpContext context,
        string action,
        string objectType,
        string objectId,
        object metadata,
        CancellationToken cancellationToken)
    {
        var actorId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      context.User.FindFirstValue("sub") ??
                      "anonymous";
        var actorDisplayName = context.User.FindFirstValue("name") ??
                               context.User.FindFirstValue(ClaimTypes.Email) ??
                               "Anonymous";
        var correlationId = context.GetCorrelationId();

        dbContext.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            Action = action,
            ObjectType = objectType,
            ObjectId = objectId,
            ActorId = actorId,
            ActorDisplayName = actorDisplayName,
            CorrelationId = correlationId,
            MetadataJson = JsonSerializer.Serialize(metadata),
            OccurredAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
