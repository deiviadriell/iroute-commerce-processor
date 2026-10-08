using IRoute.Commerce.Application.Abstractions;

namespace IRoute.Commerce.Application.Auth;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return LoginResult.Fail("Usuario y contraseña son requeridos.");

        var user = await _users.GetByUsernameAsync(username.Trim());

        if (user is null || !user.IsActive || !_passwordHasher.Verify(password, user.PasswordHash))
            return LoginResult.Fail("Usuario o contraseña incorrectos.");

        var token = _tokenService.Generate(user);
        return LoginResult.Ok(token, user.Username, user.FullName);
    }
}
