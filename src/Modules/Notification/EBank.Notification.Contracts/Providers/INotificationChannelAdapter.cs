namespace EBank.Notification.Contracts.Providers;

public enum NotificationChannel
{
    PUSH,
    EMAIL,
    SMS
}

public enum DeliveryStatus
{
    DELIVERED,
    FAILED
}

/// <summary>
/// Spec §12.8.2: Provider interface for Push, Email, and SMS adapters (mocked in implementation).
/// </summary>
public interface INotificationChannelAdapter
{
    NotificationChannel Channel { get; }
    Task<DeliveryStatus> SendAsync(string recipient, string? title, string body, CancellationToken ct = default);
}
