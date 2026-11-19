namespace EBank.BuildingBlocks.Audit;

public sealed class AuditLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Module { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
