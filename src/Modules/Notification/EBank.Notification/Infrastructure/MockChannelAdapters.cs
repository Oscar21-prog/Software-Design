using EBank.Notification.Contracts.Providers;
using Microsoft.Extensions.Logging;

namespace EBank.Notification.Infrastructure;

/// <summary>
/// Spec §12.8.2: Mock channel adapters for Push, Email, and SMS.
/// Supports deterministic failure triggers (e.g., recipient containing "fail") for testing F1 A1/A2.
/// </summary>
public abstract class BaseMockChannelAdapter(ILogger logger) : INotificationChannelAdapter
{
    public abstract NotificationChannel Channel { get; }

    public Task<DeliveryStatus> SendAsync(string recipient, string? title, string body, CancellationToken ct = default)
    {
        if (recipient.Contains("fail", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("[MOCK-{Channel}] Simulated delivery failure to {Recipient}", Channel, recipient);
            return Task.FromResult(DeliveryStatus.FAILED);
        }

        logger.LogInformation("[MOCK-{Channel}] Delivered to {Recipient} | Title: {Title} | Body: {Body}",
            Channel, recipient, title ?? "(none)", body);
        return Task.FromResult(DeliveryStatus.DELIVERED);
    }
}

public sealed class MockPushAdapter(ILogger<MockPushAdapter> logger) : BaseMockChannelAdapter(logger)
{
    public override NotificationChannel Channel => NotificationChannel.PUSH;
}

public sealed class MockEmailAdapter(ILogger<MockEmailAdapter> logger) : BaseMockChannelAdapter(logger)
{
    public override NotificationChannel Channel => NotificationChannel.EMAIL;
}

public sealed class MockSmsAdapter(ILogger<MockSmsAdapter> logger) : BaseMockChannelAdapter(logger)
{
    public override NotificationChannel Channel => NotificationChannel.SMS;
}
