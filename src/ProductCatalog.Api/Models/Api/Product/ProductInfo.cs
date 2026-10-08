namespace ProductCatalog.Api.Models.Api.Product;

public sealed class ProductInfo
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}
