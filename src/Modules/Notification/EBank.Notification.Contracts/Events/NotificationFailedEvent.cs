using EBank.BuildingBlocks.Events;

namespace EBank.Notification.Contracts.Events;

/// <summary>
/// Spec §12.8.3: Published by Notification when delivery fails after retries.
/// Consumed by Audit log.
/// </summary>
public sealed record NotificationFailedEvent(
    Guid NotificationId,
    Guid OriginalEventId,
    string OriginalEventType,
    Guid UserId,
    string Channel,
    string FailureReason,
    DateTimeOffset FailedAt
) : IIntegrationEvent
{
    public static string EventType => "NOTIFICATION_FAILED";
    public Guid? CustomerId => UserId;
}
