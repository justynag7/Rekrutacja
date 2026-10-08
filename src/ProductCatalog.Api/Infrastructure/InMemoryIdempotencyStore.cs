using System.Collections.Concurrent;
using ProductCatalog.Api.Infrastructure.Interfaces;
using ProductCatalog.Api.Models.Idempotency;

namespace ProductCatalog.Api.Infrastructure;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);
    private readonly TimeSpan _ttl = TimeSpan.FromHours(24);

    public IdempotencyOutcome BeginOrGet(string key, string requestHash)
    {
        while (true)
        {
            if (_entries.TryGetValue(key, out var existing))
            {
                if (IsExpired(existing))
                {
                    _entries.TryRemove(new KeyValuePair<string, Entry>(key, existing));
                    continue;
                }

                if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
                {
                    return new IdempotencyOutcome { Kind = IdempotencyOutcomeKind.Conflict };
                }

                if (existing.Record is not null)
                {
                    return new IdempotencyOutcome
                    {
                        Kind = IdempotencyOutcomeKind.Replay,
                        Record = existing.Record
                    };
                }

                if (!existing.Completed.Wait(TimeSpan.FromSeconds(15)))
                {
                    return new IdempotencyOutcome { Kind = IdempotencyOutcomeKind.InProgress };
                }

                continue;
            }

            var fresh = new Entry(requestHash);
            if (_entries.TryAdd(key, fresh))
            {
                return new IdempotencyOutcome { Kind = IdempotencyOutcomeKind.Started };
            }
        }
    }

    public void Complete(string key, IdempotencyRecord record)
    {
        if (_entries.TryGetValue(key, out var entry))
        {
            entry.Record = record;
            entry.Completed.Set();
        }
    }

    public void Abandon(string key)
    {
        if (_entries.TryRemove(key, out var entry))
        {
            entry.Completed.Set();
        }
    }

    private bool IsExpired(Entry entry)
    {
        if (entry.Record is null)
        {
            return false;
        }

        return DateTimeOffset.UtcNow - entry.Record.CreatedAtUtc > _ttl;
    }

    private sealed class Entry
    {
        public Entry(string requestHash)
        {
            RequestHash = requestHash;
        }

        public string RequestHash { get; }
        public IdempotencyRecord? Record { get; set; }
        public ManualResetEventSlim Completed { get; } = new(false);
    }
}
