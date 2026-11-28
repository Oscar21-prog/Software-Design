using EBank.BuildingBlocks.Events;
using EBank.Crm.Contracts.Events;
using EBank.Kyc.Contracts.Events;
using EBank.Notification.Application;
using EBank.Notification.Contracts.Providers;
using EBank.Notification.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EBank.Notification;

public static class NotificationModuleExtensions
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "notification")));

        services.AddScoped<INotificationRepository, EfNotificationRepository>();
        services.AddScoped<NotificationDispatcher>();

        // Mock channel adapters (Spec §12.8.2)
        services.AddSingleton<INotificationChannelAdapter, MockPushAdapter>();
        services.AddSingleton<INotificationChannelAdapter, MockEmailAdapter>();
        services.AddSingleton<INotificationChannelAdapter, MockSmsAdapter>();

        // Event handlers
        services.AddScoped<IIntegrationEventHandler<CustomerRegisteredEvent>, CustomerRegisteredHandler>();
        services.AddScoped<IIntegrationEventHandler<CustomerProfileUpdatedEvent>, CustomerProfileUpdatedHandler>();
        services.AddScoped<IIntegrationEventHandler<ConsentChangedEvent>, ConsentChangedHandler>();
        services.AddScoped<IIntegrationEventHandler<KycApprovedEvent>, KycApprovedHandler>();
        services.AddScoped<IIntegrationEventHandler<KycInfoRequestedEvent>, KycInfoRequestedHandler>();

        return services;
    }
}
