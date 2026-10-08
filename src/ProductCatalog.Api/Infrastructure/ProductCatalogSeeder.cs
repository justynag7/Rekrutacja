using Microsoft.AspNetCore.Identity;
using ProductCatalog.Api.Models.Identity;
using ProductCatalog.Api.Models.Rows;
using ProductCatalog.Api.Repositories;
using ProductCatalog.Api.Repositories.Interfaces;

namespace ProductCatalog.Api.Infrastructure;

public static class ProductCatalogSeeder
{
    public static void Seed(IServiceProvider services)
    {
        SeedProducts(services.GetRequiredService<IProductWriteRepository>());
        SeedUsers(
            services.GetRequiredService<InMemoryUserRepository>(),
            services.GetRequiredService<IPasswordHasher<ApplicationUser>>());
    }

    private static void SeedProducts(IProductWriteRepository write)
    {
        write.TryAdd(new ProductRow
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Code = "AU-1OZ",
            Name = "Gold bar 1 oz",
            Price = 9800.00m,
            CreatedAtUtc = DateTimeOffset.UtcNow
        }, out _);

        write.TryAdd(new ProductRow
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Code = "AG-1OZ",
            Name = "Silver coin 1 oz",
            Price = 145.50m,
            CreatedAtUtc = DateTimeOffset.UtcNow
        }, out _);
    }

    private static void SeedUsers(
        InMemoryUserRepository users,
        IPasswordHasher<ApplicationUser> hasher)
    {
        var admin = new ApplicationUser
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Email = "admin@catalog.local",
            DisplayName = "Catalog Admin"
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
        users.Upsert(admin);
    }
}
