using EBank.BuildingBlocks.Events;
using EBank.Kyc.Contracts.Enums;

namespace EBank.Kyc.Contracts.Events;

/// <summary>
/// Spec §6.10.3: Published by KYC when IDV fails, an officer requests additional documentation,
/// or a reminder is due. Consumed by CRM, Notification (NTF-01), Audit.
/// </summary>
public sealed record KycInfoRequestedEvent(
    Guid CaseId,
    Guid CustomerId,
    Guid UserId,
    Guid RequestId,
    IReadOnlyList<DocType> DocumentTypes,
    DateTimeOffset DueAt,
    bool Reminder
) : IIntegrationEvent
{
    public static string EventType => "KYC_INFO_REQUESTED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

