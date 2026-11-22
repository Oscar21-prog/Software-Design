using EBank.BuildingBlocks.Events;
using EBank.Crm.Contracts.Enums;

namespace EBank.Crm.Contracts.Events;

/// <summary>
/// Spec §7.10.4: Published by CRM when registration step 2 completes or a company is added.
/// Consumed by KYC (opens onboarding case), Notification (welcome message), Audit.
/// </summary>
public sealed record CustomerRegisteredEvent(
    Guid CustomerId,
    Guid UserId,
    CustomerType Type,
    string DisplayName,
    string Email,
    string Phone,
    Language Language,
    string? RegistrationNumber = null,
    string? RegistrationCountry = null,
    Guid? RepresentativeCustomerId = null
) : IIntegrationEvent
{
    public static string EventType => "CUSTOMER_REGISTERED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

