using ProductCatalog.Api.Helpers.Product;
using ProductCatalog.Api.Models.DTO.Product;
using ProductCatalog.Api.Models.Rows;

namespace ProductCatalog.Api.Models.Domain.Product;


public sealed class ProductAggregate
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private ProductAggregate()
    {
    }

    public static ProductAggregate Create(CreateProductDTO dto)
    {
        return new ProductAggregate
        {
            Id = Guid.NewGuid(),
            Code = ProductTextHelper.NormalizeCode(dto.Code),
            Name = ProductTextHelper.SanitizeName(dto.Name),
            Price = decimal.Round(dto.Price, 2, MidpointRounding.AwayFromZero),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public static ProductAggregate FromRow(ProductRow row)
    {
        return new ProductAggregate
        {
            Id = row.Id,
            Code = row.Code,
            Name = row.Name,
            Price = row.Price,
            CreatedAtUtc = row.CreatedAtUtc
        };
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Code))
        {
            throw new InvalidOperationException("Code is required.");
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Name is required.");
        }

        if (Price <= 0)
        {
            throw new InvalidOperationException("Price must be greater than 0.");
        }
    }

    public ProductRow ToRow() => new()
    {
        Id = Id,
        Code = Code,
        Name = Name,
        Price = Price,
        CreatedAtUtc = CreatedAtUtc
    };
}
