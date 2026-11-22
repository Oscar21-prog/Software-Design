using EBank.BuildingBlocks.Events;
using EBank.Crm.Contracts.Enums;

namespace EBank.Crm.Contracts.Events;

/// <summary>
/// Spec §7.10.4: Published by CRM when a GDPR / marketing / communications consent changes.
/// Consumed by Notification, Audit.
/// </summary>
public sealed record ConsentChangedEvent(
    Guid CustomerId,
    Guid UserId,
    ConsentPurpose Purpose,
    bool Granted,
    DateTimeOffset ChangedAt
) : IIntegrationEvent
{
    public static string EventType => "CONSENT_CHANGED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

