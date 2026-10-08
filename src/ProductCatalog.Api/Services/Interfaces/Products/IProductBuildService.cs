using ProductCatalog.Api.Models.Api.Product;
using ProductCatalog.Api.Models.Domain.Product;
using ProductCatalog.Api.Models.DTO.Product;
using ProductCatalog.Api.Models.Rows;

namespace ProductCatalog.Api.Services.Interfaces.Products;

public interface IProductBuildService
{
    CreateProductDTO BuildCreateProductDTO(CreateProductRequestInfo request);
    ProductInfo BuildProductInfo(ProductRow row);
    ProductInfo BuildProductInfo(ProductAggregate aggregate);
}
