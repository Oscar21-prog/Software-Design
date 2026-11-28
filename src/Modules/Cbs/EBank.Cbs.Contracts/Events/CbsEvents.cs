using EBank.BuildingBlocks.Events;
using EBank.Cbs.Contracts.Enums;

namespace EBank.Cbs.Contracts.Events;

// Events published by CBS (spec §8.10.3). UserId is the user to notify (the customer's user unless stated).
// Payload fields are exactly the ones listed in the spec. When the spec payload has no customerId,
// the envelope key is null.

public sealed record AccountOpenedEvent(
    Guid UserId,
    Guid CustomerId,
    Guid AccountId,
    string Iban,
    string Currency,
    AccountType Type
) : IIntegrationEvent
{
    public static string EventType => "ACCOUNT_OPENED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

public sealed record AccountStatusChangedEvent(
    Guid UserId,
    Guid CustomerId,
    Guid AccountId,
    string Iban,
    AccountStatus OldStatus,
    AccountStatus NewStatus
) : IIntegrationEvent
{
    public static string EventType => "ACCOUNT_STATUS_CHANGED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

/// <summary>Mandate saved or removed. UserId is the affected user.</summary>
public sealed record MandateChangedEvent(
    Guid UserId,
    Guid AccountId,
    string Iban,
    MandateRole Role,
    IReadOnlyList<MandatePermission> Permissions
) : IIntegrationEvent
{
    public static string EventType => "MANDATE_CHANGED";
    Guid? IIntegrationEvent.CustomerId => null;
}

/// <summary>Dual-signature payment created: one event per approver (UserId = approver).</summary>
public sealed record PaymentPendingApprovalEvent(
    Guid UserId,
    Guid PaymentId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    string BeneficiaryName
) : IIntegrationEvent
{
    public static string EventType => "PAYMENT_PENDING_APPROVAL";
    Guid? IIntegrationEvent.CustomerId => null;
}

/// <summary>Payment rejected or expired. UserId is the creator.</summary>
public sealed record PaymentRejectedEvent(
    Guid UserId,
    Guid PaymentId,
    decimal Amount,
    string Currency,
    string BeneficiaryName,
    PaymentRejectedReason Reason
) : IIntegrationEvent
{
    public static string EventType => "PAYMENT_REJECTED";
    Guid? IIntegrationEvent.CustomerId => null;
}

/// <summary>INTERNAL, SEPA, SWIFT or LOAN transaction settled. Not published for CARD (F4).</summary>
public sealed record TransactionCompletedEvent(
    Guid UserId,
    Guid CustomerId,
    Guid TransactionId,
    Guid AccountId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    decimal AmountEur,
    TransactionDirection Direction,
    string? CounterpartyName,
    string? CounterpartyCountry
) : IIntegrationEvent
{
    public static string EventType => "TRANSACTION_COMPLETED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

/// <summary>Code failure or gateway error.</summary>
public sealed record TransactionFailedEvent(
    Guid UserId,
    Guid PaymentId,
    Guid TransactionId,
    decimal Amount,
    string Currency,
    string BeneficiaryName,
    TransactionFailedReason Reason
) : IIntegrationEvent
{
    public static string EventType => "TRANSACTION_FAILED";
    Guid? IIntegrationEvent.CustomerId => null;
}

/// <summary>Fifth failed AML screening call (BR-14). The transaction stays ON_HOLD.</summary>
public sealed record AmlScreeningFailedEvent(
    Guid TransactionId,
    Guid CustomerId
) : IIntegrationEvent
{
    public static string EventType => "AML_SCREENING_FAILED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

/// <summary>One-time code created (F6, BR-04). Notification sends it by SMS only.</summary>
public sealed record OtpRequestedEvent(
    Guid UserId,
    string Code,
    OtpPurpose Purpose
) : IIntegrationEvent
{
    public static string EventType => "OTP_REQUESTED";
    Guid? IIntegrationEvent.CustomerId => null;
}

public sealed record NewDeviceLoginEvent(
    Guid UserId,
    string DeviceName,
    string? City
) : IIntegrationEvent
{
    public static string EventType => "NEW_DEVICE_LOGIN";
    Guid? IIntegrationEvent.CustomerId => null;
}
