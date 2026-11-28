using System.Text.Json.Serialization;

namespace EBank.Cbs.Contracts.Enums;

// Enumerations from spec §8.8 (CBS data model) and §8.10.3 (event payloads).

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType
{
    CURRENT,
    SAVINGS
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountStatus
{
    OPEN,
    FROZEN,
    CLOSED
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HoldStatus
{
    ACTIVE,
    RELEASED,
    CONSUMED
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HoldReferenceType
{
    PAYMENT,
    CARD_AUTHORIZATION
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MandateRole
{
    ACCOUNTANT,
    DIRECTOR,
    ADMIN
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MandatePermission
{
    VIEW,
    CREATE_PAYMENTS,
    APPROVE_PAYMENTS
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentType
{
    INTERNAL,
    SEPA,
    SWIFT
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    DRAFT,
    PENDING_APPROVAL,
    APPROVED,
    REJECTED,
    EXPIRED,
    EXECUTED,
    FAILED
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionType
{
    INTERNAL,
    SEPA,
    SWIFT,
    CARD,
    CARD_REPAYMENT,
    LOAN
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionStatus
{
    PENDING,
    COMPLETED,
    FAILED,
    REJECTED,
    ON_HOLD
}

/// <summary>TRANSACTION_COMPLETED.direction (§8.10.3, §11.8.1).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionDirection
{
    INCOMING,
    OUTGOING
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OtpPurpose
{
    LOGIN,
    PAYMENT,
    PAYMENT_APPROVAL,
    MANDATE_CHANGE,
    CARD_ACTIVATION,
    CARD_REPAYMENT
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OtpChallengeStatus
{
    PENDING,
    VERIFIED,
    EXPIRED,
    FAILED
}

/// <summary>PAYMENT_REJECTED.reason (§8.10.3).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentRejectedReason
{
    REJECTED,
    EXPIRED
}

/// <summary>TRANSACTION_FAILED.reason (§8.10.3).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionFailedReason
{
    SCA_FAILED,
    GATEWAY_ERROR
}
