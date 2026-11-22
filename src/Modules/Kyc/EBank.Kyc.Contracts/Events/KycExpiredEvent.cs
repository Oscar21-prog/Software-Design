using EBank.BuildingBlocks.Events;
using EBank.Kyc.Contracts.Enums;

namespace EBank.Kyc.Contracts.Events;

/// <summary>
/// Spec §6.10.3: Published by KYC when a case expires without completed verification (K-BR-06).
/// Consumed by CRM (transitions ACTIVE -> RESTRICTED or ONBOARDING -> PROSPECT), Notification, Audit.
/// </summary>
public sealed record KycExpiredEvent(
    Guid CaseId,
    Guid CustomerId,
    Guid UserId,
    CaseType CaseType,
    DateTimeOffset ExpiredAt
) : IIntegrationEvent
{
    public static string EventType => "KYC_EXPIRED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

