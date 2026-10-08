using ProductCatalog.Api.Models.Rows;
using ProductCatalog.Api.Repositories;

namespace ProductCatalog.Api.Tests;

public class InMemoryProductRepositoryTests
{
    [Fact]
    public void TryAdd_ConcurrentIdenticalCodes_OnlyOneSucceeds()
    {
        var repo = new InMemoryProductRepository();
        var wins = 0;

        Parallel.For(0, 100, i =>
        {
            var row = new ProductRow
            {
                Id = Guid.NewGuid(),
                Code = "CONCURRENT",
                Name = $"Name {i}",
                Price = 1m,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            if (repo.TryAdd(row, out _))
            {
                Interlocked.Increment(ref wins);
            }
        });

        Assert.Equal(1, wins);
        Assert.Single(repo.GetAll().Where(p => p.Code == "CONCURRENT"));
    }
}
