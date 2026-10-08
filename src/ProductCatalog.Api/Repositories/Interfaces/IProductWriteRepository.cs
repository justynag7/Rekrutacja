using ProductCatalog.Api.Models.Rows;

namespace ProductCatalog.Api.Repositories.Interfaces;

public interface IProductWriteRepository
{
    bool TryAdd(ProductRow row, out string? conflictCode);
}
