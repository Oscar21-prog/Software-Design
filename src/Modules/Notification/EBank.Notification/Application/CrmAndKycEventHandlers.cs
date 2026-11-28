using EBank.BuildingBlocks.Events;
using EBank.BuildingBlocks.Time;
using EBank.Crm.Contracts.Enums;
using EBank.Crm.Contracts.Events;
using EBank.Kyc.Contracts.Events;
using EBank.Notification.Domain;

namespace EBank.Notification.Application;

public sealed class CustomerRegisteredHandler(INotificationRepository repository, IClock clock)
    : IIntegrationEventHandler<CustomerRegisteredEvent>
{
    public async Task HandleAsync(Guid eventId, CustomerRegisteredEvent @event, CancellationToken ct = default)
    {
        var lang = @event.Language == Language.LT ? NotificationLanguage.LT : NotificationLanguage.EN;
        var contact = new RecipientContact
        {
            UserId = @event.UserId,
            CustomerId = @event.CustomerId,
            Email = @event.Email,
            Phone = @event.Phone,
            MarketingConsent = false,
            UpdatedAt = clock.UtcNow
        };
        await repository.UpsertContactAndSettingsAsync(contact, lang, ct);
    }
}

public sealed class CustomerProfileUpdatedHandler(INotificationRepository repository, IClock clock)
    : IIntegrationEventHandler<CustomerProfileUpdatedEvent>
{
    public async Task HandleAsync(Guid eventId, CustomerProfileUpdatedEvent @event, CancellationToken ct = default)
    {
        var contact = new RecipientContact
        {
            UserId = @event.UserId,
            CustomerId = @event.CustomerId,
            Email = @event.Email,
            Phone = @event.Phone,
            UpdatedAt = clock.UtcNow
        };
        await repository.UpsertContactAndSettingsAsync(contact, initialLanguage: null, ct);
    }
}

public sealed class ConsentChangedHandler(INotificationRepository repository, IClock clock)
    : IIntegrationEventHandler<ConsentChangedEvent>
{
    public async Task HandleAsync(Guid eventId, ConsentChangedEvent @event, CancellationToken ct = default)
    {
        if (@event.Purpose == ConsentPurpose.MARKETING)
        {
            await repository.UpdateMarketingConsentAsync(@event.UserId, @event.Granted, clock.UtcNow, ct);
        }
    }
}

public sealed class KycApprovedHandler(NotificationDispatcher dispatcher)
    : IIntegrationEventHandler<KycApprovedEvent>
{
    public Task HandleAsync(Guid eventId, KycApprovedEvent @event, CancellationToken ct = default)
    {
        return dispatcher.DispatchAsync(new DispatchNotificationCommand(
            eventId,
            KycApprovedEvent.EventType,
            Variant: null,
            @event.UserId,
            new Dictionary<string, object?>()
        ), ct);
    }
}

public sealed class KycInfoRequestedHandler(NotificationDispatcher dispatcher)
    : IIntegrationEventHandler<KycInfoRequestedEvent>
{
    public Task HandleAsync(Guid eventId, KycInfoRequestedEvent @event, CancellationToken ct = default)
    {
        return dispatcher.DispatchAsync(new DispatchNotificationCommand(
            eventId,
            KycInfoRequestedEvent.EventType,
            Variant: null,
            @event.UserId,
            new Dictionary<string, object?> { ["dueDate"] = @event.DueAt }
        ), ct);
    }
}
