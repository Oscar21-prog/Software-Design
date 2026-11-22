using EBank.BuildingBlocks.Events;
using EBank.Crm.Contracts.Enums;

namespace EBank.Crm.Contracts.Events;

public sealed record AddressDto(
    AddressType Type,
    string Street,
    string City,
    string PostalCode,
    string Country
);

public sealed record RepresentativeDto(
    Guid PersonCustomerId,
    string Name,
    string PersonalCode,
    RepRole Role,
    DateOnly ValidFrom,
    DateOnly? ValidTo
);

public sealed record BeneficialOwnerDto(
    string Name,
    string? PersonalCode,
    string Country,
    decimal SharePercent
);

/// <summary>
/// Spec §7.10.4: Published by CRM when any profile, contact, language, or corporate structure change occurs.
/// Consumed by KYC (new SubjectSnapshot version and party updates), CBS, Cards.
/// </summary>
public sealed record CustomerProfileUpdatedEvent(
    Guid CustomerId,
    Guid UserId,
    long Version,
    CustomerType Type,
    string DisplayName,
    string? Email,
    string? Phone,
    Language Language,
    AddressDto? Address,
    IReadOnlyList<RepresentativeDto> Representatives,
    IReadOnlyList<BeneficialOwnerDto> Owners
) : IIntegrationEvent
{
    public static string EventType => "CUSTOMER_PROFILE_UPDATED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

