using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProductCatalog.Api.Models.Api.Auth;
using ProductCatalog.Api.Models.Configuration;
using ProductCatalog.Api.Models.Identity;
using ProductCatalog.Api.Repositories.Interfaces;
using ProductCatalog.Api.Services.Interfaces.Auth;

namespace ProductCatalog.Api.Services.Auth;

public sealed class TokenService : ITokenService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly JwtSettings _jwt;

    public TokenService(
        IUserRepository users,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IOptions<JwtSettings> jwt)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwt.Value;
    }

    public TokenInfo? Login(LoginRequestInfo request)
    {
        var user = _users.GetByEmail(request.Email.Trim());
        if (user is null)
        {
            return null;
        }

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var expires = DateTimeOffset.UtcNow.AddMinutes(_jwt.ExpirationMinutes);
        var token = CreateJwt(user, expires);

        return new TokenInfo
        {
            AccessToken = token,
            ExpiresAtUtc = expires,
            Email = user.Email,
            DisplayName = user.DisplayName
        };
    }

    private string CreateJwt(ApplicationUser user, DateTimeOffset expires)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
