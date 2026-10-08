namespace ProductCatalog.Api.Models.DTO.Product;


public sealed class CreateProductDTO
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}
