using EBank.Kyc.Contracts.Enums;

namespace EBank.Kyc.Contracts.Providers;

public sealed record IdvVerificationRequest(
    Guid ReferenceId,
    DocType DocumentType,
    string DocumentFileName,
    string DocumentImageBase64,
    string SelfieImageBase64
);

public sealed record IdvVerificationResponse(
    string VerificationId,
    CheckResult DocumentResult,
    CheckResult LivenessResult,
    DateOnly? DocumentExpiresOn,
    string? PersonalCode,
    string? FirstName,
    string? LastName
);

/// <summary>
/// Spec §6.10.5: Identity Verification (IDV) provider adapter interface (mocked in implementation).
/// </summary>
public interface IIdvProviderAdapter
{
    Task<IdvVerificationResponse> VerifyIdentityAsync(IdvVerificationRequest request, CancellationToken ct = default);
}

