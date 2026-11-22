using EBank.Kyc.Contracts.Enums;

namespace EBank.Kyc.Contracts.Providers;

public sealed record CompanyRegistryDetails(
    string RegistrationNumber,
    string LegalName,
    string RegistrationCountry,
    string LegalForm,
    string RegisteredAddress,
    DateOnly RegistrationDate,
    bool IsActive,
    IReadOnlyList<RegistryRepresentativeRecord> Representatives,
    IReadOnlyList<RegistryShareholderRecord> Shareholders
);

public sealed record RegistryRepresentativeRecord(
    string Name,
    string PersonalCode,
    string Role,
    DateOnly ValidFrom,
    DateOnly? ValidTo
);

public sealed record RegistryShareholderRecord(
    string Name,
    string? Code,
    decimal SharePercent
);

/// <summary>
/// Spec §6.10.5: Company Registry provider adapter interface (mocked in implementation).
/// Used during corporate KYC and legal entity verification (KYC-08).
/// </summary>
public interface ICompanyRegistryAdapter
{
    Task<CompanyRegistryDetails?> LookupCompanyAsync(string registrationNumber, string countryCode, CancellationToken ct = default);
}

