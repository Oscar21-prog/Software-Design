using EBank.BuildingBlocks.Events;
using EBank.Kyc.Contracts.Enums;

namespace EBank.Kyc.Contracts.Events;

/// <summary>
/// Spec §6.10.3: Published by KYC when screening of a party is needed (K-BR-04).
/// Consumed by AML (AML-14) to run sanctions and PEP list checks.
/// </summary>
public sealed record ScreeningRequestedEvent(
    Guid ScreeningId,
    Guid CaseId,
    Guid CustomerId,
    PartyRole PartyRole,
    SubjectType SubjectType,
    string Name,
    DateOnly? BirthDate,
    string PersonalCodeOrRegistrationNumber,
    string CountryCode
) : IIntegrationEvent
{
    public static string EventType => "SCREENING_REQUESTED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

