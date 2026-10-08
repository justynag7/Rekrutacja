using ProductCatalog.Api.Models.Identity;

namespace ProductCatalog.Api.Repositories.Interfaces;

public interface IUserRepository
{
    ApplicationUser? GetByEmail(string email);
}
