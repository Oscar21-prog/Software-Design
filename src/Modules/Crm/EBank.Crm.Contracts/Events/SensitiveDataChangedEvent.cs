using EBank.BuildingBlocks.Events;
using EBank.Crm.Contracts.Enums;

namespace EBank.Crm.Contracts.Events;

/// <summary>
/// Spec §7.10.4: Published by CRM when a sensitive field changes (C-BR-07).
/// Consumed by KYC (opens a CHANGE_REVIEW case K-BR-12), Audit.
/// </summary>
public sealed record SensitiveDataChangedEvent(
    Guid CustomerId,
    Guid UserId,
    long Version,
    IReadOnlyList<SensitiveFieldGroup> FieldGroups,
    DateTimeOffset ChangedAt
) : IIntegrationEvent
{
    public static string EventType => "SENSITIVE_DATA_CHANGED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

