using IRoute.Commerce.Application.Auth;
using IRoute.Commerce.Domain.Entities;

namespace IRoute.Commerce.Application.Abstractions;

public interface ITokenService
{
    AuthToken Generate(User user);
}
