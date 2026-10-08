using ProductCatalog.Api.Models.Api.Product;
using ProductCatalog.Api.Models.Domain.Product;
using ProductCatalog.Api.Models.DTO.Product;
using ProductCatalog.Api.Models.Rows;
using ProductCatalog.Api.Services.Interfaces.Products;

namespace ProductCatalog.Api.Services.Products;

public sealed class ProductBuildService : IProductBuildService
{
    public CreateProductDTO BuildCreateProductDTO(CreateProductRequestInfo request) => new()
    {
        Code = request.Code,
        Name = request.Name,
        Price = request.Price
    };

    public ProductInfo BuildProductInfo(ProductRow row) => new()
    {
        Id = row.Id,
        Code = row.Code,
        Name = row.Name,
        Price = row.Price
    };

    public ProductInfo BuildProductInfo(ProductAggregate aggregate) => new()
    {
        Id = aggregate.Id,
        Code = aggregate.Code,
        Name = aggregate.Name,
        Price = aggregate.Price
    };
}
