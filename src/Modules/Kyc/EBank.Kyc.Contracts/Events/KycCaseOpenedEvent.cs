using EBank.BuildingBlocks.Events;
using EBank.Kyc.Contracts.Enums;

namespace EBank.Kyc.Contracts.Events;

/// <summary>
/// Spec §6.10.3: Published by KYC when a verification case is opened.
/// Consumed by CRM (transitions PROSPECT -> ONBOARDING, replica IN_PROGRESS), Notification, Audit.
/// </summary>
public sealed record KycCaseOpenedEvent(
    Guid CaseId,
    Guid CustomerId,
    Guid UserId,
    CaseType CaseType,
    DateOnly? DueDate = null
) : IIntegrationEvent
{
    public static string EventType => "KYC_CASE_OPENED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

