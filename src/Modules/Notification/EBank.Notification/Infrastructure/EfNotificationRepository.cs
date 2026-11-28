using EBank.Notification.Application;
using EBank.Notification.Contracts.Providers;
using EBank.Notification.Domain;
using Microsoft.EntityFrameworkCore;

namespace EBank.Notification.Infrastructure;

public sealed class EfNotificationRepository(NotificationDbContext db) : INotificationRepository
{
    public async Task<NotificationLanguage> GetUserLanguageAsync(Guid userId, CancellationToken ct = default)
    {
        var settings = await db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);
        return settings?.Language ?? NotificationLanguage.EN;
    }

    public async Task<NotificationTemplate?> FindTemplateAsync(
        string eventType, string? variant, NotificationLanguage language, CancellationToken ct = default)
    {
        var template = await db.Templates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EventType == eventType && x.Variant == variant && x.Language == language, ct);

        if (template is not null)
            return template;

        return DefaultTemplatesSeed.GetAll()
            .FirstOrDefault(x => x.EventType == eventType && x.Variant == variant && x.Language == language);
    }

    public Task<NotificationPreference?> GetPreferenceAsync(Guid userId, NotificationCategory category, CancellationToken ct = default) =>
        db.Preferences.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.Category == category, ct);

    public Task<RecipientContact?> GetContactAsync(Guid userId, CancellationToken ct = default) =>
        db.Contacts.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public async Task<IReadOnlyList<PushToken>> GetPushTokensAsync(Guid userId, CancellationToken ct = default) =>
        await db.PushTokens.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(ct);

    public Task<bool> ExistsMessageAsync(Guid eventId, Guid userId, NotificationChannel channel, CancellationToken ct = default) =>
        db.Notifications.AnyAsync(x => x.EventId == eventId && x.UserId == userId && x.Channel == channel, ct);

    public async Task SaveMessageAsync(NotificationMessage message, CancellationToken ct = default)
    {
        db.Notifications.Add(message);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpsertContactAndSettingsAsync(RecipientContact contact, NotificationLanguage? initialLanguage, CancellationToken ct = default)
    {
        var existing = await db.Contacts.FirstOrDefaultAsync(x => x.UserId == contact.UserId, ct);
        if (existing is null)
        {
            db.Contacts.Add(contact);
        }
        else
        {
            existing.Email = contact.Email;
            existing.Phone = contact.Phone;
            existing.UpdatedAt = contact.UpdatedAt;
        }

        if (initialLanguage.HasValue)
        {
            var settings = await db.Settings.FirstOrDefaultAsync(x => x.UserId == contact.UserId, ct);
            if (settings is null)
            {
                db.Settings.Add(new NotificationSettings
                {
                    UserId = contact.UserId,
                    Language = initialLanguage.Value,
                    UpdatedAt = contact.UpdatedAt
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateMarketingConsentAsync(Guid userId, bool granted, DateTimeOffset updatedAt, CancellationToken ct = default)
    {
        var existing = await db.Contacts.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (existing is not null)
        {
            existing.MarketingConsent = granted;
            existing.UpdatedAt = updatedAt;

            if (!granted)
            {
                var marketingPref = await db.Preferences.FirstOrDefaultAsync(
                    x => x.UserId == userId && x.Category == NotificationCategory.MARKETING, ct);
                if (marketingPref is not null)
                {
                    marketingPref.PushEnabled = false;
                    marketingPref.EmailEnabled = false;
                    marketingPref.SmsEnabled = false;
                }
            }

            await db.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveInvalidPushTokenAsync(Guid pushTokenId, CancellationToken ct = default)
    {
        var token = await db.PushTokens.FirstOrDefaultAsync(x => x.Id == pushTokenId, ct);
        if (token is not null)
        {
            db.PushTokens.Remove(token);
            await db.SaveChangesAsync(ct);
        }
    }
}
