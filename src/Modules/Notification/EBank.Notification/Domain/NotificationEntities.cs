using EBank.Notification.Contracts.Providers;

namespace EBank.Notification.Domain;

public sealed class NotificationTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public string? Variant { get; set; }
    public NotificationCategory Category { get; set; }
    public NotificationLanguage Language { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
}

public sealed class NotificationMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public Guid TemplateId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty; // OTP codes masked as ******
    public DeliveryStatus Status { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
}

public sealed class NotificationPreference
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public NotificationCategory Category { get; set; }
    public bool PushEnabled { get; set; }
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
}

public sealed class NotificationSettings
{
    public Guid UserId { get; set; }
    public NotificationLanguage Language { get; set; } = NotificationLanguage.EN;
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class PushToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public DevicePlatform Platform { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class RecipientContact
{
    public Guid UserId { get; set; }
    public Guid CustomerId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool MarketingConsent { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
