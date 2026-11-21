using EBank.Aml.Contracts.Enums;
using EBank.BuildingBlocks.Events;

namespace EBank.Aml.Contracts.Events;

public sealed record ScreeningRequestedEvent(
    Guid ScreeningId,
    Guid CaseId,
    Guid CustomerId,
    string PartyRole,
    string SubjectType,
    string Name,
    DateOnly? BirthDate,
    string? PersonalCode,
    string? RegistrationNumber,
    string CountryCode
) : IIntegrationEvent
{
    public static string EventType => "SCREENING_REQUESTED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

public sealed record WatchlistMatchDetail(
    WatchlistType ListType,
    string EntryName,
    WatchlistMatchType MatchType
);

public sealed record ScreeningCompletedEvent(
    Guid ScreeningId,
    Guid CustomerId,
    ScreeningResult Result,
    IReadOnlyList<WatchlistMatchDetail> MatchDetails
) : IIntegrationEvent
{
    public static string EventType => "SCREENING_COMPLETED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

public sealed record TransactionReleasedEvent(
    Guid TransactionId,
    Guid CustomerId,
    Guid CaseId
) : IIntegrationEvent
{
    public static string EventType => "TRANSACTION_RELEASED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}

public sealed record TransactionRejectedEvent(
    Guid TransactionId,
    Guid CustomerId,
    Guid UserId,
    Guid CaseId
) : IIntegrationEvent
{
    public static string EventType => "TRANSACTION_REJECTED";
    Guid? IIntegrationEvent.CustomerId => CustomerId;
}