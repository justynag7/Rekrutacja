using ProductCatalog.Api.Models.Api.Product;

namespace ProductCatalog.Api.Services.Interfaces.Products;

public interface IProductService
{
    IReadOnlyList<ProductInfo> GetProducts();
    ProductInfo CreateProduct(CreateProductRequestInfo request);
}
