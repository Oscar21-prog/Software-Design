using EBank.BuildingBlocks.Events;
using EBank.Crm.Contracts.Enums;

namespace EBank.Crm.Contracts.Events;

/// <summary>
/// Spec §7.10.4: Published by CRM on first activation of a customer.
/// Consumed by CBS (initial account setup readiness), Notification, Audit.
/// </summary>
public sealed record CustomerActivatedEvent(
    Guid CustomerId,
    Guid UserId,
    CustomerType Type,
    KycStatus KycStatus,
    RiskLevel RiskLevel,
    DateTimeOffset ActivatedAt
) : IIntegrationEvent
{
    public static string EventType => "CUSTOMER_ACTIVATED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

