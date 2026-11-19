namespace EBank.BuildingBlocks.Idempotency;

public sealed class IdempotencyRecord
{
    public string Key { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string ResponseJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
