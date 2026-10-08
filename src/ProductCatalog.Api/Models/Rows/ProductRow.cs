namespace ProductCatalog.Api.Models.Rows;

public sealed class ProductRow
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
