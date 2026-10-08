using ProductCatalog.Api.Models.Api.Product;
using ProductCatalog.Api.Repositories;
using ProductCatalog.Api.Services.Products;

namespace ProductCatalog.Api.Tests;

public class ProductServiceTests
{
    private static ProductService CreateSut()
    {
        var repo = new InMemoryProductRepository();

        repo.TryAdd(new Models.Rows.ProductRow
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Code = "AU-1OZ",
            Name = "Gold bar 1 oz",
            Price = 9800m,
            CreatedAtUtc = DateTimeOffset.UtcNow
        }, out _);

        return new ProductService(repo, repo, new ProductBuildService());
    }

    [Fact]
    public void CreateProduct_ValidRequest_ReturnsProductWithGeneratedId()
    {
        var sut = CreateSut();
        var product = sut.CreateProduct(new CreateProductRequestInfo
        {
            Code = "PL-NEW-1",
            Name = "Test product",
            Price = 12.345m
        });

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("PL-NEW-1", product.Code);
        Assert.Equal("Test product", product.Name);
        Assert.Equal(12.35m, product.Price);
    }

    [Fact]
    public void CreateProduct_DuplicateCode_Throws()
    {
        var sut = CreateSut();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            sut.CreateProduct(new CreateProductRequestInfo
            {
                Code = "AU-1OZ",
                Name = "Duplicate",
                Price = 10m
            }));

        Assert.Contains("AU-1OZ", ex.Message);
    }

    [Fact]
    public void CreateProduct_ParallelSameCode_OnlyOneSucceeds()
    {
        var sut = CreateSut();
        var code = "RACE-CODE";
        var successes = 0;
        var conflicts = 0;

        Parallel.For(0, 50, _ =>
        {
            try
            {
                sut.CreateProduct(new CreateProductRequestInfo
                {
                    Code = code,
                    Name = $"Parallel {Guid.NewGuid():N}",
                    Price = 9.99m
                });
                Interlocked.Increment(ref successes);
            }
            catch (InvalidOperationException)
            {
                Interlocked.Increment(ref conflicts);
            }
        });

        Assert.Equal(1, successes);
        Assert.Equal(49, conflicts);
        Assert.Equal(1, sut.GetProducts().Count(p => p.Code == code));
    }

    [Fact]
    public void GetProducts_ReturnsSeededItems()
    {
        var sut = CreateSut();
        Assert.Contains(sut.GetProducts(), p => p.Code == "AU-1OZ");
    }
}
