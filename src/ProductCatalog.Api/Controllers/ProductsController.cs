using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.Helpers.Product;
using ProductCatalog.Api.Infrastructure;
using ProductCatalog.Api.Models.Api;
using ProductCatalog.Api.Models.Api.Product;
using ProductCatalog.Api.Services.Interfaces.Products;

namespace ProductCatalog.Api.Controllers;

[ApiController]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Route(Routes.GetProducts)]
    public RestResponse<List<ProductInfo>> GetProducts()
    {
        var products = _productService.GetProducts();
        return RestResponse<List<ProductInfo>>.CreateSuccessResponse(products.ToList());
    }

    [HttpPost]
    [Route(Routes.CreateProduct)]
    [ServiceFilter(typeof(IdempotentCreateProductFilter))]
    public RestResponse<ProductInfo> CreateProduct(CreateProductRequestInfo request)
    {
        var product = _productService.CreateProduct(request);
        return RestResponse<ProductInfo>.CreateSuccessResponse(product);
    }
}
