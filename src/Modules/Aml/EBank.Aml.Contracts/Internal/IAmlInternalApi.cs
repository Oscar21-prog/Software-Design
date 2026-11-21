using EBank.Aml.Contracts.Enums;

namespace EBank.Aml.Contracts.Internal;

public interface IAmlInternalApi
{
    Task<ScreeningResponse> ScreenAsync(ScreeningRequest request, CancellationToken ct = default);
}

public sealed record ScreeningRequest(
    Guid ReferenceId,
    ScreeningSource Source,
    Guid CustomerId,
    Guid? AccountId,
    Guid InitiatedByUserId,
    string Direction,
    string Amount,
    string Currency,
    string AmountEur,
    string? CounterpartyName,
    string? CounterpartyIban,
    string CounterpartyCountry,
    DateTimeOffset CreatedAt
);

public sealed record ScreeningResponse(
    ScreeningDecision Decision,
    Guid? AlertId,
    int RiskScore
);