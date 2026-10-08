namespace ProductCatalog.Api.Models.Idempotency;

public enum IdempotencyOutcomeKind
{
    Started,
    Replay,
    Conflict,
    InProgress
}

public sealed class IdempotencyOutcome
{
    public IdempotencyOutcomeKind Kind { get; init; }
    public IdempotencyRecord? Record { get; init; }
}

public sealed class IdempotencyRecord
{
    public required string RequestHash { get; init; }
    public int StatusCode { get; init; }
    public string ContentType { get; init; } = "application/json";
    public string Body { get; init; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; init; }
}
