namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Messaging;

public sealed record EmailMessage(string To, string Subject, string TextBody);

/// <summary>
/// Outbound email adapter, called from the outbox dispatcher rather than inside a request transaction (gap G43).
/// Messages to clients must not contain confidential matter content. No provider is chosen or registered yet.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
