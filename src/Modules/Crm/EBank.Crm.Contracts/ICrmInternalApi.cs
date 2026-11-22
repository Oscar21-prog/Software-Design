using EBank.Crm.Contracts.Enums;

namespace EBank.Crm.Contracts;

/// <summary>
/// Spec §7.10.3: Synchronous internal interface exposed by CRM for CBS, Cards, and other modules.
/// Mapped from the spec's /internal/crm/... REST routes (Deviation DV-2).
/// </summary>
public interface ICrmInternalApi
{
    /// <summary>
    /// POST /internal/crm/users/authenticate (Spec §7.10.3)
    /// Used by CBS sign-in (F6 step 1). Verifies credentials against hashed password.
    /// </summary>
    Task<AuthenticateUserResponse> AuthenticateUserAsync(AuthenticateUserRequest request, CancellationToken ct = default);

    /// <summary>
    /// GET /internal/crm/customers/{customerId} (Spec §7.10.3)
    /// Used by Cards authorization and other modules for on-demand customer status verification.
    /// </summary>
    Task<CustomerSummaryResponse?> GetCustomerByIdAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// GET /internal/crm/companies/{customerId}/representatives (Spec §7.10.3)
    /// Used by CBS mandate assignment (S7 user search) to list legal representatives.
    /// </summary>
    Task<IReadOnlyList<CompanyRepresentativeResponse>> GetCompanyRepresentativesAsync(Guid companyCustomerId, CancellationToken ct = default);
}

public sealed record AuthenticateUserRequest(
    string Email,
    string Password
);

public sealed record AuthenticateUserResponse(
    Guid UserId,
    UserStatus UserStatus,
    Guid CustomerId,
    CustomerStatus CustomerStatus
);

public sealed record CustomerSummaryResponse(
    Guid Id,
    CustomerType Type,
    CustomerStatus Status,
    KycStatus KycStatus,
    Guid UserId,
    string DisplayName
);

public sealed record CompanyRepresentativeResponse(
    Guid PersonCustomerId,
    Guid UserId,
    string Name,
    RepRole Role,
    DateOnly ValidFrom,
    DateOnly? ValidTo
);

