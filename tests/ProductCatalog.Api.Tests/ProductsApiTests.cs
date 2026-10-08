using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ProductCatalog.Api.Models.Api;
using ProductCatalog.Api.Models.Api.Auth;
using ProductCatalog.Api.Models.Api.Product;

namespace ProductCatalog.Api.Tests;

public class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithToken_ReturnsSuccessEnvelope()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<RestResponse<List<ProductInfo>>>(JsonOptions);
        Assert.True(body!.IsSuccess);
        Assert.NotEmpty(body.Data!);
    }

    [Fact]
    public async Task CreateProduct_Valid_ReturnsSuccess()
    {
        await AuthenticateAsync();
        var code = $"OK-{Guid.NewGuid():N}"[..16];
        using var request = CreatePost(new { code, name = "Valid product", price = 19.99m }, Guid.NewGuid().ToString("N"));

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<RestResponse<ProductInfo>>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body!.IsSuccess);
        Assert.Equal(code, body.Data!.Code);
        Assert.Equal(19.99m, body.Data.Price);
    }

    [Fact]
    public async Task CreateProduct_InvalidPrice_ReturnsErrorEnvelope()
    {
        await AuthenticateAsync();
        using var request = CreatePost(new { code = "BAD-PRICE", name = "X", price = 0 }, Guid.NewGuid().ToString("N"));

        var body = await (await _client.SendAsync(request))
            .Content.ReadFromJsonAsync<RestResponse<object>>(JsonOptions);

        Assert.False(body!.IsSuccess);
    }

    [Fact]
    public async Task CreateProduct_MissingName_ReturnsErrorEnvelope()
    {
        await AuthenticateAsync();
        using var request = CreatePost(new { code = "NO-NAME", price = 5m }, Guid.NewGuid().ToString("N"));

        var body = await (await _client.SendAsync(request))
            .Content.ReadFromJsonAsync<RestResponse<object>>(JsonOptions);

        Assert.False(body!.IsSuccess);
    }

    [Fact]
    public async Task CreateProduct_DuplicateCode_ReturnsErrorEnvelope()
    {
        await AuthenticateAsync();
        using var request = CreatePost(new { code = "AU-1OZ", name = "Dup", price = 1m }, Guid.NewGuid().ToString("N"));

        var body = await (await _client.SendAsync(request))
            .Content.ReadFromJsonAsync<RestResponse<ProductInfo>>(JsonOptions);

        Assert.False(body!.IsSuccess);
        Assert.Contains("AU-1OZ", body.ErrorMessage);
    }

    [Fact]
    public async Task CreateProduct_MissingIdempotencyKey_ReturnsErrorEnvelope()
    {
        await AuthenticateAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/products")
        {
            Content = new StringContent(
                """{"code":"NO-KEY","name":"X","price":1}""",
                Encoding.UTF8,
                "application/json")
        };

        var body = await (await _client.SendAsync(request))
            .Content.ReadFromJsonAsync<RestResponse<ProductInfo>>(JsonOptions);

        Assert.False(body!.IsSuccess);
        Assert.Contains("Idempotency-Key", body.ErrorMessage);
    }

    [Fact]
    public async Task CreateProduct_SameIdempotencyKey_ReplaysWithoutDuplicate()
    {
        await AuthenticateAsync();
        var key = Guid.NewGuid().ToString("N");
        var code = $"IDEM-{Guid.NewGuid():N}"[..16];
        var payload = new { code, name = "Idempotent", price = 7.5m };

        using var first = CreatePost(payload, key);
        using var second = CreatePost(payload, key);

        var firstBody = await (await _client.SendAsync(first))
            .Content.ReadFromJsonAsync<RestResponse<ProductInfo>>(JsonOptions);
        var secondResponse = await _client.SendAsync(second);
        var secondBody = await secondResponse.Content.ReadFromJsonAsync<RestResponse<ProductInfo>>(JsonOptions);

        Assert.True(firstBody!.IsSuccess);
        Assert.True(secondBody!.IsSuccess);
        Assert.Equal("true", secondResponse.Headers.GetValues("Idempotency-Replayed").Single());
        Assert.Equal(firstBody.Data!.Id, secondBody.Data!.Id);

        var list = await _client.GetFromJsonAsync<RestResponse<List<ProductInfo>>>("/api/products", JsonOptions);
        Assert.Equal(1, list!.Data!.Count(p => p.Code == code));
    }

    [Fact]
    public async Task CreateProduct_SameKeyDifferentPayload_ReturnsError()
    {
        await AuthenticateAsync();
        var key = Guid.NewGuid().ToString("N");

        using var first = CreatePost(new { code = $"A-{Guid.NewGuid():N}"[..12], name = "One", price = 1m }, key);
        Assert.True((await (await _client.SendAsync(first))
            .Content.ReadFromJsonAsync<RestResponse<ProductInfo>>(JsonOptions))!.IsSuccess);

        using var second = CreatePost(new { code = $"B-{Guid.NewGuid():N}"[..12], name = "Two", price = 2m }, key);
        var secondBody = await (await _client.SendAsync(second))
            .Content.ReadFromJsonAsync<RestResponse<ProductInfo>>(JsonOptions);

        Assert.False(secondBody!.IsSuccess);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsError()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@catalog.local",
            password = "wrong"
        });
        var body = await response.Content.ReadFromJsonAsync<RestResponse<TokenInfo>>(JsonOptions);

        Assert.False(body!.IsSuccess);
    }

    private async Task AuthenticateAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@catalog.local",
            password = "Admin123!"
        });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<RestResponse<TokenInfo>>(JsonOptions);
        Assert.True(body!.IsSuccess);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body.Data!.AccessToken);
    }

    private static HttpRequestMessage CreatePost(object body, string idempotencyKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/products")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        return request;
    }
}
