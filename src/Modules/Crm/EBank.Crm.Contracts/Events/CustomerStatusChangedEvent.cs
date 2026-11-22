using EBank.BuildingBlocks.Events;
using EBank.Crm.Contracts.Enums;

namespace EBank.Crm.Contracts.Events;

/// <summary>
/// Spec §7.10.4: Published by CRM on every status transition other than initial activation (F4).
/// Consumed by KYC, CBS (blocks/unblocks operations), Cards, Loans, Notification, Audit.
/// </summary>
public sealed record CustomerStatusChangedEvent(
    Guid CustomerId,
    Guid UserId,
    CustomerType Type,
    CustomerStatus OldStatus,
    CustomerStatus NewStatus,
    KycStatus KycStatus,
    StatusReason ReasonCode,
    DateTimeOffset ChangedAt
) : IIntegrationEvent
{
    public static string EventType => "CUSTOMER_STATUS_CHANGED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

