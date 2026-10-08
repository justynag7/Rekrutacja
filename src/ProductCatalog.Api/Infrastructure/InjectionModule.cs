using Microsoft.AspNetCore.Identity;
using ProductCatalog.Api.Infrastructure.Interfaces;
using ProductCatalog.Api.Models.Configuration;
using ProductCatalog.Api.Models.Identity;
using ProductCatalog.Api.Repositories;
using ProductCatalog.Api.Repositories.Interfaces;
using ProductCatalog.Api.Services.Auth;
using ProductCatalog.Api.Services.Interfaces.Auth;
using ProductCatalog.Api.Services.Interfaces.Products;
using ProductCatalog.Api.Services.Products;

namespace ProductCatalog.Api.Infrastructure;

public static class InjectionModule
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        services.AddSingleton<InMemoryProductRepository>();
        services.AddSingleton<IProductReadRepository>(sp => sp.GetRequiredService<InMemoryProductRepository>());
        services.AddSingleton<IProductWriteRepository>(sp => sp.GetRequiredService<InMemoryProductRepository>());
        services.AddSingleton<InMemoryUserRepository>();
        services.AddSingleton<IUserRepository>(sp => sp.GetRequiredService<InMemoryUserRepository>());
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

        services.AddSingleton<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
        services.AddScoped<IProductBuildService, ProductBuildService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IdempotentCreateProductFilter>();
        services.AddScoped<RestResponseExceptionFilter>();
    }
}
