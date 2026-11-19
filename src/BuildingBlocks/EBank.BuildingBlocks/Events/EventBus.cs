using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace EBank.BuildingBlocks.Events;

public sealed record IntegrationEventEnvelope(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    string Source,
    Guid? CustomerId,
    string PayloadJson
);

public interface IIntegrationEvent
{
    static abstract string EventType { get; }
    Guid? CustomerId { get; }
}

public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(Guid eventId, TEvent @event, CancellationToken ct = default);
}

public interface IEventBus
{
    Task PublishAsync<TEvent>(string sourceModule, TEvent @event, CancellationToken ct = default)
        where TEvent : IIntegrationEvent;
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}

public sealed class InboxMessage
{
    public Guid EventId { get; set; }
    public string ConsumerModule { get; set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; set; }
}

public sealed class InMemoryEventBus(IServiceProvider serviceProvider) : IEventBus
{
    public async Task PublishAsync<TEvent>(string sourceModule, TEvent @event, CancellationToken ct = default)
        where TEvent : IIntegrationEvent
    {
        var eventId = Guid.NewGuid();
        using var scope = serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(eventId, @event, ct);
        }
    }
}
