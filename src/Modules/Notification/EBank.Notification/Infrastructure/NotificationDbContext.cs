using EBank.Notification.Domain;
using Microsoft.EntityFrameworkCore;

namespace EBank.Notification.Infrastructure;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<NotificationTemplate> Templates => Set<NotificationTemplate>();
    public DbSet<NotificationMessage> Notifications => Set<NotificationMessage>();
    public DbSet<NotificationPreference> Preferences => Set<NotificationPreference>();
    public DbSet<NotificationSettings> Settings => Set<NotificationSettings>();
    public DbSet<PushToken> PushTokens => Set<PushToken>();
    public DbSet<RecipientContact> Contacts => Set<RecipientContact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notification");

        modelBuilder.Entity<NotificationTemplate>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            b.Property(x => x.Variant).HasMaxLength(50);
            b.Property(x => x.Category).HasConversion<string>().HasMaxLength(30);
            b.Property(x => x.Language).HasConversion<string>().HasMaxLength(5);
            b.HasIndex(x => new { x.EventType, x.Variant, x.Language }).IsUnique();
        });

        modelBuilder.Entity<NotificationMessage>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            // Spec §12.7.2 & NTF-NFR-02: eventId + userId + channel is unique
            b.HasIndex(x => new { x.EventId, x.UserId, x.Channel }).IsUnique();
        });

        modelBuilder.Entity<NotificationPreference>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Category).HasConversion<string>().HasMaxLength(30);
            b.HasIndex(x => new { x.UserId, x.Category }).IsUnique();
        });

        modelBuilder.Entity<NotificationSettings>(b =>
        {
            b.HasKey(x => x.UserId);
            b.Property(x => x.Language).HasConversion<string>().HasMaxLength(5);
        });

        modelBuilder.Entity<PushToken>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Platform).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(x => new { x.UserId, x.DeviceId }).IsUnique();
        });

        modelBuilder.Entity<RecipientContact>(b =>
        {
            b.HasKey(x => x.UserId);
        });
    }
}
