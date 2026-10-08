using ProductCatalog.Api.Models.Rows;

namespace ProductCatalog.Api.Repositories.Interfaces;

public interface IProductReadRepository
{
    IReadOnlyList<ProductRow> GetAll();
}
