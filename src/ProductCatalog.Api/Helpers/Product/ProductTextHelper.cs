namespace ProductCatalog.Api.Helpers.Product;

public static class ProductTextHelper
{
    public static string NormalizeCode(string code) => code.Trim();

    public static string SanitizeName(string name) =>
        new string(name.Trim().Where(c => !char.IsControl(c)).ToArray());
}
