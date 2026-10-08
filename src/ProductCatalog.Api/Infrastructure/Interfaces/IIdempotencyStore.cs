using ProductCatalog.Api.Models.Idempotency;

namespace ProductCatalog.Api.Infrastructure.Interfaces;

public interface IIdempotencyStore
{
    IdempotencyOutcome BeginOrGet(string key, string requestHash);
    void Complete(string key, IdempotencyRecord record);
    void Abandon(string key);
}
