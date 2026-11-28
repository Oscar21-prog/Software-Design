using EBank.Notification.Contracts.Providers;
using EBank.Notification.Domain;

namespace EBank.Notification.Application;

public interface INotificationRepository
{
    Task<NotificationLanguage> GetUserLanguageAsync(Guid userId, CancellationToken ct = default);
    Task<NotificationTemplate?> FindTemplateAsync(string eventType, string? variant, NotificationLanguage language, CancellationToken ct = default);
    Task<NotificationPreference?> GetPreferenceAsync(Guid userId, NotificationCategory category, CancellationToken ct = default);
    Task<RecipientContact?> GetContactAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<PushToken>> GetPushTokensAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsMessageAsync(Guid eventId, Guid userId, NotificationChannel channel, CancellationToken ct = default);
    Task SaveMessageAsync(NotificationMessage message, CancellationToken ct = default);
    Task UpsertContactAndSettingsAsync(RecipientContact contact, NotificationLanguage? initialLanguage, CancellationToken ct = default);
    Task UpdateMarketingConsentAsync(Guid userId, bool granted, DateTimeOffset updatedAt, CancellationToken ct = default);
    Task RemoveInvalidPushTokenAsync(Guid pushTokenId, CancellationToken ct = default);
}
