using EBank.BuildingBlocks.Events;
using EBank.BuildingBlocks.Time;
using EBank.Notification.Application;
using EBank.Notification.Contracts.Events;
using EBank.Notification.Contracts.Providers;
using EBank.Notification.Domain;
using FluentAssertions;
using Xunit;

namespace EBank.Notification.Tests;

public sealed class NotificationDispatcherTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 11, 28, 12, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeEventBus : IEventBus
    {
        public List<object> PublishedEvents { get; } = [];
        public Task PublishAsync<TEvent>(string sourceModule, TEvent @event, CancellationToken ct = default)
            where TEvent : IIntegrationEvent
        {
            PublishedEvents.Add(@event!);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAdapter(NotificationChannel channel, Func<string, DeliveryStatus> behavior) : INotificationChannelAdapter
    {
        public NotificationChannel Channel => channel;
        public List<(string Recipient, string? Title, string Body)> SentMessages { get; } = [];

        public Task<DeliveryStatus> SendAsync(string recipient, string? title, string body, CancellationToken ct = default)
        {
            SentMessages.Add((recipient, title, body));
            return Task.FromResult(behavior(recipient));
        }
    }

    private sealed class InMemoryNotificationRepo : INotificationRepository
    {
        public Dictionary<Guid, NotificationLanguage> Languages { get; } = [];
        public Dictionary<(Guid, NotificationCategory), NotificationPreference> Preferences { get; } = [];
        public Dictionary<Guid, RecipientContact> Contacts { get; } = [];
        public List<PushToken> PushTokens { get; } = [];
        public List<NotificationMessage> SavedMessages { get; } = [];

        public Task<NotificationLanguage> GetUserLanguageAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult(Languages.TryGetValue(userId, out var l) ? l : NotificationLanguage.EN);

        public Task<NotificationTemplate?> FindTemplateAsync(string eventType, string? variant, NotificationLanguage language, CancellationToken ct = default) =>
            Task.FromResult(DefaultTemplatesSeed.GetAll().FirstOrDefault(t => t.EventType == eventType && t.Variant == variant && t.Language == language));

        public Task<NotificationPreference?> GetPreferenceAsync(Guid userId, NotificationCategory category, CancellationToken ct = default) =>
            Task.FromResult(Preferences.TryGetValue((userId, category), out var p) ? p : null);

        public Task<RecipientContact?> GetContactAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult(Contacts.TryGetValue(userId, out var c) ? c : null);

        public Task<IReadOnlyList<PushToken>> GetPushTokensAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PushToken>>(PushTokens.Where(t => t.UserId == userId).ToList());

        public Task<bool> ExistsMessageAsync(Guid eventId, Guid userId, NotificationChannel channel, CancellationToken ct = default) =>
            Task.FromResult(SavedMessages.Any(m => m.EventId == eventId && m.UserId == userId && m.Channel == channel));

        public Task SaveMessageAsync(NotificationMessage message, CancellationToken ct = default)
        {
            SavedMessages.Add(message);
            return Task.CompletedTask;
        }

        public Task UpsertContactAndSettingsAsync(RecipientContact contact, NotificationLanguage? initialLanguage, CancellationToken ct = default)
        {
            Contacts[contact.UserId] = contact;
            if (initialLanguage.HasValue) Languages[contact.UserId] = initialLanguage.Value;
            return Task.CompletedTask;
        }

        public Task UpdateMarketingConsentAsync(Guid userId, bool granted, DateTimeOffset updatedAt, CancellationToken ct = default) => Task.CompletedTask;
        public Task RemoveInvalidPushTokenAsync(Guid pushTokenId, CancellationToken ct = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task NTF01_NTF02_TransactionCompleted_UsesLithuanianTemplate_AndFormatsAmount()
    {
        var repo = new InMemoryNotificationRepo();
        var userId = Guid.NewGuid();
        repo.Languages[userId] = NotificationLanguage.LT;
        repo.Contacts[userId] = new RecipientContact { UserId = userId, Email = "user@ebank.lt", Phone = "+37060000001" };
        repo.PushTokens.Add(new PushToken { UserId = userId, DeviceId = "dev1", Token = "tok1", Platform = DevicePlatform.IOS });

        var push = new FakeAdapter(NotificationChannel.PUSH, _ => DeliveryStatus.DELIVERED);
        var email = new FakeAdapter(NotificationChannel.EMAIL, _ => DeliveryStatus.DELIVERED);
        var sms = new FakeAdapter(NotificationChannel.SMS, _ => DeliveryStatus.DELIVERED);

        var dispatcher = new NotificationDispatcher(repo, [push, email, sms], new FakeEventBus(), new FakeClock());

        await dispatcher.DispatchAsync(new DispatchNotificationCommand(
            Guid.NewGuid(),
            "TRANSACTION_COMPLETED",
            "INCOMING",
            userId,
            new Dictionary<string, object?>
            {
                ["amount"] = 125.5m,
                ["currency"] = "EUR",
                ["counterpartyName"] = "Jonas Jonaitis"
            }));

        // Default preference: push on, email on, SMS off (NTF-03)
        push.SentMessages.Should().ContainSingle();
        push.SentMessages[0].Title.Should().Be("Gautas mokėjimas");
        push.SentMessages[0].Body.Should().Be("Gavote 125.50 EUR iš Jonas Jonaitis.");

        email.SentMessages.Should().ContainSingle();
        sms.SentMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task NTF04_NTF06_OtpRequested_SendsOnlyBySms_AndMasksStoredCode()
    {
        var repo = new InMemoryNotificationRepo();
        var userId = Guid.NewGuid();
        repo.Languages[userId] = NotificationLanguage.EN;
        repo.Contacts[userId] = new RecipientContact { UserId = userId, Email = "user@ebank.lt", Phone = "+37060000002" };
        repo.PushTokens.Add(new PushToken { UserId = userId, DeviceId = "dev1", Token = "tok1", Platform = DevicePlatform.ANDROID });

        var push = new FakeAdapter(NotificationChannel.PUSH, _ => DeliveryStatus.DELIVERED);
        var email = new FakeAdapter(NotificationChannel.EMAIL, _ => DeliveryStatus.DELIVERED);
        var sms = new FakeAdapter(NotificationChannel.SMS, _ => DeliveryStatus.DELIVERED);

        var dispatcher = new NotificationDispatcher(repo, [push, email, sms], new FakeEventBus(), new FakeClock());

        await dispatcher.DispatchAsync(new DispatchNotificationCommand(
            Guid.NewGuid(),
            "OTP_REQUESTED",
            null,
            userId,
            new Dictionary<string, object?> { ["code"] = "482910" }));

        // Sent only via SMS with real code and null title (§12.8.2 & NTF-04)
        push.SentMessages.Should().BeEmpty();
        email.SentMessages.Should().BeEmpty();
        sms.SentMessages.Should().ContainSingle();
        sms.SentMessages[0].Title.Should().BeNull();
        sms.SentMessages[0].Body.Should().Contain("482910");

        // Stored copy has code masked as ****** (NTF-06 / NTF-NFR-04)
        repo.SavedMessages.Should().ContainSingle();
        repo.SavedMessages[0].Body.Should().Be("Your code is ******. Never share it with anyone.");
        repo.SavedMessages[0].Body.Should().NotContain("482910");
    }

    [Fact]
    public async Task NTF04_NTF05_MandatoryNotification_FallsBackFromPushToSms_WhenPushFails()
    {
        var repo = new InMemoryNotificationRepo();
        var userId = Guid.NewGuid();
        repo.Contacts[userId] = new RecipientContact { UserId = userId, Email = "user@ebank.lt", Phone = "+37060000003" };
        // User turned off all channels for SECURITY, which must be ignored (NTF-04)
        repo.Preferences[(userId, NotificationCategory.SECURITY)] = new NotificationPreference
        {
            UserId = userId,
            Category = NotificationCategory.SECURITY,
            PushEnabled = false,
            EmailEnabled = false,
            SmsEnabled = false
        };

        var push = new FakeAdapter(NotificationChannel.PUSH, _ => DeliveryStatus.FAILED); // No push tokens anyway -> fails
        var sms = new FakeAdapter(NotificationChannel.SMS, _ => DeliveryStatus.DELIVERED);
        var email = new FakeAdapter(NotificationChannel.EMAIL, _ => DeliveryStatus.DELIVERED);

        var dispatcher = new NotificationDispatcher(repo, [push, sms, email], new FakeEventBus(), new FakeClock());

        await dispatcher.DispatchAsync(new DispatchNotificationCommand(
            Guid.NewGuid(),
            "KYC_APPROVED",
            null,
            userId,
            new Dictionary<string, object?>()));

        // Attempt 1: PUSH (failed), Attempt 2: SMS (delivered) -> stops before EMAIL
        repo.SavedMessages.Should().HaveCount(2);
        repo.SavedMessages[0].Channel.Should().Be(NotificationChannel.PUSH);
        repo.SavedMessages[0].Status.Should().Be(DeliveryStatus.FAILED);
        repo.SavedMessages[1].Channel.Should().Be(NotificationChannel.SMS);
        repo.SavedMessages[1].Status.Should().Be(DeliveryStatus.DELIVERED);
        email.SentMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task DuplicateEventId_IsIgnoredIdempotently()
    {
        var repo = new InMemoryNotificationRepo();
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        repo.Contacts[userId] = new RecipientContact { UserId = userId, Email = "user@ebank.lt", Phone = "+37060000004" };

        var email = new FakeAdapter(NotificationChannel.EMAIL, _ => DeliveryStatus.DELIVERED);
        var dispatcher = new NotificationDispatcher(repo, [email], new FakeEventBus(), new FakeClock());

        var cmd = new DispatchNotificationCommand(eventId, "KYC_APPROVED", null, userId, new Dictionary<string, object?>());

        await dispatcher.DispatchAsync(cmd);
        await dispatcher.DispatchAsync(cmd); // Duplicate event

        email.SentMessages.Should().ContainSingle();
    }
}
