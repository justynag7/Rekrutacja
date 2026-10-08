using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Repositories.Interfaces;

public interface IProductRepository
{
    IReadOnlyList<Product> GetAll();
    Product? GetByKod(string kod);
    Product Add(Product product);
}
