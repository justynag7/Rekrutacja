using ProductCatalog.Api.Models.Api.Auth;

namespace ProductCatalog.Api.Services.Interfaces.Auth;

public interface ITokenService
{
    TokenInfo? Login(LoginRequestInfo request);
}
