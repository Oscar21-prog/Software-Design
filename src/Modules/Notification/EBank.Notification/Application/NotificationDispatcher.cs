using EBank.BuildingBlocks.Events;
using EBank.BuildingBlocks.Time;
using EBank.Notification.Contracts.Events;
using EBank.Notification.Contracts.Providers;
using EBank.Notification.Domain;

namespace EBank.Notification.Application;

public sealed record DispatchNotificationCommand(
    Guid EventId,
    string EventType,
    string? Variant,
    Guid UserId,
    IReadOnlyDictionary<string, object?> Placeholders
);

public sealed class NotificationDispatcher(
    INotificationRepository repository,
    IEnumerable<INotificationChannelAdapter> adapters,
    IEventBus eventBus,
    IClock clock)
{
    private static readonly NotificationChannel[] MandatoryOrder =
    [
        NotificationChannel.PUSH,
        NotificationChannel.SMS,
        NotificationChannel.EMAIL
    ];

    public async Task DispatchAsync(DispatchNotificationCommand cmd, CancellationToken ct = default)
    {
        // NTF-02: Load user language (LT or EN), fallback to EN if template missing
        var userLang = await repository.GetUserLanguageAsync(cmd.UserId, ct);
        var template = await repository.FindTemplateAsync(cmd.EventType, cmd.Variant, userLang, ct)
                       ?? await repository.FindTemplateAsync(cmd.EventType, cmd.Variant, NotificationLanguage.EN, ct);

        if (template is null)
            return;

        // NTF-06: Security sanitization — never include full card numbers, passwords, or AML rejection reasons
        var safePlaceholders = SanitizePlaceholders(cmd.Placeholders);
        var (title, deliveryBody, storedBody) = TemplateRenderer.Render(
            cmd.EventType, template.Title, template.Body, safePlaceholders);

        var contact = await repository.GetContactAsync(cmd.UserId, ct);
        var pushTokens = await repository.GetPushTokensAsync(cmd.UserId, ct);

        // Special rule F1 A1 & NTF-04: OTP_REQUESTED is sent ONLY by SMS, once
        if (cmd.EventType == "OTP_REQUESTED")
        {
            await SendSingleChannelAsync(cmd, template, NotificationChannel.SMS, title, deliveryBody, storedBody, contact, pushTokens, publishFailureOnFail: true, ct);
            return;
        }

        // NTF-04 & NTF-05: Mandatory templates ignore preferences and try PUSH -> SMS -> EMAIL (max 3 attempts)
        if (template.Mandatory)
        {
            var deliveredAny = false;
            var attempts = 0;

            foreach (var channel in MandatoryOrder)
            {
                if (attempts >= 3) break;
                attempts++;

                var delivered = await SendSingleChannelAsync(
                    cmd, template, channel, title, deliveryBody, storedBody, contact, pushTokens, publishFailureOnFail: false, ct);

                if (delivered)
                {
                    deliveredAny = true;
                    break;
                }
            }

            if (!deliveredAny)
            {
                await PublishFailureAsync(Guid.NewGuid(), cmd, "ALL_MANDATORY_CHANNELS_FAILED", ct);
            }
            return;
        }

        // NTF-03: Non-mandatory notifications use enabled channels from NotificationPreference (or §12.7.1 defaults)
        var pref = await repository.GetPreferenceAsync(cmd.UserId, template.Category, ct);
        var isMarketing = template.Category == NotificationCategory.MARKETING;

        var pushEnabled = pref?.PushEnabled ?? !isMarketing;
        var emailEnabled = pref?.EmailEnabled ?? !isMarketing;
        var smsEnabled = pref?.SmsEnabled ?? false;

        var enabledChannels = new List<NotificationChannel>(3);
        if (pushEnabled) enabledChannels.Add(NotificationChannel.PUSH);
        if (emailEnabled) enabledChannels.Add(NotificationChannel.EMAIL);
        if (smsEnabled) enabledChannels.Add(NotificationChannel.SMS);

        // F1 A3: Customer turned off every channel -> send nothing and save no Notification
        if (enabledChannels.Count == 0)
            return;

        var anySuccess = false;
        foreach (var channel in enabledChannels)
        {
            var ok = await SendSingleChannelAsync(
                cmd, template, channel, title, deliveryBody, storedBody, contact, pushTokens, publishFailureOnFail: false, ct);
            if (ok) anySuccess = true;
        }

        if (!anySuccess)
        {
            await PublishFailureAsync(Guid.NewGuid(), cmd, "ALL_ENABLED_CHANNELS_FAILED", ct);
        }
    }

    private async Task<bool> SendSingleChannelAsync(
        DispatchNotificationCommand cmd,
        NotificationTemplate template,
        NotificationChannel channel,
        string title,
        string deliveryBody,
        string storedBody,
        RecipientContact? contact,
        IReadOnlyList<PushToken> pushTokens,
        bool publishFailureOnFail,
        CancellationToken ct)
    {
        // NTF-NFR-02: Idempotency check (eventId + userId + channel is unique)
        if (await repository.ExistsMessageAsync(cmd.EventId, cmd.UserId, channel, ct))
            return true;

        var now = clock.UtcNow;
        var msg = new NotificationMessage
        {
            EventId = cmd.EventId,
            UserId = cmd.UserId,
            TemplateId = template.Id,
            Channel = channel,
            Title = title,
            Body = storedBody,
            Status = DeliveryStatus.PENDING,
            CreatedAt = now
        };

        var adapter = adapters.FirstOrDefault(a => a.Channel == channel);
        if (adapter is null)
        {
            msg.Status = DeliveryStatus.FAILED;
            msg.LastError = $"No adapter registered for {channel}";
            await repository.SaveMessageAsync(msg, ct);
            if (publishFailureOnFail) await PublishFailureAsync(msg.Id, cmd, msg.LastError, ct);
            return false;
        }

        if (channel == NotificationChannel.PUSH)
        {
            if (pushTokens.Count == 0)
            {
                msg.Status = DeliveryStatus.FAILED;
                msg.LastError = "NO_PUSH_TOKENS";
                await repository.SaveMessageAsync(msg, ct);
                if (publishFailureOnFail) await PublishFailureAsync(msg.Id, cmd, msg.LastError, ct);
                return false;
            }

            var atLeastOneDelivered = false;
            foreach (var pt in pushTokens)
            {
                var status = await SendWithTimeoutAsync(adapter, pt.Token, title, deliveryBody, ct);
                if (status == DeliveryStatus.DELIVERED)
                    atLeastOneDelivered = true;
            }

            if (atLeastOneDelivered)
            {
                msg.Status = DeliveryStatus.DELIVERED;
                msg.SentAt = clock.UtcNow;
                await repository.SaveMessageAsync(msg, ct);
                return true;
            }

            msg.Status = DeliveryStatus.FAILED;
            msg.LastError = "PUSH_DELIVERY_FAILED";
            await repository.SaveMessageAsync(msg, ct);
            if (publishFailureOnFail) await PublishFailureAsync(msg.Id, cmd, msg.LastError, ct);
            return false;
        }

        var recipient = channel == NotificationChannel.EMAIL ? contact?.Email : contact?.Phone;
        if (string.IsNullOrWhiteSpace(recipient))
        {
            msg.Status = DeliveryStatus.FAILED;
            msg.LastError = channel == NotificationChannel.EMAIL ? "NO_EMAIL_ADDRESS" : "NO_PHONE_NUMBER";
            await repository.SaveMessageAsync(msg, ct);
            if (publishFailureOnFail) await PublishFailureAsync(msg.Id, cmd, msg.LastError, ct);
            return false;
        }

        // Spec §12.8.2: An SMS carries only the body
        var effectiveTitle = channel == NotificationChannel.SMS ? null : title;
        var deliveryStatus = await SendWithTimeoutAsync(adapter, recipient, effectiveTitle, deliveryBody, ct);

        if (deliveryStatus == DeliveryStatus.DELIVERED)
        {
            msg.Status = DeliveryStatus.DELIVERED;
            msg.SentAt = clock.UtcNow;
            await repository.SaveMessageAsync(msg, ct);
            return true;
        }

        msg.Status = DeliveryStatus.FAILED;
        msg.LastError = "PROVIDER_DELIVERY_FAILED_OR_TIMEOUT";
        await repository.SaveMessageAsync(msg, ct);
        if (publishFailureOnFail) await PublishFailureAsync(msg.Id, cmd, msg.LastError, ct);
        return false;
    }

    private static async Task<DeliveryStatus> SendWithTimeoutAsync(
        INotificationChannelAdapter adapter, string recipient, string? title, string body, CancellationToken ct)
    {
        try
        {
            // Spec §12.8.2: Provider must respond within 30 seconds
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
            return await adapter.SendAsync(recipient, title, body, timeoutCts.Token);
        }
        catch
        {
            return DeliveryStatus.FAILED;
        }
    }

    private async Task PublishFailureAsync(Guid notificationId, DispatchNotificationCommand cmd, string reason, CancellationToken ct)
    {
        await eventBus.PublishAsync("Notification", new NotificationFailedEvent(
            notificationId,
            cmd.EventId,
            cmd.EventType,
            cmd.UserId,
            "MULTI",
            reason,
            clock.UtcNow
        ), ct);
    }

    private static IReadOnlyDictionary<string, object?> SanitizePlaceholders(IReadOnlyDictionary<string, object?> input)
    {
        var clean = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (k, v) in input)
        {
            // NTF-06: Strip forbidden sensitive keys if accidentally passed
            if (k.Equals("password", StringComparison.OrdinalIgnoreCase) ||
                k.Equals("passwordHash", StringComparison.OrdinalIgnoreCase) ||
                k.Equals("cardNumber", StringComparison.OrdinalIgnoreCase) ||
                k.Equals("pan", StringComparison.OrdinalIgnoreCase) ||
                k.Equals("amlReason", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            clean[k] = v;
        }
        return clean;
    }
}
