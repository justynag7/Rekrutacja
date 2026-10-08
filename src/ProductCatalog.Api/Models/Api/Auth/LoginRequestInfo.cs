using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Api.Models.Api.Auth;

public sealed class LoginRequestInfo
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
