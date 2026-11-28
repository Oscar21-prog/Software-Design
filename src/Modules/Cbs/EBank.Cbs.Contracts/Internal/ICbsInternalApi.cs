using EBank.Cbs.Contracts.Enums;

namespace EBank.Cbs.Contracts.Internal;

/// <summary>
/// Spec §8.10.2: synchronous internal interface exposed by CBS to Cards, Loans, CRM and Accounting.
/// Each method mirrors one /internal/cbs/... route (decision D-003). Errors are thrown as
/// <c>EBankException</c> with the spec code from §8.10.4.
/// Money is <c>decimal</c> with 2 decimals; it is sent as a string only in JSON.
/// </summary>
public interface ICbsInternalApi
{
    /// <summary>
    /// GET /internal/cbs/accounts/{id}: status check for Cards and Loans.
    /// Returns null when the account does not exist (ACCOUNT_NOT_FOUND).
    /// </summary>
    Task<InternalAccountResponse?> GetAccountAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// GET /internal/cbs/accounts/by-iban/{iban}: IBAN lookup for CRM.
    /// Returns null when no account has this IBAN.
    /// </summary>
    Task<AccountByIbanResponse?> GetAccountByIbanAsync(string iban, CancellationToken ct = default);

    /// <summary>GET /internal/cbs/customers/{customerId}/accounts: products for Customer 360 (CRM-06).</summary>
    Task<IReadOnlyList<CustomerAccountResponse>> GetCustomerAccountsAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>
    /// POST /internal/cbs/sca/validate: checks a single-use scaToken for Cards (F6, BR-04).
    /// Throws SCA_CODE_INVALID (401) if the token is unknown, expired, already used,
    /// or bound to another purpose or reference.
    /// </summary>
    Task<ValidateScaTokenResponse> ValidateScaTokenAsync(ValidateScaTokenRequest request, CancellationToken ct = default);

    /// <summary>
    /// POST /internal/cbs/holds: reserves funds (F4). Creates an ACTIVE hold and a PENDING transaction.
    /// Errors: ACCOUNT_NOT_FOUND, ACCOUNT_NOT_OPEN, CUSTOMER_RESTRICTED, INSUFFICIENT_FUNDS, DAILY_LIMIT_EXCEEDED.
    /// A repeated <paramref name="idempotencyKey"/> returns the first result (BR-12).
    /// </summary>
    Task<CreateHoldResponse> CreateHoldAsync(CreateHoldRequest request, string idempotencyKey, CancellationToken ct = default);

    /// <summary>
    /// POST /internal/cbs/holds/{id}/capture: consumes the hold, deducts the balance and sets the
    /// transaction to COMPLETED. No TRANSACTION_COMPLETED is published for card transactions (F4).
    /// Errors: INVALID_STATE (hold not ACTIVE), VALIDATION_ERROR (finalAmount above the reserved amount).
    /// </summary>
    Task<TransactionResponse> CaptureHoldAsync(Guid holdId, CaptureHoldRequest request, CancellationToken ct = default);

    /// <summary>
    /// POST /internal/cbs/holds/{id}/cancel: releases the hold and sets the transaction to FAILED (F4, alternative 4B).
    /// </summary>
    Task CancelHoldAsync(Guid holdId, CancellationToken ct = default);

    /// <summary>
    /// POST /internal/cbs/postings/credit: loan payout (type LOAN). COMPLETED at once, publishes TRANSACTION_COMPLETED.
    /// Errors: ACCOUNT_NOT_FOUND, ACCOUNT_NOT_OPEN (F4, alternative 4C).
    /// </summary>
    Task<TransactionResponse> CreditPostingAsync(PostingRequest request, string idempotencyKey, CancellationToken ct = default);

    /// <summary>
    /// POST /internal/cbs/postings/debit: loan installment (LOAN) or credit-card repayment (CARD_REPAYMENT).
    /// Errors: ACCOUNT_NOT_FOUND, ACCOUNT_NOT_OPEN, INSUFFICIENT_FUNDS (F4, alternative 4A).
    /// </summary>
    Task<TransactionResponse> DebitPostingAsync(PostingRequest request, string idempotencyKey, CancellationToken ct = default);

    /// <summary>
    /// GET /internal/cbs/transactions: Accounting nightly batch. Filters on completedAt in [From, To).
    /// Each item carries fee and fxRate from the payment that produced it.
    /// </summary>
    Task<IReadOnlyList<TransactionResponse>> GetTransactionsAsync(TransactionQuery query, CancellationToken ct = default);
}

public sealed record InternalAccountResponse(
    Guid Id,
    Guid CustomerId,
    string Currency,
    AccountStatus Status,
    decimal Balance,
    decimal AvailableBalance
);

public sealed record AccountByIbanResponse(
    Guid AccountId,
    Guid CustomerId,
    AccountStatus Status
);

public sealed record CustomerAccountResponse(
    Guid Id,
    string Iban,
    AccountType Type,
    string Currency,
    AccountStatus Status,
    decimal Balance,
    decimal AvailableBalance
);

public sealed record ValidateScaTokenRequest(
    string ScaToken,
    OtpPurpose Purpose,
    Guid ReferenceId
);

public sealed record ValidateScaTokenResponse(
    bool Valid
);

/// <param name="ExpiresAt">Set only for card holds: 7 days from now (CD-BR-06, §8.8).</param>
public sealed record CreateHoldRequest(
    Guid AccountId,
    decimal Amount,
    string Currency,
    HoldReferenceType ReferenceType,
    Guid ReferenceId,
    DateTimeOffset? ExpiresAt
);

public sealed record CreateHoldResponse(
    Guid HoldId,
    Guid TransactionId
);

/// <param name="FinalAmount">Optional, never above the reserved amount. Null captures the full hold.</param>
public sealed record CaptureHoldRequest(
    decimal? FinalAmount
);

/// <param name="Type">LOAN for credits; LOAN or CARD_REPAYMENT for debits.</param>
/// <param name="ReferenceId">Loan id or card repayment id.</param>
public sealed record PostingRequest(
    Guid AccountId,
    decimal Amount,
    string Currency,
    TransactionType Type,
    Guid ReferenceId
);

/// <param name="From">UTC instant, inclusive, compared with completedAt.</param>
/// <param name="To">UTC instant, exclusive, compared with completedAt.</param>
public sealed record TransactionQuery(
    DateTimeOffset From,
    DateTimeOffset To,
    TransactionStatus? Status
);

/// <summary>
/// Transaction as described in §8.8 and BR-11: Amount is negative for a debit and positive for a credit.
/// PaymentId is null for postings and holds from Cards and Loans; ReferenceId holds their id.
/// Fee and FxRate come from the payment (null when there is no payment).
/// </summary>
public sealed record TransactionResponse(
    Guid Id,
    Guid AccountId,
    TransactionType Type,
    TransactionStatus Status,
    decimal Amount,
    string Currency,
    Guid? ToAccountId,
    Guid? PaymentId,
    Guid? ReferenceId,
    string? CounterpartyName,
    string? CounterpartyIban,
    string? CounterpartyCountry,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    decimal? Fee,
    decimal? FxRate
);
