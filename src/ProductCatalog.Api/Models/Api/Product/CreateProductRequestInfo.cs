using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Api.Models.Api.Product;


public sealed class CreateProductRequestInfo
{
    [Required(ErrorMessage = "Code is required.")]
    [StringLength(32, MinimumLength = 1, ErrorMessage = "Code must be 1–32 characters.")]
    [RegularExpression(@"^[A-Za-z0-9\-_.]+$", ErrorMessage = "Code may contain only letters, digits and - _ .")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be 1–200 characters.")]
    public string Name { get; set; } = string.Empty;


    [Range(0.01, 1_000_000, ErrorMessage = "Price must be greater than 0 and at most 1 000 000.")]
    public decimal Price { get; set; }
}
