namespace ProductCatalog.Api.Models.Api.Auth;

public sealed class TokenInfo
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}
