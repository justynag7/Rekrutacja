using System.Collections.Concurrent;
using ProductCatalog.Api.Models.Identity;
using ProductCatalog.Api.Repositories.Interfaces;

namespace ProductCatalog.Api.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, ApplicationUser> _users =
        new(StringComparer.OrdinalIgnoreCase);

    public ApplicationUser? GetByEmail(string email) =>
        _users.TryGetValue(email, out var user) ? user : null;

    public void Upsert(ApplicationUser user)
    {
        _users[user.Email] = user;
    }
}
