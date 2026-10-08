using System.Collections.Concurrent;
using ProductCatalog.Api.Models.Rows;
using ProductCatalog.Api.Repositories.Interfaces;

namespace ProductCatalog.Api.Repositories;


public sealed class InMemoryProductRepository : IProductReadRepository, IProductWriteRepository
{
    private readonly ConcurrentDictionary<Guid, ProductRow> _byId = new();
    private readonly ConcurrentDictionary<string, Guid> _codeIndex =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ProductRow> GetAll() =>
        _byId.Values
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public bool TryAdd(ProductRow row, out string? conflictCode)
    {
        if (!_codeIndex.TryAdd(row.Code, row.Id))
        {
            conflictCode = row.Code;
            return false;
        }

        if (!_byId.TryAdd(row.Id, row))
        {
            _codeIndex.TryRemove(row.Code, out _);
            conflictCode = null;
            throw new InvalidOperationException("Failed to store product due to ID collision.");
    }

        conflictCode = null;
        return true;
    }
}
