using IRoute.Commerce.Domain.Entities;

namespace IRoute.Commerce.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
}
