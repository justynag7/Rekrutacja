using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.Helpers.Auth;
using ProductCatalog.Api.Models.Api;
using ProductCatalog.Api.Models.Api.Auth;
using ProductCatalog.Api.Services.Interfaces.Auth;

namespace ProductCatalog.Api.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost]
    [AllowAnonymous]
    [Route(Routes.Login)]
    public RestResponse<TokenInfo> Login(LoginRequestInfo request)
    {
        var token = _tokenService.Login(request);
        if (token is null)
        {
            return RestResponse<TokenInfo>.CreateErrorResponse("Invalid email or password.");
        }

        return RestResponse<TokenInfo>.CreateSuccessResponse(token);
    }
}
