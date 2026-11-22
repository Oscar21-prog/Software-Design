using EBank.BuildingBlocks.Events;
using EBank.Kyc.Contracts.Enums;

namespace EBank.Kyc.Contracts.Events;

/// <summary>
/// Spec §6.10.3: Published by KYC when a case is approved (four-eyes completed or low-risk auto approved).
/// Consumed by CRM (activates customer or updates replica), Notification, Audit.
/// </summary>
public sealed record KycApprovedEvent(
    Guid CaseId,
    Guid CustomerId,
    Guid UserId,
    CaseType CaseType,
    RiskLevel RiskLevel,
    DateOnly NextReviewDate
) : IIntegrationEvent
{
    public static string EventType => "KYC_APPROVED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

