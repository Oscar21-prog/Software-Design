using EBank.BuildingBlocks.Events;

namespace EBank.Accounting.Contracts.Events;

/// <summary>
/// Spec §10.8.3: Published by Accounting when end-of-day batch reconciliation detects a mismatch.
/// Consumed by Accounting back-office and Audit log.
/// </summary>
public sealed record ReconciliationDiscrepancyEvent(
    Guid DiscrepancyId,
    DateOnly ReconciliationDate,
    string SourceModule,
    Guid? ExternalTransactionId,
    string ExpectedAmount,
    string ActualAmount,
    string Reason,
    DateTimeOffset DetectedAt
) : IIntegrationEvent
{
    public static string EventType => "RECONCILIATION_DISCREPANCY";
    public Guid? CustomerId => null;
}
