namespace EnterpriseKnowledgeAssistant.Api.Infrastructure.Outbox;

/// <summary>
/// Polls the outbox and hands events to provider adapters. Not registered until <c>outbox_events</c> exists.
/// Once multi-firm lands, each batch must run inside one firm's scope and clear it afterwards.
/// </summary>
public sealed class OutboxDispatcher(TimeProvider timeProvider) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await DispatchBatchAsync(stoppingToken);
        }
    }

    // Lease pending rows with FOR UPDATE SKIP LOCKED (validated as R3 in db/design), dispatch, then mark done.
    private static Task DispatchBatchAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
