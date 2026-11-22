using EBank.BuildingBlocks.Events;
using EBank.Kyc.Contracts.Enums;

namespace EBank.Kyc.Contracts.Events;

/// <summary>
/// Spec §6.10.3: Published by KYC when a case is rejected.
/// The rejection reason is never revealed to the customer (K-BR-08). Consumed by CRM, Notification, Audit.
/// </summary>
public sealed record KycRejectedEvent(
    Guid CaseId,
    Guid CustomerId,
    Guid UserId,
    CaseType CaseType,
    RejectReason ReasonCode
) : IIntegrationEvent
{
    public static string EventType => "KYC_REJECTED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

