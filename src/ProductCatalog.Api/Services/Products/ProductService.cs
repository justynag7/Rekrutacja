using ProductCatalog.Api.Models.Api.Product;
using ProductCatalog.Api.Models.Domain.Product;
using ProductCatalog.Api.Repositories.Interfaces;
using ProductCatalog.Api.Services.Interfaces.Products;

namespace ProductCatalog.Api.Services.Products;

public sealed class ProductService : IProductService
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly IProductBuildService _productBuildService;

    public ProductService(
        IProductReadRepository productReadRepository,
        IProductWriteRepository productWriteRepository,
        IProductBuildService productBuildService)
    {
        _productReadRepository = productReadRepository;
        _productWriteRepository = productWriteRepository;
        _productBuildService = productBuildService;
    }

    public IReadOnlyList<ProductInfo> GetProducts() =>
        _productReadRepository
            .GetAll()
            .Select(_productBuildService.BuildProductInfo)
            .ToList();

    public ProductInfo CreateProduct(CreateProductRequestInfo request)
    {
        var dto = _productBuildService.BuildCreateProductDTO(request);
        var aggregate = ProductAggregate.Create(dto);
        aggregate.Validate();

        if (!_productWriteRepository.TryAdd(aggregate.ToRow(), out var conflictCode))
        {
            throw new InvalidOperationException(
                $"Product with code '{conflictCode}' already exists.");
        }

        return _productBuildService.BuildProductInfo(aggregate);
    }
}
